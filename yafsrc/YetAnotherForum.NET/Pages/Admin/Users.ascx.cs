/* Yet Another Forum.NET
 * Copyright (C) 2003-2005 Bjørnar Henden
 * Copyright (C) 2006-2013 Jaben Cargman
 * Copyright (C) 2014-2020 Ingo Herbote
 * https://www.yetanotherforum.net/
 *
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at

 * https://www.apache.org/licenses/LICENSE-2.0

 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

namespace YAF.Pages.Admin
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Web;
    using System.Web.UI.WebControls;
    using System.Xml.Serialization;

    using ServiceStack;

    using YAF.Configuration;
    using YAF.Core.BasePages;
    using YAF.Core.Extensions;
    using YAF.Core.Helpers;
    using YAF.Core.Model;
    using YAF.Core.Tasks;
    using YAF.Core.Utilities;
    using YAF.Types;
    using YAF.Types.Constants;
    using YAF.Types.Extensions;
    using YAF.Types.Flags;
    using YAF.Types.Interfaces;
    using YAF.Types.Interfaces.Identity;
    using YAF.Types.Models;
    using YAF.Utils;
    using YAF.Utils.Helpers;
    using YAF.Web.Extensions;

    #endregion

    /// <summary>
    /// Admin Members Page.
    /// </summary>
    public partial class Users : AdminPage
    {
        #region Public Methods

        /// <summary>
        /// Redirects to the Create User Page
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void NewUserClick([NotNull] object sender, [NotNull] EventArgs e)
        {
            // redirect to create new user page
            BuildLink.Redirect(ForumPages.Admin_RegisterUser);
        }

        /// <summary>
        /// The user list_ item command.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterCommandEventArgs"/> instance containing the event data.</param>
        public void UserListItemCommand([NotNull] object source, [NotNull] RepeaterCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "edit":

                    // we are going to edit user - redirect to edit page
                    BuildLink.Redirect(ForumPages.Admin_EditUser, "u={0}", e.CommandArgument);
                    break;
                case "delete":

                    // we are deleting user
                    if (this.PageContext.PageUserID == int.Parse(e.CommandArgument.ToString()))
                    {
                        // deleting yourself isn't an option
                        this.PageContext.AddLoadMessage(
                            this.GetText("ADMIN_USERS", "MSG_SELF_DELETE"),
                            MessageTypes.danger);
                        return;
                    }

                    // get user(s) we are about to delete
                    var userToDelete = this.GetRepository<User>().GetBoardUser(
                        e.CommandArgument.ToType<int>(),
                        this.PageContext.PageBoardID);
                    
                    if (userToDelete.Item1.IsGuest == true)
                    {
                        // we cannot delete guest
                        this.PageContext.AddLoadMessage(
                            this.GetText("ADMIN_USERS", "MSG_DELETE_GUEST"),
                            MessageTypes.danger);
                        return;
                    }

                    if (userToDelete.Item4.IsAdmin || userToDelete.Item1.UserFlags.IsHostAdmin)
                    {
                        // admin are not deletable either
                        this.PageContext.AddLoadMessage(
                            this.GetText("ADMIN_USERS", "MSG_DELETE_ADMIN"),
                            MessageTypes.danger);
                        return;
                    }

                    // all is good, user can be deleted
                    this.Get<IAspNetUsersHelper>().DeleteUser(e.CommandArgument.ToType<int>());

                    // rebind data
                    this.BindData();

                    // quit case
                    break;
            }
        }

        /// <summary>
        /// The search_ click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        public void SearchClick([NotNull] object sender, [NotNull] EventArgs e)
        {
            // re-bind data
            this.BindData();
        }

        /// <summary>
        /// Handles the Click event of the Reset control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        public void Reset_Click([NotNull] object sender, [NotNull] EventArgs e)
        {
            // re-direct to self.
            BuildLink.Redirect(ForumPages.Admin_Users);
        }

        /// <summary>
        /// Export all Users as CSV
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void ExportUsersCsvClick(object sender, EventArgs e)
        {
            this.ExportAllUsers("csv");
        }

        /// <summary>
        /// Export all Users as XML
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void ExportUsersXmlClick(object sender, EventArgs e)
        {
            this.ExportAllUsers("xml");
        }

        /// <summary>
        /// Gets the suspended string.
        /// </summary>
        /// <param name="suspendedUntil">The suspended until.</param>
        /// <returns>Returns the suspended string</returns>
        protected string GetSuspendedString(string suspendedUntil)
        {
            return suspendedUntil.IsNotSet()
                       ? this.GetText("COMMON", "NO")
                       : this.GetTextFormatted(
                           "USERSUSPENDED",
                           this.Get<IDateTime>().FormatDateTime(suspendedUntil.ToType<DateTime>()));
        }

        /// <summary>
        /// The get is user disabled label.
        /// </summary>
        /// <param name="userFlag">
        /// The user Flag.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected string GetIsUserDisabledLabel(object userFlag)
        {
            var flag = new UserFlags((int)userFlag);
            return flag.IsApproved
                       ? string.Empty
                       : $@"<span class=""badge bg-warning text-dark"">{this.GetText("DISABLED")}</span>";
        }

        #endregion

        #region Methods

        /// <summary>
        /// Registers the needed Java Scripts
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnPreRender([NotNull] EventArgs e)
        {
            this.PageContext.PageElements.RegisterJsBlockStartup(
                "BlockUIExecuteJs",
                JavaScriptBlocks.BlockUiExecuteJs("SyncUsersMessage", $"#{this.SyncUsers.ClientID}"));

            // setup jQuery and Jquery Ui Tabs.
            this.PageContext.PageElements.RegisterJsBlock("dropDownToggleJs", JavaScriptBlocks.DropDownToggleJs());

            base.OnPreRender(e);
        }

        /// <summary>
        /// Creates navigation page links on top of forum (breadcrumbs).
        /// </summary>
        protected override void CreatePageLinks()
        {
            // link to board index
            this.PageLinks.AddRoot();

            // link to administration index
            this.PageLinks.AddAdminIndex();

            // current page label (no link)
            this.PageLinks.AddLink(this.GetText("ADMIN_USERS", "TITLE"), string.Empty);

            this.Page.Header.Title =
                $"{this.GetText("ADMIN_ADMIN", "Administration")} - {this.GetText("ADMIN_USERS", "TITLE")}";
        }

        /// <summary>
        /// Initializes dropdown with options to filter results by date.
        /// </summary>
        protected void InitSinceDropdown()
        {
            var lastVisit = this.Get<ISession>().LastVisit;

            // value 0, for since last visit
            this.Since.Items.Add(
                new ListItem(
                    this.GetTextFormatted(
                        "last_visit",
                        this.Get<IDateTime>().FormatDateTime(
                            lastVisit.HasValue && lastVisit.Value != DateTimeHelper.SqlDbMinTime()
                                ? lastVisit.Value
                                : DateTime.UtcNow)),
                    "0"));

            // negative values for hours backward
            this.Since.Items.Add(new ListItem("Last hour", "-1"));
            this.Since.Items.Add(new ListItem("Last two hours", "-2"));

            // positive values for days backward
            this.Since.Items.Add(new ListItem("Last day", "1"));
            this.Since.Items.Add(new ListItem("Last two days", "2"));
            this.Since.Items.Add(new ListItem("Last week", "7"));
            this.Since.Items.Add(new ListItem("Last two weeks", "14"));
            this.Since.Items.Add(new ListItem("Last month", "31"));

            // all time (i.e. no filter)
            this.Since.Items.Add(new ListItem("All time", "9999"));
        }

        /// <summary>
        /// The page_ load.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load([NotNull] object sender, [NotNull] EventArgs e)
        {
            if (this.IsPostBack)
            {
                return;
            }

            if (Config.IsAnyPortal)
            {
                this.ImportUsers.Visible = false;
                this.SyncUsers.Visible = false;
            }

            // initialize since filter items
            this.InitSinceDropdown();

            // set since filter to last item "All time"
            this.Since.SelectedIndex = this.Since.Items.Count - 1;

            // get list of user groups for filtering
            var groups = this.GetRepository<Group>().List(boardId: this.PageContext.PageBoardID);

            groups.Insert(0, new Group { Name = this.GetText("FILTER_NO"), ID = 0 });

            this.group.DataTextField = "Name";
            this.group.DataValueField = "ID";
            this.group.DataSource = groups;

            this.group.DataBind();

            // get list of user ranks for filtering
            var ranks = this.GetRepository<Rank>().GetByBoardId().OrderBy(r => r.SortOrder).ToList();

            // add empty for for no filtering
            ranks.Insert(0, new Rank { Name = this.GetText("FILTER_NO"), ID = 0 });

            this.rank.DataSource = ranks;
            this.rank.DataTextField = "Name";
            this.rank.DataValueField = "ID";
            this.rank.DataBind();

            this.PageSize.DataSource = StaticDataHelper.PageEntries();
            this.PageSize.DataTextField = "Name";
            this.PageSize.DataValueField = "Value";
            this.PageSize.DataBind();

            this.PagerTop.PageSize = this.PageSize.SelectedValue.ToType<int>();

            // Hide "New User" & Sync Button on DotNetNuke
            this.ImportAndSyncHolder.Visible = !Config.IsDotNetNuke;

            this.BindData();
        }

        /// <summary>
        /// The page size on selected index changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void PageSizeSelectedIndexChanged(object sender, EventArgs e)
        {
            this.BindData();
        }

        /// <summary>
        /// The pager top_ page change.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void PagerTopPageChange([NotNull] object sender, [NotNull] EventArgs e)
        {
            // rebind
            this.BindData();
        }

        /// <summary>
        /// The sync users_ click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SyncUsersClick([NotNull] object sender, [NotNull] EventArgs e)
        {
            // start...
            SyncMembershipUsersTask.Start(this.PageContext.PageBoardID);

            // enable timer...
            this.UpdateStatusTimer.Enabled = true;
        }

        /// <summary>
        /// The update status timer_ tick.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void UpdateStatusTimerTick([NotNull] object sender, [NotNull] EventArgs e)
        {
            // see if the migration is done...
            if (this.Get<ITaskModuleManager>().IsTaskRunning(SyncMembershipUsersTask.TaskName))
            {
                // continue...
                return;
            }

            this.UpdateStatusTimer.Enabled = false;

            // done here...
            BuildLink.Redirect(ForumPages.Admin_Users);
        }

        /// <summary>
        /// The lock accounts click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void LockAccountsClick(object sender, EventArgs e)
        {
            this.Get<IAspNetUsersHelper>().LockInactiveAccounts(DateTime.UtcNow.AddYears(-this.YearsOld.Text.ToType<int>()));
        }

        /// <summary>
        /// The bind data.
        /// </summary>
        private void BindData()
        {
            this.SearchResults.Visible = true;

            // default since date is now
            DateTime? sinceDate = null;

            // default since option is "since last visit"
            var sinceValue = 0;

            // is any "since"option selected
            if (this.Since.SelectedItem != null)
            {
                // get selected value
                sinceValue = int.Parse(this.Since.SelectedItem.Value);

                if (sinceValue > 0)
                {
                    // days
                    // get posts newer then defined number of days
                    sinceDate = DateTime.UtcNow - TimeSpan.FromDays(sinceValue);
                }
                else if (sinceValue < 0)
                {
                    // hours
                    // get posts newer then defined number of hours
                    sinceDate = DateTime.UtcNow + TimeSpan.FromHours(sinceValue);
                }
            }

            // we want to filter topics since last visit
            if (sinceValue == 0)
            {
                sinceDate = this.Get<ISession>().LastVisit ?? DateTime.UtcNow;
            }

            // get users, eventually filter by groups or ranks
            var users = this.GetRepository<User>().GetUsersPaged(
                this.PageContext.PageBoardID,
                this.PagerTop.CurrentPageIndex,
                this.PagerTop.PageSize,
                this.name.Text.Trim(),
                this.Email.Text.Trim(), 
                sinceDate,
                this.SuspendedOnly.Checked,
                this.group.SelectedIndex <= 0 ? null : this.group.SelectedValue.ToType<int?>(),
                this.rank.SelectedIndex <= 0 ? null : this.rank.SelectedValue.ToType<int?>());

            this.PagerTop.Count = users.FirstOrDefault().TotalRows;

            // bind list
            this.UserList.DataSource = users;
            this.UserList.DataBind();

            this.NoInfo.Visible = this.UserList.Items.Count == 0;
        }

        /// <summary>
        /// Export All Users
        /// </summary>
        /// <param name="type">
        /// The export format type.
        /// </param>
        private void ExportAllUsers(string type)
        {
            var usersList = this.GetRepository<User>().GetByBoardId(this.PageContext.PageBoardID);

            switch (type)
            {
                case "xml":
                    this.ExportAsXml(usersList);
                    break;
                case "csv":
                    this.ExportAsCsv(usersList);
                    break;
            }
        }

        /// <summary>
        /// Export As CSV
        /// </summary>
        /// <param name="usersList">
        /// The users list.
        /// </param>
        private void ExportAsCsv(IList<User> usersList)
        {
            this.Get<HttpResponseBase>().ContentType = "application/vnd.csv";

            this.Get<HttpResponseBase>().AppendHeader(
                "Content-Disposition",
                $"attachment; filename=YafUsersExport-{HttpUtility.UrlEncode(DateTime.Now.ToString("yyyy'-'MM'-'dd'-'HHmm"))}.csv");

            var sw = new StreamWriter(this.Get<HttpResponseBase>().OutputStream);

            sw.Write(usersList.ToCsv());
            sw.Close();

            this.Get<HttpResponseBase>().Flush();
            this.Get<HttpResponseBase>().End();
        }

        /// <summary>
        /// Export As Xml
        /// </summary>
        /// <param name="usersList">
        /// The users list.
        /// </param>
        private void ExportAsXml(IList<User> usersList)
        {
            this.Get<HttpResponseBase>().ContentType = "text/xml";

            this.Get<HttpResponseBase>().AppendHeader(
                "Content-Disposition",
                $"attachment; filename=YafUsersExport-{HttpUtility.UrlEncode(DateTime.Now.ToString("yyyy'-'MM'-'dd'-'HHmm"))}.xml");

            var xmlSerializer = new XmlSerializer(typeof(List<User>));
            xmlSerializer.Serialize(this.Get<HttpResponseBase>().OutputStream, usersList);

            this.Get<HttpResponseBase>().Flush();
            this.Get<HttpResponseBase>().End();
        }

        #endregion
    }
}