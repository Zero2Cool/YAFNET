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

namespace YAF.Controls
{
    #region Using

    using System;
    using System.Linq;
    using System.Web;

    using YAF.Configuration;
    using YAF.Core.BaseControls;
    using YAF.Core.Extensions;
    using YAF.Core.Helpers;
    using YAF.Core.Model;
    using YAF.Core.Utilities;
    using YAF.Types;
    using YAF.Types.Constants;
    using YAF.Types.EventProxies;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Types.Interfaces.Events;
    using YAF.Types.Interfaces.Identity;
    using YAF.Types.Models;
    using YAF.Types.Models.Identity;
    using YAF.Utils;
    using YAF.Utils.Helpers;

    #endregion

    /// <summary>
    /// The edit users settings.
    /// </summary>
    public partial class EditUsersSettings : BaseUserControl
    {
        #region Constants and Fields

        /// <summary>
        /// The current user id.
        /// </summary>
        private int currentUserId;

        /// <summary>
        /// The user.
        /// </summary>
        private Tuple<User, AspNetUsers, Rank, vaccess> user;

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether UpdateEmailFlag.
        /// </summary>
        protected bool UpdateEmailFlag
        {
            get => this.ViewState["bUpdateEmail"] != null && this.ViewState["bUpdateEmail"].ToType<bool>();

            set => this.ViewState["bUpdateEmail"] = value;
        }

        /// <summary>
        /// Gets the User Data.
        /// </summary>
        [NotNull]
        private Tuple<User, AspNetUsers, Rank, vaccess> User => this.user ??= this.GetRepository<User>().GetBoardUser(this.currentUserId);

        #endregion

        #region Methods

        /// <summary>
        /// Handles the Click event of the Cancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void CancelClick([NotNull] object sender, [NotNull] EventArgs e)
        {
            BuildLink.Redirect(
                this.PageContext.CurrentForumPage.IsAdminPage ? ForumPages.Admin_Users : ForumPages.MyAccount);
        }

        /// <summary>
        /// Handles the TextChanged event of the Email control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void EmailTextChanged([NotNull] object sender, [NotNull] EventArgs e)
        {
            this.UpdateEmailFlag = true;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender([NotNull] EventArgs e)
        {
            this.PageContext.PageElements.RegisterJsBlockStartup(
                "DatePickerJs",
                JavaScriptBlocks.DatePickerLoadJs(
                    this.GetText("COMMON", "CAL_JQ_CULTURE_DFORMAT"),
                    this.GetText("COMMON", "CAL_JQ_CULTURE")));

            base.OnPreRender(e);
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load([NotNull] object sender, [NotNull] EventArgs e)
        {
            if (this.PageContext.CurrentForumPage.IsAdminPage && this.PageContext.IsAdmin
                                                              && this.Get<HttpRequestBase>().QueryString.Exists("u"))
            {
                this.currentUserId = Security.StringToIntOrRedirect(this.Get<HttpRequestBase>().QueryString.GetFirstOrDefault("u"));
            }
            else
            {
                this.currentUserId = this.PageContext.PageUserID;
            }

            if (this.IsPostBack)
            {
                return;
            }

            this.LoginInfo.Visible = true;

            // End Modifications for enhanced profile
            this.ForumSettingsRows.Visible = this.Get<BoardSettings>().AllowUserTheme
                                             || this.Get<BoardSettings>().AllowUserLanguage;

            this.UserThemeRow.Visible = this.Get<BoardSettings>().AllowUserTheme;
            this.UserLanguageRow.Visible = this.Get<BoardSettings>().AllowUserLanguage;
            this.LoginInfo.Visible = this.Get<BoardSettings>().AllowEmailChange;

            // override Place Holders for DNN, dnn users should only see the forum settings but not the profile page
            if (Config.IsDotNetNuke)
            {
                this.LoginInfo.Visible = false;
            }

            this.BindData();
        }

        /// <summary>
        /// Saves the Updated Profile
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void UpdateProfileClick([NotNull] object sender, [NotNull] EventArgs e)
        {
            if (this.UpdateEmailFlag)
            {
                var newEmail = this.Email.Text.Trim();

                if (!ValidationHelper.IsValidEmail(newEmail))
                {
                    this.PageContext.AddLoadMessage(this.GetText("PROFILE", "BAD_EMAIL"), MessageTypes.warning);
                    return;
                }

                var userFromEmail = this.Get<IAspNetUsersHelper>().GetUserByEmail(this.Email.Text.Trim());

                if (userFromEmail != null && userFromEmail.Email != this.User.Item1.Name)
                {
                    this.PageContext.AddLoadMessage(this.GetText("PROFILE", "BAD_EMAIL"), MessageTypes.warning);
                    return;
                }

                try
                {
                    this.Get<IAspNetUsersHelper>().UpdateEmail(userFromEmail, this.Email.Text.Trim());
                }
                catch (ApplicationException)
                {
                    this.PageContext.AddLoadMessage(
                        this.GetText("PROFILE", "DUPLICATED_EMAIL"),
                        MessageTypes.warning);

                    return;
                }
            }

            // vzrus: We should do it as we need to write null value to db, else it will be empty.
            // Localizer currently treats only nulls.
            object language = null;
            object culture = this.Culture.SelectedValue;
            object theme = this.Theme.SelectedValue;

            if (this.Theme.SelectedValue.IsNotSet())
            {
                theme = null;
            }

            if (this.Culture.SelectedValue.IsNotSet())
            {
                culture = null;
            }
            else
            {
                StaticDataHelper.Cultures()
                    .Where(row => culture.ToString() == row.CultureTag).ForEach(
                        row => language = row.CultureFile);
            }

            // save remaining settings to the DB
            this.GetRepository<User>().Save(
                this.currentUserId,
                this.PageContext.PageBoardID,
                null,
                this.User.Item1.DisplayName,
                null,
                this.TimeZones.SelectedValue,
                language,
                culture,
                theme,
                this.HideMe.Checked);

            this.GetRepository<User>().UpdateOnly(
                () => new User { Activity = this.Activity.Checked },
                u => u.ID == this.currentUserId);

            if (this.User.Item1.IsGuest.Value)
            {
                this.GetRepository<Registry>().Save(
                    "timezone",
                    this.TimeZones.SelectedValue,
                    this.PageContext.PageBoardID);
            }

            // clear the cache for this user...)
            this.Get<IRaiseEvent>().Raise(new UpdateUserEvent(this.currentUserId));

            this.Get<IDataCache>().Clear();

            if (!this.PageContext.CurrentForumPage.IsAdminPage)
            {
                BuildLink.Redirect(ForumPages.MyAccount);
            }
            else
            {
                this.BindData();
            }
        }

        /// <summary>
        /// Binds the data.
        /// </summary>
        private void BindData()
        {
            this.TimeZones.DataSource = StaticDataHelper.TimeZones();

            if (this.Get<BoardSettings>().AllowUserTheme)
            {
                this.Theme.DataSource = StaticDataHelper.Themes();
            }

            if (this.Get<BoardSettings>().AllowUserLanguage)
            {
                this.Culture.DataSource = StaticDataHelper.Cultures();
                this.Culture.DataValueField = "CultureTag";
                this.Culture.DataTextField = "CultureNativeName";
            }

            this.DataBind();

            this.Email.Text = this.User.Item1.Email;

            var timeZoneItem = this.TimeZones.Items.FindByValue(this.User.Item1.TimeZoneInfo.Id);

            if (timeZoneItem != null)
            {
                timeZoneItem.Selected = true;
            }

            if (this.Get<BoardSettings>().AllowUserTheme && this.Theme.Items.Count > 0)
            {
                // Allows to use different per-forum themes,
                // While "Allow User Change Theme" option in the host settings is true
                var themeFile = this.Get<BoardSettings>().Theme;

                if (this.User.Item1.ThemeFile.IsSet())
                {
                    themeFile = this.User.Item1.ThemeFile;
                }

                var themeItem = this.Theme.Items.FindByValue(themeFile);

                if (themeItem != null)
                {
                    themeItem.Selected = true;
                }
                else
                {
                    themeItem = this.Theme.Items.FindByValue("yaf");

                    if (themeItem != null)
                    {
                        themeItem.Selected = true;
                    }
                }
            }

            this.HideMe.Checked = this.User.Item1.IsActiveExcluded.Value
                                  && (this.Get<BoardSettings>().AllowUserHideHimself || this.PageContext.IsAdmin);

            this.Activity.Checked = this.User.Item1.Activity;

            if (!this.Get<BoardSettings>().AllowUserLanguage || this.Culture.Items.Count <= 0)
            {
                return;
            }

            // If 2-letter language code is the same we return Culture, else we return a default full culture from language file
            var foundCultItem = this.Culture.Items.FindByValue(this.GetCulture(true));

            if (foundCultItem != null)
            {
                foundCultItem.Selected = true;
            }
        }

        #endregion

        /// <summary>
        /// Gets the culture.
        /// </summary>
        /// <param name="overrideByPageUserCulture">if set to <c>true</c> [override by page user culture].</param>
        /// <returns>
        /// The get culture.
        /// </returns>
        private string GetCulture(bool overrideByPageUserCulture)
        {
            // Language and culture
            var languageFile = this.Get<BoardSettings>().Language;
            var culture4Tag = this.Get<BoardSettings>().Culture;

            if (overrideByPageUserCulture)
            {
                if (this.PageContext.CurrentUser.LanguageFile.IsSet())
                {
                    languageFile = this.PageContext.CurrentUser.LanguageFile;
                }

                if (this.PageContext.CurrentUser.Culture.IsSet())
                {
                    culture4Tag = this.PageContext.CurrentUser.Culture;
                }
            }
            else
            {
                if (this.User.Item1.LanguageFile.IsSet())
                {
                    languageFile = this.User.Item1.LanguageFile;
                }

                if (this.User.Item1.Culture.IsSet())
                {
                    culture4Tag = this.User.Item1.Culture;
                }
            }

            // Get first default full culture from a language file tag.
            var langFileCulture = StaticDataHelper.CultureDefaultFromFile(languageFile);
            return langFileCulture.Substring(0, 2) == culture4Tag.Substring(0, 2) ? culture4Tag : langFileCulture;
        }
    }
}