﻿/* Yet Another Forum.NET
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

namespace YAF.Core.Services
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Net.Mail;
    using System.Web;
    using System.Web.Security;

    using YAF.Configuration;
    using YAF.Core.Model;
    using YAF.Core.UsersRoles;
    using YAF.Types;
    using YAF.Types.Constants;
    using YAF.Types.Extensions;
    using YAF.Types.Flags;
    using YAF.Types.Interfaces;
    using YAF.Types.Models;
    using YAF.Types.Objects;
    using YAF.Utils;
    using YAF.Utils.Helpers;

    #endregion

    /// <summary>
    /// The YAF Send Notification.
    /// </summary>
    public class YafSendNotification : ISendNotification, IHaveServiceLocator
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YafSendNotification"/> class.
        /// </summary>
        /// <param name="serviceLocator">
        /// The service locator.
        /// </param>
        public YafSendNotification([NotNull] IServiceLocator serviceLocator)
        {
            this.ServiceLocator = serviceLocator;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets ServiceLocator.
        /// </summary>
        public IServiceLocator ServiceLocator { get; set; }

        /// <summary>
        /// Gets the board settings.
        /// </summary>
        /// <value>
        /// The board settings.
        /// </value>
        public YafBoardSettings BoardSettings => this.Get<YafBoardSettings>();

        #endregion

        #region Implemented Interfaces

        #region ISendNotification

        /// <summary>
        /// Sends Notifications to Moderators that Message Needs Approval
        /// </summary>
        /// <param name="forumId">The forum id.</param>
        /// <param name="newMessageId">The new message id.</param>
        /// <param name="isSpamMessage">if set to <c>true</c> [is spam message].</param>
        public void ToModeratorsThatMessageNeedsApproval(int forumId, int newMessageId, bool isSpamMessage)
        {
            var moderatorsFiltered = this.Get<YafDbBroker>().GetAllModerators().Where(f => f.ForumID.Equals(forumId));
            var moderatorUserNames = new List<string>();

            moderatorsFiltered.ForEach(
                moderator =>
                    {
                        if (moderator.IsGroup)
                        {
                            moderatorUserNames.AddRange(this.Get<RoleProvider>().GetUsersInRole(moderator.Name));
                        }
                        else
                        {
                            moderatorUserNames.Add(moderator.Name);
                        }
                    });

            // send each message...
            moderatorUserNames.Distinct().ForEach(
                userName =>
                    {
                        // add each member of the group
                        var membershipUser = UserMembershipHelper.GetUser(userName);
                        var userId = UserMembershipHelper.GetUserIDFromProviderUserKey(membershipUser.ProviderUserKey);

                        var languageFile = UserHelper.GetUserLanguageFile(userId);

                        var subject = string.Format(
                            this.Get<ILocalization>().GetText(
                                "COMMON",
                                isSpamMessage
                                    ? "NOTIFICATION_ON_MODERATOR_SPAMMESSAGE_APPROVAL"
                                    : "NOTIFICATION_ON_MODERATOR_MESSAGE_APPROVAL",
                                languageFile),
                            this.BoardSettings.Name);

                        var notifyModerators = new YafTemplateEmail(
                                                   isSpamMessage
                                                       ? "NOTIFICATION_ON_MODERATOR_SPAMMESSAGE_APPROVAL"
                                                       : "NOTIFICATION_ON_MODERATOR_MESSAGE_APPROVAL")
                                                   {
                                                       // get the user localization...
                                                       TemplateLanguageFile = languageFile,
                                                       TemplateParams =
                                                           {
                                                               ["{adminlink}"] = YafBuildLink.GetLinkNotEscaped(
                                                                   ForumPages.moderate_unapprovedposts,
                                                                   true,
                                                                   "f={0}",
                                                                   forumId)
                                                           }
                                                   };

                        notifyModerators.SendEmail(
                            new MailAddress(membershipUser.Email, membershipUser.UserName),
                            subject,
                            true);
                    });
        }

        /// <summary>
        /// Sends Notifications to Moderators that a Message was Reported
        /// </summary>
        /// <param name="pageForumID">
        /// The page Forum ID.
        /// </param>
        /// <param name="reportedMessageId">
        /// The reported message id.
        /// </param>
        /// <param name="reporter">
        /// The reporter.
        /// </param>
        /// <param name="reportText">
        /// The report Text.
        /// </param>
        public void ToModeratorsThatMessageWasReported(
            int pageForumID,
            int reportedMessageId,
            int reporter,
            string reportText)
        {
            try
            {
                var moderatorsFiltered =
                    this.Get<YafDbBroker>().GetAllModerators().Where(f => f.ForumID.Equals(pageForumID));
                var moderatorUserNames = new List<string>();

                moderatorsFiltered.ForEach(
                    moderator =>
                        {
                            if (moderator.IsGroup)
                            {
                                moderatorUserNames.AddRange(this.Get<RoleProvider>().GetUsersInRole(moderator.Name));
                            }
                            else
                            {
                                moderatorUserNames.Add(moderator.Name);
                            }
                        });

                // send each message...
                moderatorUserNames.Distinct().ForEach(
                    userName =>
                        {
                            // add each member of the group
                            var membershipUser = UserMembershipHelper.GetUser(userName);
                            var userId =
                                UserMembershipHelper.GetUserIDFromProviderUserKey(membershipUser.ProviderUserKey);

                            var languageFile = UserHelper.GetUserLanguageFile(userId);

                            var subject = string.Format(
                                this.Get<ILocalization>().GetText(
                                    "COMMON",
                                    "NOTIFICATION_ON_MODERATOR_REPORTED_MESSAGE",
                                    languageFile),
                                this.BoardSettings.Name);

                            var notifyModerators = new YafTemplateEmail("NOTIFICATION_ON_MODERATOR_REPORTED_MESSAGE")
                                                       {
                                                           // get the user localization...
                                                           TemplateLanguageFile = languageFile,
                                                           TemplateParams =
                                                               {
                                                                   ["{reason}"] = reportText,
                                                                   ["{reporter}"] =
                                                                       this.Get<IUserDisplayName>().GetName(reporter),
                                                                   ["{adminlink}"] = YafBuildLink.GetLinkNotEscaped(
                                                                       ForumPages.moderate_reportedposts,
                                                                       true,
                                                                       "f={0}",
                                                                       pageForumID)
                                                               }
                                                       };

                            notifyModerators.SendEmail(
                                new MailAddress(membershipUser.Email, membershipUser.UserName),
                                subject,
                                true);
                        });
            }
            catch (Exception x)
            {
                // report exception to the forum's event log
                this.Get<ILogger>().Error(
                    x,
                    $"Send Message Report Notification Error for UserID {YafContext.Current.PageUserID}");
            }
        }

        /// <summary>
        /// Sends notification about new PM in user's inbox.
        /// </summary>
        /// <param name="toUserId">
        /// User supposed to receive notification about new PM.
        /// </param>
        /// <param name="subject">
        /// Subject of PM user is notified about.
        /// </param>
        public void ToPrivateMessageRecipient(int toUserId, [NotNull] string subject)
        {
            try
            {
                // user's PM notification setting
                var privateMessageNotificationEnabled = false;

                // user's email
                var toEMail = string.Empty;

                var toUser = this.GetRepository<User>().UserList(
                    YafContext.Current.PageBoardID,
                    toUserId,
                    true,
                    null,
                    null,
                    null).FirstOrDefault();

                if (toUser != null)
                {
                    privateMessageNotificationEnabled = toUser.PMNotification ?? false;
                    toEMail = toUser.Email;
                }

                if (!privateMessageNotificationEnabled)
                {
                    return;
                }

                // get the PM ID
                var userPMessageId = this.GetRepository<PMessage>().ListAsDataTable(toUserId, null, null).GetFirstRow()
                    .Field<int>("UserPMessageID");

                var languageFile = UserHelper.GetUserLanguageFile(toUserId);

                var displayName = this.Get<IUserDisplayName>().GetName(YafContext.Current.PageUserID);

                // send this user a PM notification e-mail
                var notificationTemplate = new YafTemplateEmail("PMNOTIFICATION")
                                               {
                                                   TemplateLanguageFile = languageFile,
                                                   TemplateParams =
                                                       {
                                                           ["{fromuser}"] = displayName,
                                                           ["{link}"] =
                                                               $"{YafBuildLink.GetLinkNotEscaped(ForumPages.cp_message, true, "pm={0}", userPMessageId)}\r\n\r\n",
                                                           ["{subject}"] = subject,
                                                           ["{username}"] =
                                                               this.BoardSettings.EnableDisplayName
                                                                   ? toUser.DisplayName
                                                                   : toUser.Name
                                                       }
                                               };

                // create notification email subject
                var emailSubject = string.Format(
                    this.Get<ILocalization>().GetText("COMMON", "PM_NOTIFICATION_SUBJECT", languageFile),
                    displayName,
                    this.BoardSettings.Name,
                    subject);

                // send email
                notificationTemplate.SendEmail(new MailAddress(toEMail), emailSubject, true);
            }
            catch (Exception x)
            {
                // report exception to the forum's event log
                this.Get<ILogger>().Error(x, $"Send PM Notification Error for UserID {YafContext.Current.PageUserID}");

                // tell user about failure
                YafContext.Current.AddLoadMessage(
                    this.Get<ILocalization>().GetTextFormatted("Failed", x.Message),
                    MessageTypes.danger);
            }
        }

        /// <summary>
        /// The to watching users.
        /// </summary>
        /// <param name="newMessageId">
        /// The new message id.
        /// </param>
        public void ToWatchingUsers(int newMessageId)
        {
            // Always send watch mails with boards default language
            // TODO : Send in user Language
            var languageFile = this.BoardSettings.Language;

            var boardName = this.BoardSettings.Name;
            var forumEmail = this.BoardSettings.ForumEmail;

            var message = this.GetRepository<Message>().MessageList(newMessageId).FirstOrDefault();

            var messageAuthorUserID = message.UserID ?? 0;

            // cleaned body as text...
            var bodyText = this.Get<IBadWordReplace>()
                .Replace(BBCodeHelper.StripBBCode(HtmlHelper.StripHtml(HtmlHelper.CleanHtmlString(message.Message))))
                .RemoveMultipleWhitespace();

            // Send track mails
            var subject = string.Format(
                this.Get<ILocalization>().GetText("COMMON", "TOPIC_NOTIFICATION_SUBJECT", languageFile),
                boardName);

            var watchEmail = new YafTemplateEmail("TOPICPOST")
                                 {
                                     TemplateLanguageFile = languageFile,
                                     TemplateParams =
                                         {
                                             ["{topic}"] =
                                                 HttpUtility.HtmlDecode(
                                                     this.Get<IBadWordReplace>().Replace(message.Topic)),
                                             ["{postedby}"] =
                                                 UserMembershipHelper.GetDisplayNameFromID(messageAuthorUserID),
                                             ["{body}"] = bodyText,
                                             ["{bodytruncated}"] = bodyText.Truncate(160),
                                             ["{link}"] = YafBuildLink.GetLinkNotEscaped(
                                                 ForumPages.posts,
                                                 true,
                                                 "m={0}#post{0}",
                                                 newMessageId),
                                             ["{subscriptionlink}"] = YafBuildLink.GetLinkNotEscaped(
                                                 ForumPages.cp_subscriptions,
                                                 true)
                                         }
                                 };

            watchEmail.CreateWatch(
                message.TopicID ?? 0,
                messageAuthorUserID,
                new MailAddress(forumEmail, boardName),
                subject);

            if (!this.BoardSettings.AllowNotificationAllPostsAllTopics)
            {
                return;
            }

            var usersWithAll = this.GetRepository<User>().FindUserTyped(
                false,
                notificationType: UserNotificationSetting.AllTopics.ToInt());

            // create individual watch emails for all users who have All Posts on...
            usersWithAll.Where(x => x.ID != messageAuthorUserID && x.ProviderUserKey != null).ForEach(
                user =>
                    {
                        if (user.Email.IsNotSet())
                        {
                            return;
                        }

                        watchEmail.TemplateLanguageFile = user.LanguageFile.IsSet()
                                                              ? user.LanguageFile
                                                              : this.Get<ILocalization>().LanguageFileName;
                        watchEmail.SendEmail(
                            new MailAddress(forumEmail, boardName),
                            new MailAddress(
                                user.Email,
                                this.BoardSettings.EnableDisplayName ? user.DisplayName : user.Name),
                            subject,
                            true);
                    });
        }

        /// <summary>
        /// Send an Email to the Newly Created User with
        /// his Account Info (Pass, Security Question and Answer)
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="pass">
        /// The pass.
        /// </param>
        /// <param name="securityAnswer">
        /// The security answer.
        /// </param>
        /// <param name="templateName">
        /// The template Name.
        /// </param>
        public void SendRegistrationNotificationToUser(
            [NotNull] MembershipUser user,
            [NotNull] string pass,
            [NotNull] string securityAnswer,
            string templateName)
        {
            var subject = this.Get<ILocalization>().GetTextFormatted(
                "NOTIFICATION_ON_NEW_FACEBOOK_USER_SUBJECT",
                this.BoardSettings.Name);

            var notifyUser = new YafTemplateEmail(templateName)
                                 {
                                     TemplateParams =
                                         {
                                             ["{user}"] = user.UserName,
                                             ["{email}"] = user.Email,
                                             ["{pass}"] = pass,
                                             ["{answer}"] = securityAnswer
                                         }
                                 };

            notifyUser.SendEmail(new MailAddress(user.Email, user.UserName), subject, true);
        }

        /// <summary>
        /// Sends notification that the User was awarded with a Medal
        /// </summary>
        /// <param name="toUserId">To user id.</param>
        /// <param name="medalName">Name of the medal.</param>
        public void ToUserWithNewMedal([NotNull] int toUserId, [NotNull] string medalName)
        {
            var userList = this.GetRepository<User>().UserList(
                YafContext.Current.PageBoardID,
                toUserId,
                true,
                null,
                null,
                null).ToList();

            TypedUserList toUser;

            if (userList.Any())
            {
                toUser = userList.First();
            }
            else
            {
                return;
            }

            var languageFile = UserHelper.GetUserLanguageFile(toUser.UserID.ToType<int>());

            var subject = string.Format(
                this.Get<ILocalization>().GetText("COMMON", "NOTIFICATION_ON_MEDAL_AWARDED_SUBJECT", languageFile),
                this.BoardSettings.Name);

            var notifyUser = new YafTemplateEmail("NOTIFICATION_ON_MEDAL_AWARDED")
                                 {
                                     TemplateLanguageFile = languageFile,
                                     TemplateParams =
                                         {
                                             ["{user}"] =
                                                 this.BoardSettings.EnableDisplayName
                                                     ? toUser.DisplayName
                                                     : toUser.Name,
                                             ["{medalname}"] = medalName
                                         }
                                 };

            notifyUser.SendEmail(
                new MailAddress(toUser.Email, this.BoardSettings.EnableDisplayName ? toUser.DisplayName : toUser.Name),
                subject,
                true);
        }

        /// <summary>
        /// Sends the role un assignment notification.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="removedRoles">The removed roles.</param>
        public void SendRoleUnAssignmentNotification([NotNull] MembershipUser user, List<string> removedRoles)
        {
            var subject = this.Get<ILocalization>().GetTextFormatted(
                "NOTIFICATION_ROLE_ASSIGNMENT_SUBJECT",
                this.BoardSettings.Name);

            var templateEmail = new YafTemplateEmail("NOTIFICATION_ROLE_UNASSIGNMENT")
                                    {
                                        TemplateParams =
                                            {
                                                ["{user}"] = user.UserName,
                                                ["{roles}"] = string.Join(", ", removedRoles.ToArray()),
                                                ["{forumname}"] = this.BoardSettings.Name,
                                                ["{forumurl}"] = YafForumInfo.ForumURL
                                            }
                                    };

            templateEmail.SendEmail(new MailAddress(user.Email, user.UserName), subject, true);
        }

        /// <summary>
        /// Sends the role assignment notification.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="addedRoles">The added roles.</param>
        public void SendRoleAssignmentNotification([NotNull] MembershipUser user, List<string> addedRoles)
        {
            var subject = this.Get<ILocalization>().GetTextFormatted(
                "NOTIFICATION_ROLE_ASSIGNMENT_SUBJECT",
                this.BoardSettings.Name);

            var templateEmail = new YafTemplateEmail("NOTIFICATION_ROLE_ASSIGNMENT")
                                    {
                                        TemplateParams =
                                            {
                                                ["{user}"] = user.UserName,
                                                ["{roles}"] = string.Join(", ", addedRoles.ToArray()),
                                                ["{forumname}"] = this.BoardSettings.Name,
                                                ["{forumurl}"] = YafForumInfo.ForumURL
                                            }
                                    };

            templateEmail.SendEmail(new MailAddress(user.Email, user.UserName), subject, true);
        }

        /// <summary>
        /// Sends a new user notification email to all emails in the NotificationOnUserRegisterEmailList
        /// Setting
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="userId">The user id.</param>
        public void SendRegistrationNotificationEmail([NotNull] MembershipUser user, int userId)
        {
            var emails = this.BoardSettings.NotificationOnUserRegisterEmailList.Split(';');

            var subject = this.Get<ILocalization>().GetTextFormatted(
                "NOTIFICATION_ON_USER_REGISTER_EMAIL_SUBJECT",
                this.BoardSettings.Name);

            var notifyAdmin = new YafTemplateEmail("NOTIFICATION_ON_USER_REGISTER")
                                  {
                                      TemplateParams =
                                          {
                                              ["{adminlink}"] = YafBuildLink.GetLinkNotEscaped(
                                                  ForumPages.admin_edituser,
                                                  true,
                                                  "u={0}",
                                                  userId),
                                              ["{user}"] = user.UserName,
                                              ["{email}"] = user.Email
                                          }
                                  };

            emails.Where(email => email.Trim().IsSet()).ForEach(
                email => notifyAdmin.SendEmail(new MailAddress(email.Trim()), subject, true));
        }

        /// <summary>
        /// Sends a spam bot notification to admins.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="userId">The user id.</param>
        public void SendSpamBotNotificationToAdmins([NotNull] MembershipUser user, int userId)
        {
            // Get Admin Group ID
            var adminGroupId = this.GetRepository<Group>().List(boardId: YafContext.Current.PageBoardID)
                .Where(group => group.Name.Contains("Admin")).Select(group => group.ID).FirstOrDefault();

            if (adminGroupId <= 0)
            {
                return;
            }

            using (var dt = this.GetRepository<User>().EmailsAsDataTable(YafContext.Current.PageBoardID, adminGroupId))
            {
                dt.Rows.Cast<DataRow>().ForEach(
                    row =>
                        {
                            var emailAddress = row.Field<string>("Email");

                            if (emailAddress.IsNotSet())
                            {
                                return;
                            }

                            var subject = this.Get<ILocalization>().GetTextFormatted(
                                "COMMON",
                                "NOTIFICATION_ON_BOT_USER_REGISTER_EMAIL_SUBJECT",
                                this.BoardSettings.Name);

                            var notifyAdmin = new YafTemplateEmail("NOTIFICATION_ON_BOT_USER_REGISTER")
                                                  {
                                                      TemplateParams =
                                                          {
                                                              ["{adminlink}"] = YafBuildLink.GetLinkNotEscaped(
                                                                  ForumPages.admin_edituser,
                                                                  true,
                                                                  "u={0}",
                                                                  userId),
                                                              ["{user}"] = user.UserName,
                                                              ["{email}"] = user.Email
                                                          }
                                                  };

                            notifyAdmin.SendEmail(new MailAddress(emailAddress), subject, true);
                        });
            }
        }

        /// <summary>
        /// Sends the user welcome notification.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="userId">The user identifier.</param>
        public void SendUserWelcomeNotification([NotNull] MembershipUser user, int? userId)
        {
            if (this.BoardSettings.SendWelcomeNotificationAfterRegister.Equals(0))
            {
                return;
            }

            var subject = this.Get<ILocalization>().GetTextFormatted(
                "NOTIFICATION_ON_WELCOME_USER_SUBJECT",
                this.BoardSettings.Name);

            var notifyUser = new YafTemplateEmail("NOTIFICATION_ON_WELCOME_USER")
                                 {
                                     TemplateParams = { ["{user}"] = user.UserName }
                                 };

            var emailBody = notifyUser.ProcessTemplate("NOTIFICATION_ON_WELCOME_USER_TEXT");

            var messageFlags = new MessageFlags { IsHtml = false, IsBBCode = true };

            if (this.BoardSettings.AllowPrivateMessages
                && this.BoardSettings.SendWelcomeNotificationAfterRegister.Equals(2))
            {
                var users = this.GetRepository<User>().UserList(
                    YafContext.Current.PageBoardID,
                    null,
                    true,
                    null,
                    null,
                    null).ToList();

                var hostUser = users.FirstOrDefault(u => u.IsHostAdmin > 0);

                YafContext.Current.GetRepository<PMessage>().SendMessage(
                    hostUser.UserID.Value,
                    userId.Value,
                    subject,
                    emailBody,
                    messageFlags.BitValue,
                    -1);
            }
            else
            {
                notifyUser.SendEmail(new MailAddress(user.Email, user.UserName), subject, true);
            }
        }

        /// <summary>
        /// Sends the verification email.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="email">The email.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="newUsername">The new username.</param>
        public void SendVerificationEmail(
            [NotNull] MembershipUser user,
            [NotNull] string email,
            int? userId,
            string newUsername = null)
        {
            CodeContracts.VerifyNotNull(email, "email");
            CodeContracts.VerifyNotNull(user, "user");

            var hashInput = $"{DateTime.UtcNow}{email}{Security.CreatePassword(20)}";
            var hash = FormsAuthentication.HashPasswordForStoringInConfigFile(hashInput, "md5");

            // save verification record...
            this.GetRepository<CheckEmail>().Save(userId, hash, user.Email);

            var subject = this.Get<ILocalization>().GetTextFormatted(
                "VERIFICATION_EMAIL_SUBJECT",
                this.BoardSettings.Name);

            var verifyEmail = new YafTemplateEmail("VERIFYEMAIL")
                                  {
                                      TemplateParams =
                                          {
                                              ["{link}"] =
                                                  YafBuildLink.GetLinkNotEscaped(
                                                      ForumPages.approve,
                                                      true,
                                                      "k={0}",
                                                      hash),
                                              ["{key}"] = hash,
                                              ["{username}"] = user.UserName
                                          }
                                  };

            verifyEmail.SendEmail(new MailAddress(email, newUsername ?? user.UserName), subject, true);
        }

        /// <summary>
        /// Send Email Verification to changed Email Address
        /// </summary>
        /// <param name="newEmail">
        /// The new email.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <param name="userName">
        /// The user Name.
        /// </param>
        public void SendEmailChangeVerification([NotNull] string newEmail, [NotNull] int userId, string userName)
        {
            var hashInput = $"{DateTime.UtcNow}{newEmail}{Security.CreatePassword(20)}";
            var hash = FormsAuthentication.HashPasswordForStoringInConfigFile(hashInput, "md5");

            // Create Email
            var changeEmail = new YafTemplateEmail("CHANGEEMAIL")
                                  {
                                      TemplateParams =
                                          {
                                              ["{user}"] = userName,
                                              ["{link}"] =
                                                  $"{YafBuildLink.GetLinkNotEscaped(ForumPages.approve, true, "k={0}", hash)}\r\n\r\n",
                                              ["{newemail}"] = newEmail,
                                              ["{key}"] = hash
                                          }
                                  };

            // save a change email reference to the db
            this.GetRepository<CheckEmail>().Save(userId, hash, newEmail);

            // send a change email message...
            changeEmail.SendEmail(
                new MailAddress(newEmail),
                this.Get<ILocalization>().GetText("COMMON", "CHANGEEMAIL_SUBJECT"),
                true);

            // show a confirmation
            YafContext.Current.AddLoadMessage(
                string.Format(this.Get<ILocalization>().GetText("PROFILE", "mail_sent"), newEmail),
                MessageTypes.info);
        }

        /// <summary>
        /// Sends the user a suspension notification.
        /// </summary>
        /// <param name="suspendedUntil">The suspended until.</param>
        /// <param name="suspendReason">The suspend reason.</param>
        /// <param name="email">The email.</param>
        /// <param name="userName">Name of the user.</param>
        public void SendUserSuspensionNotification(
            [NotNull] DateTime suspendedUntil,
            [NotNull] string suspendReason,
            [NotNull] string email,
            [NotNull] string userName)
        {
            var subject = this.Get<ILocalization>().GetTextFormatted(
                "NOTIFICATION_ON_SUSPENDING_USER_SUBJECT",
                this.BoardSettings.Name);

            var notifyUser = new YafTemplateEmail("NOTIFICATION_ON_SUSPENDING_USER")
                                 {
                                     TemplateParams =
                                         {
                                             ["{user}"] = userName,
                                             ["{suspendReason}"] = suspendReason,
                                             ["{suspendedUntil}"] =
                                                 suspendedUntil.ToString(CultureInfo.InvariantCulture),
                                             ["{forumname}"] = this.BoardSettings.Name,
                                             ["{forumurl}"] = YafForumInfo.ForumURL
                                         }
                                 };

            notifyUser.SendEmail(new MailAddress(email, userName), subject, true);
        }

        /// <summary>
        /// Sends the user a suspension notification.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <param name="userName">Name of the user.</param>
        public void SendUserSuspensionEndedNotification([NotNull] string email, [NotNull] string userName)
        {
            var subject = this.Get<ILocalization>().GetTextFormatted(
                "NOTIFICATION_ON_SUSPENDING_USER_SUBJECT",
                this.BoardSettings.Name);

            var notifyUser = new YafTemplateEmail("NOTIFICATION_ON_SUSPENDING_ENDED_USER")
                                 {
                                     TemplateParams =
                                         {
                                             ["{user}"] = userName,
                                             ["{forumname}"] = this.BoardSettings.Name,
                                             ["{forumurl}"] = YafForumInfo.ForumURL
                                         }
                                 };

            notifyUser.SendEmail(new MailAddress(email, userName), subject, true);
        }

        #endregion

        #endregion
    }
}