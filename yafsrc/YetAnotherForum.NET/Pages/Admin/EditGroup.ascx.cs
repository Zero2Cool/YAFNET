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
    using System.Linq;
    using System.Web;
    using System.Web.UI.WebControls;

    using YAF.Core.BasePages;
    using YAF.Core.Extensions;
    using YAF.Core.Model;
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
    /// Interface for creating or editing user roles/groups.
    /// </summary>
    public partial class EditGroup : AdminPage
    {
        #region Methods

        /// <summary>
        /// Gets or sets the access masks list.
        /// </summary>
        public IList<AccessMask> AccessMasksList { get; set; }

        /// <summary>
        /// Handles data binding event of initial access masks dropdown control.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void BindDataAccessMaskId([NotNull] object sender, [NotNull] EventArgs e)
        {
            // We don't change access masks if it's a guest
            if (this.IsGuestX.Checked)
            {
                return;
            }

            // get sender object as dropdown list
            var c = (DropDownList)sender;

            // list all access masks as data source
            c.DataSource = this.AccessMasksList;

            // set value and text field names
            c.DataValueField = "ID";
            c.DataTextField = "Name";
        }

        /// <summary>
        /// Handles click on cancel button.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void CancelClick([NotNull] object sender, [NotNull] EventArgs e)
        {
            // go back to roles administration
            BuildLink.Redirect(ForumPages.Admin_Groups);
        }

        /// <summary>
        /// Creates page links for this page.
        /// </summary>
        protected override void CreatePageLinks()
        {
            // forum index
            this.PageLinks.AddRoot();

            // admin index
            this.PageLinks.AddAdminIndex();

            this.PageLinks.AddLink(
                this.GetText("ADMIN_GROUPS", "TITLE"),
                BuildLink.GetLink(ForumPages.Admin_Groups));

            // current page label (no link)
            this.PageLinks.AddLink(this.GetText("ADMIN_EDITGROUP", "TITLE"), string.Empty);

            this.Page.Header.Title =
                $"{this.GetText("ADMIN_ADMIN", "Administration")} - {this.GetText("ADMIN_GROUPS", "TITLE")} - {this.GetText("ADMIN_EDITGROUP", "TITLE")}";
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load([NotNull] object sender, [NotNull] EventArgs e)
        {
            this.PageContext.PageElements.RegisterJsBlockStartup(
                nameof(JavaScriptBlocks.FormValidatorJs),
                JavaScriptBlocks.FormValidatorJs(this.Save.ClientID));

            // this needs to be done just once, not during post-backs
            if (this.IsPostBack)
            {
                return;
            }

            // bind data
            this.BindData();

            // is this editing of existing role or creation of new one?
            if (!this.Get<HttpRequestBase>().QueryString.Exists("i"))
            {
                return;
            }

            // we are not creating new role
            this.NewGroupRow.Visible = false;

            // get data about edited role
            var row = this.GetRepository<Group>().GetById(
                this.Get<HttpRequestBase>().QueryString.GetFirstOrDefaultAs<int>("i"));

            // get role flags
            var flags = row.GroupFlags;

            // set controls to role values
            this.Name.Text = row.Name;

            this.IsAdminX.Checked = flags.IsAdmin;
            this.IsAdminX.Enabled = !flags.IsGuest;

            this.IsStartX.Checked = flags.IsStart;
            this.IsStartX.Enabled = !flags.IsGuest;

            this.IsModeratorX.Checked = flags.IsModerator;
            this.IsModeratorX.Enabled = !flags.IsGuest;

            this.PMLimit.Text = row.PMLimit.ToString();
            this.PMLimit.Enabled = !flags.IsGuest;

            this.StyleTextBox.Text = row.Style;

            this.Priority.Text = row.SortOrder.ToString();

            this.UsrAlbums.Text = row.UsrAlbums.ToString();
            this.UsrAlbums.Enabled = !flags.IsGuest;

            this.UsrAlbumImages.Text = row.UsrAlbumImages.ToString();
            this.UsrAlbumImages.Enabled = !flags.IsGuest;

            this.UsrSigChars.Text = row.UsrSigChars.ToString();
            this.UsrSigChars.Enabled = !flags.IsGuest;

            this.UsrSigBBCodes.Text = row.UsrSigBBCodes;
            this.UsrSigBBCodes.Enabled = !flags.IsGuest;

            this.UsrSigHTMLTags.Text = row.UsrSigHTMLTags;
            this.UsrSigHTMLTags.Enabled = !flags.IsGuest;

            this.Description.Text = row.Description;

            this.IsGuestX.Checked = flags.IsGuest;

            // IsGuest flag can be set for only one role. if it isn't for this, disable that row
            if (!flags.IsGuest)
            {
                return;
            }

            this.IsGuestTR.Visible = true;
            this.IsGuestX.Enabled = !flags.IsGuest;
            this.AccessList.Visible = false;
        }

        /// <summary>
        /// Saves the click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void SaveClick([NotNull] object sender, [NotNull] EventArgs e)
        {
            if (!ValidationHelper.IsValidInt(this.PMLimit.Text.Trim()))
            {
                this.PageContext.AddLoadMessage(
                    this.GetText("ADMIN_EDITGROUP", "MSG_VALID_NUMBER"),
                    MessageTypes.warning);
                return;
            }

            if (!ValidationHelper.IsValidInt(this.Priority.Text.Trim()))
            {
                this.PageContext.AddLoadMessage(this.GetText("ADMIN_EDITGROUP", "MSG_INTEGER"), MessageTypes.warning);
                return;
            }

            if (!ValidationHelper.IsValidInt(this.UsrAlbums.Text.Trim()))
            {
                this.PageContext.AddLoadMessage(
                    this.GetText("ADMIN_EDITGROUP", "MSG_ALBUM_NUMBER"),
                    MessageTypes.warning);
                return;
            }

            if (!ValidationHelper.IsValidInt(this.UsrSigChars.Text.Trim()))
            {
                this.PageContext.AddLoadMessage(
                    this.GetText("ADMIN_EDITGROUP", "MSG_SIG_NUMBER"),
                    MessageTypes.warning);
                return;
            }

            if (!ValidationHelper.IsValidInt(this.UsrAlbumImages.Text.Trim()))
            {
                this.PageContext.AddLoadMessage(
                    this.GetText("ADMIN_EDITGROUP", "MSG_TOTAL_NUMBER"),
                    MessageTypes.warning);
                return;
            }

            // Role
            int? roleId = null;

            // get role ID from page's parameter
            if (this.Get<HttpRequestBase>().QueryString.Exists("i"))
            {
                roleId = this.Get<HttpRequestBase>().QueryString.GetFirstOrDefaultAs<int>("i");
            }

            // get new and old name
            var roleName = this.Name.Text.Trim();
            var oldRoleName = string.Empty;

            // if we are editing existing role, get it's original name
            if (roleId.HasValue)
            {
                // get the current role name in the DB
                var groups = this.GetRepository<Group>().List(boardId: this.PageContext.PageBoardID);

                groups.ForEach(group => oldRoleName = group.Name);
            }

            var groupFlags = new GroupFlags
            {
                IsGuest = this.IsGuestX.Checked,
                IsAdmin = this.IsAdminX.Checked,
                IsModerator = this.IsModeratorX.Checked,
                IsStart = this.IsStartX.Checked
            };

            // save role and get its ID if it's new (if it's old role, we get it anyway)
            roleId = this.GetRepository<Group>().Save(
                roleId,
                this.PageContext.PageBoardID,
                roleName,
                groupFlags,
                this.AccessMaskID.SelectedValue.ToType<int>(),
                this.PMLimit.Text.Trim().ToType<int>(),
                this.StyleTextBox.Text.Trim(),
                this.Priority.Text.Trim().ToType<short>(),
                this.Description.Text,
                this.UsrSigChars.Text.ToType<int>(),
                this.UsrSigBBCodes.Text,
                this.UsrSigHTMLTags.Text,
                this.UsrAlbums.Text.Trim().ToType<int>(),
                this.UsrAlbumImages.Text.Trim().ToType<int>());

            // see if need to rename an existing role...
            if (oldRoleName.IsSet() && roleName != oldRoleName && this.Get<IAspNetRolesHelper>().RoleExists(oldRoleName)
                && !this.Get<IAspNetRolesHelper>().RoleExists(roleName) && !this.IsGuestX.Checked)
            {
                // transfer users in addition to changing the name of the role...
                var users = this.Get<IAspNetRolesHelper>().GetUsersInRole(oldRoleName);

                // delete the old role...
                this.Get<IAspNetRolesHelper>().DeleteRole(oldRoleName);

                // create new role...
                this.Get<IAspNetRolesHelper>().CreateRole(roleName);

                if (users.Any())
                {
                    // put users into new role...
                    users.ForEach(user => this.Get<IAspNetRolesHelper>().AddUserToRole(user, roleName));
                }
            }
            else if (!this.Get<IAspNetRolesHelper>().RoleExists(roleName) && !this.IsGuestX.Checked)
            {
                // if role doesn't exist in provider's data source, create it

                // simply create it
                this.Get<IAspNetRolesHelper>().CreateRole(roleName);
            }

            // Access masks for a newly created or an existing role
            if (this.Get<HttpRequestBase>().QueryString.Exists("i"))
            {
                // go through all forums
                for (var i = 0; i < this.AccessList.Items.Count; i++)
                {
                    // get current repeater item
                    var item = this.AccessList.Items[i];

                    // get forum ID
                    var forumId = int.Parse(item.FindControlAs<HiddenField>("ForumID").Value);

                    // save forum access masks for this role
                    this.GetRepository<ForumAccess>().Save(
                        forumId,
                        roleId.ToType<int>(),
                        item.FindControlAs<DropDownList>("AccessmaskID").SelectedValue.ToType<int>());
                }

                BuildLink.Redirect(ForumPages.Admin_Groups);
            }

            // Done, redirect to role editing page
            BuildLink.Redirect(ForumPages.Admin_EditGroup, "i={0}", roleId);
        }

        /// <summary>
        /// Handles the ItemCreated event of the ForumList1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void AccessList_OnItemCreated([NotNull] object sender, [NotNull] RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            var item = ((int ForumID, string ForumName, int? ParentID, int AccessMaskID))e.Item.DataItem;

            var label = e.Item.FindControlAs<Label>("AccessMask");
            label.Text = item.ForumName;

            var hiddenField = e.Item.FindControlAs<HiddenField>("ForumID");

            hiddenField.Value = item.ForumID.ToString();

            var list = e.Item.FindControlAs<DropDownList>("AccessMaskID");

            // We don't change access masks if it's a guest
            if (this.IsGuestX.Checked)
            {
                return;
            }

            // list all access masks as data source
            list.DataSource = this.AccessMasksList;

            // set value and text field names
            list.DataValueField = "ID";
            list.DataTextField = "Name";
            list.DataBind();

            // select value from the list
            var foundItem = list.Items.FindByValue(item.AccessMaskID.ToString());

            // verify something was found...
            if (foundItem != null)
            {
                foundItem.Selected = true;
            }
        }

        /// <summary>
        /// Bind data for this control.
        /// </summary>
        private void BindData()
        {
            // set data source of access list (list of forums and role's access masks) if we are editing existing mask
            if (this.Get<HttpRequestBase>().QueryString.Exists("i"))
            {
                this.AccessList.DataSource = this.GetRepository<ForumAccess>()
                    .ListByGroups(this.Get<HttpRequestBase>().QueryString.GetFirstOrDefaultAs<int>("i"));
            }

            this.AccessMasksList = this.GetRepository<AccessMask>().GetByBoardId();

            // bind data to controls
            this.DataBind();
        }

        #endregion
    }
}