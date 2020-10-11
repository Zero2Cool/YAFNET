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

namespace YAF.Pages.Account
{
    #region Using

    using System;
    using System.Linq;
    using System.Web;

    using YAF.Configuration;
    using YAF.Core.BasePages;
    using YAF.Core.Helpers;
    using YAF.Core.Model;
    using YAF.Core.Utilities;
    using YAF.Types;
    using YAF.Types.Constants;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Types.Interfaces.Identity;
    using YAF.Types.Models;
    using YAF.Types.Models.Identity;
    using YAF.Utils;
    using YAF.Utils.Helpers;
    using YAF.Web.Extensions;

    #endregion

    /// <summary>
    /// The User Register Page.
    /// </summary>
    public partial class Register : AccountPage
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "Register" /> class.
        /// </summary>
        public Register()
            : base("REGISTER")
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Gets a value indicating whether IsProtected.
        /// </summary>
        public override bool IsProtected => false;

        /// <summary>
        /// Gets or sets a value indicating whether the user is possible spam bot.
        /// </summary>
        /// <value>
        /// <c>true</c> if the user is possible spam bot; otherwise, <c>false</c>.
        /// </value>
        private bool IsPossibleSpamBot { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// The On PreRender event.
        /// </summary>
        /// <param name="e">
        /// the Event Arguments
        /// </param>
        protected override void OnPreRender([NotNull] EventArgs e)
        {
            this.PageContext.PageElements.RegisterJsBlockStartup(
                "passwordStrengthCheckJs",
                JavaScriptBlocks.PasswordStrengthCheckerJs(
                    this.Password.ClientID,
                    this.ConfirmPassword.ClientID,
                    this.PageContext.BoardSettings.MinRequiredPasswordLength,
                    this.GetText("PASSWORD_NOTMATCH"),
                    this.GetTextFormatted("PASSWORD_MIN", this.PageContext.BoardSettings.MinRequiredPasswordLength),
                    this.GetText("PASSWORD_GOOD"),
                    this.GetText("PASSWORD_STRONGER"),
                    this.GetText("PASSWORD_WEAK")));

            base.OnPreRender(e);
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// The <see cref="EventArgs"/> instance containing the event data.
        /// </param>
        protected void Page_Load([NotNull] object sender, [NotNull] EventArgs e)
        {
            if (this.IsPostBack)
            {
                this.BodyRegister.CssClass = "card-body was-validated";
                return;
            }

            // handle the CreateUser Step localization
            this.SetupCreateUserStep();

            if (this.PageContext.IsGuest && !Config.IsAnyPortal && Config.AllowLoginAndLogoff)
            {
                this.LoginButton.Visible = true;
                this.LoginButton.NavigateUrl = BuildLink.GetLink(ForumPages.Account_Login);
            }

            this.DataBind();

            this.EmailValid.ErrorMessage = this.GetText("PROFILE", "BAD_EMAIL");

            if (this.PageContext.BoardSettings.CaptchaTypeRegister == 2)
            {
                this.SetupRecaptchaControl();
            }
        }

        /// <summary>
        /// Create user
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void RegisterClick(object sender, EventArgs e)
        {
            if (!this.Page.IsValid)
            {
                return;
            }

            if (!this.ValidateUser())
            {
                return;
            }

            var user = new AspNetUsers
            {
                Id = Guid.NewGuid().ToString(),
                ApplicationId = this.PageContext.BoardSettings.ApplicationId,
                UserName = this.UserName.Text,
                LoweredUserName = this.UserName.Text,
                Email = this.Email.Text,
                IsApproved = false,
                EmailConfirmed = false
            };

            var result = this.Get<IAspNetUsersHelper>().Create(user, this.Password.Text.Trim());

            if (!result.Succeeded)
            {
                // error of some kind
                this.PageContext.AddLoadMessage(result.Errors.FirstOrDefault(), MessageTypes.danger);
            }
            else
            {
                // setup initial roles (if any) for this user
                this.Get<IAspNetRolesHelper>().SetupUserRoles(this.PageContext.PageBoardID, user);

                var displayName = this.DisplayName.Text;

                // create the user in the YAF DB as well as sync roles...
                var userID = this.Get<IAspNetRolesHelper>().CreateForumUser(user, displayName, this.PageContext.PageBoardID);

                if (userID == null)
                {
                    // something is seriously wrong here -- redirect to failure...
                    BuildLink.RedirectInfoPage(InfoMessage.Failure);
                }

                if (this.IsPossibleSpamBot)
                {
                    if (this.PageContext.BoardSettings.BotHandlingOnRegister.Equals(1))
                    {
                        this.Get<ISendNotification>().SendSpamBotNotificationToAdmins(user, userID.Value);
                    }
                }
                else
                {
                    // handle e-mail verification
                    var email = this.Email.Text.Trim();

                    this.Get<ISendNotification>().SendVerificationEmail(user, email, userID);

                    if (this.PageContext.BoardSettings.NotificationOnUserRegisterEmailList.IsSet())
                    {
                        // send user register notification to the following admin users...
                        this.Get<ISendNotification>().SendRegistrationNotificationEmail(user, userID.Value);
                    }
                }

                this.BodyRegister.Visible = false;
                this.Footer.Visible = false;

                // success notification localization
                this.Message.Visible = true;
                this.AccountCreated.Text = this.Get<IBBCode>().MakeHtml(
                    this.GetText("ACCOUNT_CREATED_VERIFICATION"),
                    true,
                    true);
            }
        }

        /// <summary>
        /// Create the Page links.
        /// </summary>
        protected override void CreatePageLinks()
        {
            this.PageLinks.AddRoot();
            this.PageLinks.AddLink(this.GetText("TITLE"));
        }

        /// <summary>
        /// Handles the Click event of the RefreshCaptcha control.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// The <see cref="EventArgs"/> instance containing the event data.
        /// </param>
        protected void RefreshCaptcha_Click(object sender, EventArgs e)
        {
            this.imgCaptcha.ImageUrl = $"{BoardInfo.ForumClientFileRoot}resource.ashx?c=1&t=";
        }

        /// <summary>
        /// The setup display name UI.
        /// </summary>
        /// <param name="enabled">
        /// The enabled.
        /// </param>
        private void SetupDisplayNameUI(bool enabled)
        {
            this.DisplayNamePlaceHolder.Visible = enabled;
        }

        /// <summary>
        /// The setup create user step.
        /// </summary>
        private void SetupCreateUserStep()
        {
            var captchaPlaceHolder = this.YafCaptchaHolder;
            var recaptchaPlaceHolder = this.RecaptchaPlaceHolder;

            if (this.PageContext.BoardSettings.CaptchaTypeRegister == 1)
            {
                this.imgCaptcha.ImageUrl = $"{BoardInfo.ForumClientFileRoot}resource.ashx?c=1&t={DateTime.UtcNow}";

                this.RefreshCaptcha.Text = this.GetText("GENERATE_CAPTCHA");

                this.RefreshCaptcha.Click += this.RefreshCaptcha_Click;

                captchaPlaceHolder.Visible = true;
            }
            else
            {
                captchaPlaceHolder.Visible = false;
            }

            recaptchaPlaceHolder.Visible = this.PageContext.BoardSettings.CaptchaTypeRegister == 2;

            this.SetupDisplayNameUI(this.PageContext.BoardSettings.EnableDisplayName);
        }

        /// <summary>
        /// The setup reCAPTCHA control.
        /// </summary>
        private void SetupRecaptchaControl()
        {
            this.RecaptchaPlaceHolder.Visible = true;

            if (this.PageContext.BoardSettings.RecaptchaPrivateKey.IsSet() &&
                this.PageContext.BoardSettings.RecaptchaPublicKey.IsSet())
            {
                return;
            }

            this.Logger.Log(this.PageContext.PageUserID, this, "secret or site key is required for reCAPTCHA!");
            BuildLink.AccessDenied();
        }

        /// <summary>
        /// Validate user for user name and or display name, captcha and spam
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool ValidateUser()
        {
            var userName = this.UserName.Text.Trim();

            // username cannot contain semi-colon or to be a bad word
            var badWord = this.Get<IBadWordReplace>().ReplaceItems.Any(
                i => userName.Equals(i.BadWord, StringComparison.CurrentCultureIgnoreCase));

            var guestUserName = this.Get<IAspNetUsersHelper>().GuestUserName;

            guestUserName = guestUserName.IsSet() ? guestUserName.ToLower() : string.Empty;

            if (userName.Contains(";") || badWord || userName.ToLower().Equals(guestUserName))
            {
                this.PageContext.AddLoadMessage(this.GetText("BAD_USERNAME"), MessageTypes.warning);

                return false;
            }

            if (userName.Length < this.PageContext.BoardSettings.DisplayNameMinLength)
            {
                this.PageContext.AddLoadMessage(
                    this.GetTextFormatted("USERNAME_TOOSMALL", this.PageContext.BoardSettings.DisplayNameMinLength),
                    MessageTypes.danger);

                return false;
            }

            if (userName.Length > this.PageContext.BoardSettings.UserNameMaxLength)
            {
                this.PageContext.AddLoadMessage(
                    this.GetTextFormatted("USERNAME_TOOLONG", this.PageContext.BoardSettings.UserNameMaxLength),
                    MessageTypes.danger);

                return false;
            }

            if (this.PageContext.BoardSettings.EnableDisplayName && this.DisplayName.Text.Trim().IsSet())
            {
                var displayName = this.DisplayName.Text.Trim();

                // Check if name matches the required minimum length
                if (displayName.Length < this.PageContext.BoardSettings.DisplayNameMinLength)
                {
                    this.PageContext.AddLoadMessage(
                        this.GetTextFormatted("USERNAME_TOOSMALL", this.PageContext.BoardSettings.DisplayNameMinLength),
                        MessageTypes.warning);

                    return false;
                }

                // Check if name matches the required minimum length
                if (displayName.Length > this.PageContext.BoardSettings.UserNameMaxLength)
                {
                    this.PageContext.AddLoadMessage(
                        this.GetTextFormatted("USERNAME_TOOLONG", this.PageContext.BoardSettings.UserNameMaxLength),
                        MessageTypes.warning);

                    return false;
                }

                if (this.Get<IUserDisplayName>().FindUserByName(displayName.Trim()) != null)
                {
                    this.PageContext.AddLoadMessage(
                        this.GetText("ALREADY_REGISTERED_DISPLAYNAME"),
                        MessageTypes.warning);
                }
            }

            this.IsPossibleSpamBot = false;

            // Check user for bot
            var userIpAddress = this.Get<HttpRequestBase>().GetUserRealIPAddress();

            // Check content for spam
            if (this.Get<ISpamCheck>().CheckUserForSpamBot(userName, this.Email.Text, userIpAddress, out var result))
            {
                // Flag user as spam bot
                this.IsPossibleSpamBot = true;

                this.GetRepository<Registry>().IncrementDeniedRegistrations();

                this.Logger.Log(
                    null,
                    "Bot Detected",
                    $"Bot Check detected a possible SPAM BOT: (user name : '{userName}', email : '{this.Email.Text}', ip: '{userIpAddress}', reason : {result}), user was rejected.",
                    EventLogTypes.SpamBotDetected);

                if (this.PageContext.BoardSettings.BotHandlingOnRegister.Equals(2))
                {
                    this.GetRepository<Registry>().IncrementBannedUsers();

                    this.PageContext.AddLoadMessage(this.GetText("BOT_MESSAGE"), MessageTypes.danger);

                    if (this.PageContext.BoardSettings.BanBotIpOnDetection)
                    {
                        this.GetRepository<BannedIP>().Save(
                            null,
                            userIpAddress,
                            $"A spam Bot who was trying to register was banned by IP {userIpAddress}",
                            this.PageContext.PageUserID);

                        if (this.PageContext.BoardSettings.LogBannedIP)
                        {
                            this.Logger.Log(
                                this.PageContext.PageUserID,
                                "IP BAN of Bot During Registration",
                                $"A spam Bot who was trying to register was banned by IP {userIpAddress}",
                                EventLogTypes.IpBanSet);
                        }
                    }

                    // Ban Name ?
                    this.PageContext.GetRepository<BannedName>().Save(
                        null,
                        userName,
                        "Name was reported by the automatic spam system.");

                    // Ban User Email?
                    this.PageContext.GetRepository<BannedEmail>().Save(
                        null,
                        this.Email.Text,
                        "Email was reported by the automatic spam system.");

                    return false;
                }
            }

            switch (this.PageContext.BoardSettings.CaptchaTypeRegister)
            {
                case 1:
                {
                    // Check YAF Captcha
                    if (!CaptchaHelper.IsValid(this.tbCaptcha.Text.Trim()))
                    {
                        this.PageContext.AddLoadMessage(this.GetText("BAD_CAPTCHA"), MessageTypes.danger);

                        return false;
                    }
                }

                    break;
                case 2:
                {
                    // Check reCAPTCHA
                    if (!this.Recaptcha1.IsValid)
                    {
                        this.PageContext.AddLoadMessage(this.GetText("BAD_RECAPTCHA"), MessageTypes.danger);

                        return false;
                    }
                }

                    break;
            }

            return true;
        }

        #endregion
    }
}