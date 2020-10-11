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

    using YAF.Configuration;
    using YAF.Core.BasePages;
    using YAF.Core.Extensions;
    using YAF.Core.Model;
    using YAF.Core.Utilities;
    using YAF.Types;
    using YAF.Types.Constants;
    using YAF.Types.Extensions;
    using YAF.Types.Flags;
    using YAF.Types.Interfaces;
    using YAF.Types.Models;
    using YAF.Types.Objects;
    using YAF.Utils;
    using YAF.Utils.Helpers;
    using YAF.Web.Extensions;

    #endregion

    /// <summary>
    /// Administration Page to Add/Edit Medals
    /// </summary>
    public partial class EditMedal : AdminPage
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "EditMedal" /> class. 
        ///   Default constructor.
        /// </summary>
        public EditMedal()
            : base("ADMIN_EDITMEDAL")
        {
        }

        #endregion

        /// <summary>
        /// Gets the current medal identifier.
        /// </summary>
        /// <value>
        /// The current medal identifier.
        /// </value>
        protected int? CurrentMedalId =>
            this.Get<HttpRequestBase>().QueryString.Exists("medalid")
                ? this.Get<HttpRequestBase>().QueryString.GetFirstOrDefaultAs<int?>("medalid")
                : null;

        #region Methods

        /// <summary>
        /// Handles click on add group button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Shows user-medal adding/editing controls.
        /// </remarks>
        protected void AddGroupClick([NotNull] object sender, [NotNull] EventArgs e)
        {
            this.GroupEditDialog.BindData(null, this.CurrentMedalId);

            this.PageContext.PageElements.RegisterJsBlockStartup(
                "openModalJs",
                JavaScriptBlocks.OpenModalJs("GroupEditDialog"));
        }

        /// <summary>
        /// Handles click on add user button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Shows user-medal adding/editing controls.
        /// </remarks>
        protected void AddUserClick([NotNull] object sender, [NotNull] EventArgs e)
        {
            this.UserEditDialog.BindData(null, this.CurrentMedalId);

            this.PageContext.PageElements.RegisterJsBlockStartup(
                "openModalJs",
                JavaScriptBlocks.OpenModalJs("UserEditDialog"));
        }

        /// <summary>
        /// Handles click on cancel button.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void CancelClick([NotNull] object sender, [NotNull] EventArgs e)
        {
            // go back to medals administration
            BuildLink.Redirect(ForumPages.Admin_Medals);
        }

        /// <summary>
        /// Creates page links for this page.
        /// </summary>
        protected override void CreatePageLinks()
        {
            // forum index
            this.PageLinks.AddRoot();

            // administration index
            this.PageLinks.AddAdminIndex();

            this.PageLinks.AddLink(
                this.GetText("ADMIN_MEDALS", "TITLE"),
                BuildLink.GetLink(ForumPages.Admin_Medals));

            // current page label (no link)
            this.PageLinks.AddLink(this.GetText("ADMIN_EDITMEDAL", "TITLE"), string.Empty);

            this.Page.Header.Title =
                $"{this.GetText("ADMIN_ADMIN", "Administration")} - {this.GetText("ADMIN_MEDALS", "TITLE")} - {this.GetText("ADMIN_EDITMEDAL", "TITLE")}";
        }

        /// <summary>
        /// Creates link to group editing admin interface.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The format group link.
        /// </returns>
        protected string FormatGroupLink([NotNull] object data)
        {
            var dr = (Tuple<Medal, GroupMedal, Group>)data;

            return string.Format(
                "<a href=\"{1}\">{0}</a>",
                dr.Item3.Name,
                BuildLink.GetLink(ForumPages.Admin_EditGroup, "i={0}", dr.Item3.ID));
        }

        /// <summary>
        /// Creates link to user editing admin interface.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The format user link.
        /// </returns>
        protected string FormatUserLink([NotNull] object data)
        {
            var dr = (Tuple<Medal, UserMedal, User>)data;

            return string.Format(
                "<a href=\"{2}\">{0}&nbsp;({1})</a>",
                this.HtmlEncode(dr.Item3.DisplayName),
                this.HtmlEncode(dr.Item3.Name),
                BuildLink.GetLink(ForumPages.Admin_EditUser, "u={0}", dr.Item3.ID));
        }

        /// <summary>
        /// Handles click on GroupList repeaters item command link button.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void GroupListItemCommand([NotNull] object source, [NotNull] RepeaterCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "edit":
                    this.GroupEditDialog.BindData(e.CommandArgument.ToType<int>(), this.CurrentMedalId);

                    this.PageContext.PageElements.RegisterJsBlockStartup(
                        "openModalJs",
                        JavaScriptBlocks.OpenModalJs("GroupEditDialog"));
                    break;
                case "remove":
                    this.GetRepository<GroupMedal>().Delete(
                        medal => medal.GroupID == e.CommandArgument.ToType<int>()
                                 && medal.MedalID == this.CurrentMedalId);

                    // remove all user medals...
                    this.RemoveMedalsFromCache();

                    this.BindData();
                    break;
            }
        }

        /// <summary>
        /// Handles page load event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load([NotNull] object sender, [NotNull] EventArgs e)
        {
            this.PageContext.PageElements.RegisterJsBlockStartup(
                nameof(JavaScriptBlocks.FormValidatorJs),
                JavaScriptBlocks.FormValidatorJs(this.Save.ClientID));

            // this needs to be done just once, not during post-backs
            if (!this.IsPostBack)
            {
                // bind data
                this.BindData();
            }
        }

        /// <summary>
        /// Removals all medals from the cache...
        /// </summary>
        protected void RemoveMedalsFromCache()
        {
            // remove all user medals...
            this.Get<IDataCache>().Remove(k => k.StartsWith(string.Format(Constants.Cache.UserMedals, string.Empty)));
        }

        /// <summary>
        /// Handles save button click.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void SaveClick([NotNull] object sender, [NotNull] EventArgs e)
        {
            if (this.MedalImage.SelectedIndex <= 0)
            {
                this.PageContext.AddLoadMessage(this.GetText("ADMIN_EDITMEDAL", "MSG_IMAGE"), MessageTypes.warning);
                return;
            }

            if (this.SmallMedalImage.SelectedIndex <= 0)
            {
                this.PageContext.AddLoadMessage(
                    this.GetText("ADMIN_EDITMEDAL", "MSG_SMALL_IMAGE"),
                    MessageTypes.warning);
                return;
            }

            // data
            string ribbonUrl = null, smallRibbonUrl = null;
            
            // flags
            var flags = new MedalFlags(0)
                            {
                                ShowMessage = this.ShowMessage.Checked,
                                AllowRibbon = this.AllowRibbon.Checked,
                                AllowReOrdering = false,
                                AllowHiding = this.AllowHiding.Checked
                            };

            // get medal images
            var imageUrl = this.MedalImage.SelectedItem.Text;
            var smallImageUrl = this.SmallMedalImage.SelectedItem.Text;
            if (this.RibbonImage.SelectedIndex > 0)
            {
                ribbonUrl = this.RibbonImage.SelectedItem.Text;
            }

            if (this.SmallRibbonImage.SelectedIndex > 0)
            {
                smallRibbonUrl = this.SmallRibbonImage.SelectedItem.Text;
            }

            // save medal
            this.GetRepository<Medal>().Save(
                this.CurrentMedalId,
                this.Name.Text,
                this.Description.Text,
                this.Message.Text,
                this.Category.Text,
                imageUrl,
                ribbonUrl,
                smallImageUrl,
                smallRibbonUrl,
                flags.BitValue);

            // go back to medals administration
            BuildLink.Redirect(ForumPages.Admin_Medals);
        }

        /// <summary>
        /// Handles click on UserList repeaters item command link button.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void UserListItemCommand([NotNull] object source, [NotNull] RepeaterCommandEventArgs e)
        {
            switch (e.CommandName)
            {
                case "edit":
                    this.UserEditDialog.BindData(e.CommandArgument.ToType<int>(), this.CurrentMedalId);

                    this.PageContext.PageElements.RegisterJsBlockStartup(
                        "openModalJs",
                        JavaScriptBlocks.OpenModalJs("UserEditDialog"));
                    break;
                case "remove":

                    // delete user-medal
                    this.GetRepository<UserMedal>().Delete(
                        medal => medal.UserID == e.CommandArgument.ToType<int>()
                                 && medal.MedalID == this.CurrentMedalId);

                    // clear cache...
                    this.Get<IDataCache>().Remove(
                        string.Format(Constants.Cache.UserMedals, e.CommandArgument.ToType<int>()));
                    this.BindData();
                    break;
            }
        }

        /// <summary>
        /// Registers the needed Java Scripts
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnPreRender([NotNull] EventArgs e)
        {
            base.OnPreRender(e);
        }

        /// <summary>
        /// Select image in dropdown list and sets appropriate preview.
        /// </summary>
        /// <param name="list">
        /// DropDownList where to search.
        /// </param>
        /// <param name="imageUrl">
        /// URL to search for.
        /// </param>
        private static void SelectImage([NotNull] ListControl list, [NotNull] string imageUrl)
        {
            // try to find item in a list
            var item = list.Items.FindByText(imageUrl);

            if (item != null)
            {
                // select found item
                item.Selected = true;
            }
        }

        /// <summary>
        /// Bind data for this control.
        /// </summary>
        private void BindData()
        {
            // load available images from images/medals folder
            var medals = new List<NamedParameter>
            {
                new NamedParameter(
                    this.GetText("ADMIN_EDITMEDAL", "SELECT_IMAGE"),
                    BoardInfo.GetURLToContent("images/spacer.gif"))
            };


            // add files from medals folder
            var dir = new DirectoryInfo(
                this.Get<HttpRequestBase>().MapPath($"{BoardInfo.ForumServerFileRoot}{BoardFolders.Current.Medals}"));
            var files = dir.GetFiles("*.*").ToList();

            medals.AddImageFiles(files, BoardFolders.Current.Medals);

            // medal image
            this.MedalImage.DataSource = medals;
            this.MedalImage.DataValueField = "Value";
            this.MedalImage.DataTextField = "Name";

            // ribbon bar image
            this.RibbonImage.DataSource = medals;
            this.RibbonImage.DataValueField = "Value";
            this.RibbonImage.DataTextField = "Name";

            // small medal image
            this.SmallMedalImage.DataSource = medals;
            this.SmallMedalImage.DataValueField = "Value";
            this.SmallMedalImage.DataTextField = "Name";

            // small ribbon bar image
            this.SmallRibbonImage.DataSource = medals;
            this.SmallRibbonImage.DataValueField = "Value";
            this.SmallRibbonImage.DataTextField = "Name";

            // bind data to controls
            this.DataBind();

            if (!this.CurrentMedalId.HasValue)
            {
                // Hide user & group
                this.UserAndGroupsHolder.Visible = false;

                return;
            }

            // load users and groups who has been assigned this medal
            this.UserList.DataSource = this.GetRepository<UserMedal>().List(null, this.CurrentMedalId.Value);
            this.UserList.DataBind();

            this.GroupList.DataSource = this.GetRepository<GroupMedal>().List(null, this.CurrentMedalId.Value);
            this.GroupList.DataBind();

            var medal = this.GetRepository<Medal>().GetSingle(m => m.ID == this.CurrentMedalId.Value);

            // set controls
            this.Name.Text = medal.Name;
            this.Description.Text = medal.Description;
            this.Message.Text = medal.Message;
            this.Category.Text = medal.Category;
            this.ShowMessage.Checked = medal.MedalFlags.ShowMessage;
            this.AllowRibbon.Checked = medal.MedalFlags.AllowRibbon;
            this.AllowHiding.Checked = medal.MedalFlags.AllowHiding;

            // select images
            SelectImage(this.MedalImage, medal.MedalURL);
            SelectImage(this.RibbonImage, medal.RibbonURL);
            SelectImage(this.SmallMedalImage, medal.SmallMedalURL);
            SelectImage(this.SmallRibbonImage, medal.SmallRibbonURL);
        }

        #endregion
    }
}