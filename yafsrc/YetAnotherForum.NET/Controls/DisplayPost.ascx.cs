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

namespace YAF.Controls
{
    #region Using

    using System;
    using System.Text;
    using System.Web;
    using System.Web.UI.WebControls;

    using ServiceStack;

    using YAF.Core.BaseControls;
    using YAF.Core.Context.Start;
    using YAF.Core.Extensions;
    using YAF.Core.Helpers;
    using YAF.Core.Model;
    using YAF.Core.Utilities;
    using YAF.Types;
    using YAF.Types.Constants;
    using YAF.Types.Extensions;
    using YAF.Types.Flags;
    using YAF.Types.Interfaces;
    using YAF.Types.Interfaces.Identity;
    using YAF.Types.Models;
    using YAF.Types.Objects.Model;
    using YAF.Utils;
    using YAF.Utils.Helpers;
    using YAF.Web.Controls;

    using ButtonStyle = YAF.Types.Constants.ButtonStyle;

    #endregion

    /// <summary>
    /// DisplayPost Class.
    /// </summary>
    public partial class DisplayPost : BaseUserControl
    {
        #region Constants and Fields

        /// <summary>
        ///   The current Post Data for this post.
        /// </summary>
        private PostDataHelperWrapper postDataHelperWrapper;

        #endregion

        #region Public Properties

        /// <summary>
        ///   Gets or sets Current Page Index.
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// Gets or sets the data source.
        /// </summary>
        [CanBeNull]
        public PagedMessage DataSource { get; set; }

        /// <summary>
        ///   Gets a value indicating whether IsGuest.
        /// </summary>
        public bool IsGuest => this.PostData == null || this.Get<IAspNetUsersHelper>().IsGuestUser(this.PostData.UserId);

        /// <summary>
        ///   Gets or sets Post Count.
        /// </summary>
        public int PostCount { get; set; }

        /// <summary>
        ///   Gets Post Data helper functions.
        /// </summary>
        public PostDataHelperWrapper PostData
        {
            get
            {
                if (this.postDataHelperWrapper == null && this.DataSource != null)
                {
                    this.postDataHelperWrapper = new PostDataHelperWrapper(this.DataSource);
                }

                return this.postDataHelperWrapper;
            }
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// The on pre render.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        protected override void OnPreRender(EventArgs e)
        {
            if (this.PageContext.IsGuest)
            {
                this.ShowHideIgnoredUserPost.Visible = false;
                this.MessageRow.CssClass = "collapse show";
            }
            else if (this.Get<IUserIgnored>().IsIgnored(this.PostData.UserId))
            {
                this.MessageRow.CssClass = "collapse";
                this.ShowHideIgnoredUserPost.Visible = true;
            }
            else if (!this.Get<IUserIgnored>().IsIgnored(this.PostData.UserId))
            {
                this.MessageRow.CssClass = "collapse show";
            }

            this.Edit.Visible = this.Edit2.Visible =
                                    !this.PostData.PostDeleted && this.PostData.CanEditPost && !this.PostData.IsLocked;
            this.Edit.NavigateUrl = this.Edit2.NavigateUrl = BuildLink.GetLink(
                                        ForumPages.PostMessage,
                                        "m={0}",
                                        this.PostData.MessageId);
            this.MovePost.Visible =
                this.Move.Visible = this.PageContext.ForumModeratorAccess && !this.PostData.IsLocked;
            this.MovePost.NavigateUrl = this.Move.NavigateUrl = BuildLink.GetLink(
                                            ForumPages.MoveMessage,
                                            "m={0}",
                                            this.PostData.MessageId);
            this.Delete.Visible = this.Delete2.Visible =
                                      !this.PostData.PostDeleted && this.PostData.CanDeletePost
                                                                 && !this.PostData.IsLocked;

            this.Delete.NavigateUrl = this.Delete2.NavigateUrl = BuildLink.GetLink(
                                          ForumPages.DeleteMessage,
                                          "m={0}&action=delete",
                                          this.PostData.MessageId);
            this.UnDelete.Visible = this.UnDelete2.Visible = this.PostData.CanUnDeletePost && !this.PostData.IsLocked;
            this.UnDelete.NavigateUrl = this.UnDelete2.NavigateUrl = BuildLink.GetLink(
                                            ForumPages.DeleteMessage,
                                            "m={0}&action=undelete",
                                            this.PostData.MessageId);

            this.Quote.Visible = this.Quote2.Visible =
                                     this.Reply.Visible =
                                         this.ReplyFooter.Visible =
                                             this.QuickReplyLink.Visible =
                                                 !this.PostData.PostDeleted && this.PostData.CanReply
                                                                            && !this.PostData.IsLocked;

            if (!this.PostData.PostDeleted && this.PostData.CanReply
                                           && !this.PostData.IsLocked)
            {
                this.ContextMenu.Attributes.Add(
                    "data-url", 
                    BuildLink.GetLink(
                        ForumPages.PostMessage,
                        "t={0}&f={1}",
                        this.PageContext.PageTopicID,
                        this.PageContext.PageForumID));

                this.ContextMenu.Attributes.Add(
                    "data-quote",
                    this.GetText("COMMON", "SELECTED_QUOTE"));
            }

            this.ContextMenu.Attributes.Add(
                "data-search",
                this.GetText("COMMON", "SELECTED_SEARCH"));

            if (!this.PageContext.IsMobileDevice)
            {
                this.Quote.Text = this.GetText("BUTTON_QUOTE_TT");
                this.ReplyFooter.Text = this.GetText("REPLY");
            }

            this.MultiQuote.Visible = !this.PostData.PostDeleted && this.PostData.CanReply && !this.PostData.IsLocked;

            this.Quote.NavigateUrl = this.Quote2.NavigateUrl = BuildLink.GetLink(
                                         ForumPages.PostMessage,
                                         "t={0}&f={1}&q={2}",
                                         this.PageContext.PageTopicID,
                                         this.PageContext.PageForumID,
                                         this.PostData.MessageId);

            this.Reply.NavigateUrl = this.ReplyFooter.NavigateUrl = BuildLink.GetLink(
                                         ForumPages.PostMessage,
                                         "t={0}&f={1}",
                                         this.PageContext.PageTopicID,
                                         this.PageContext.PageForumID);

            if (this.MultiQuote.Visible)
            {
                this.MultiQuote.Attributes.Add(
                    "onclick",
                    $"handleMultiQuoteButton(this, '{this.PostData.MessageId}', '{this.PostData.TopicId}')");

                this.PageContext.PageElements.RegisterJsBlockStartup(
                    "MultiQuoteButtonJs",
                    JavaScriptBlocks.MultiQuoteButtonJs);
                this.PageContext.PageElements.RegisterJsBlockStartup(
                    "MultiQuoteCallbackSuccessJS",
                    JavaScriptBlocks.MultiQuoteCallbackSuccessJs);

                var icon = new Icon { IconName = "quote-left", IconNameBadge = "plus" };

                this.MultiQuote.Text = this.PageContext.IsMobileDevice
                                           ? icon.RenderToString()
                                           : $"{icon.RenderToString()}&nbsp;{this.GetText("BUTTON_MULTI_QUOTE")}";

                this.MultiQuote.ToolTip = this.GetText("BUTTON_MULTI_QUOTE_TT");
            }

            if (this.PageContext.BoardSettings.EnableUserReputation)
            {
                this.AddReputationControls();
            }

            if (this.Edit.Visible || this.Delete.Visible || this.MovePost.Visible)
            {
                this.ManageDropPlaceHolder.Visible = true;
            }
            else
            {
                this.ManageDropPlaceHolder.Visible = false;
            }

            this.PageContext.PageElements.RegisterJsBlockStartup(
                "asynchCallFailedJs",
                "function CallFailed(res){console.log(res);  }");

            this.FormatThanksRow();

            this.ShowIpInfo();

            this.panMessage.CssClass = "col";

            var avatarUrl = this.Get<IAvatars>().GetAvatarUrlForUser(
                this.PostData.UserId,
                this.DataSource.Avatar,
                this.DataSource.HasAvatarImage,
                string.Empty);

            var displayName = this.PageContext.BoardSettings.EnableDisplayName
                ? this.DataSource.DisplayName
                : this.DataSource.UserName;

            if (avatarUrl.IsSet())
            {
                this.Avatar.Visible = true;
                this.Avatar.AlternateText = displayName;
                this.Avatar.ToolTip = displayName;
                this.Avatar.ImageUrl = avatarUrl;
            }
            else
            {
                this.Avatar.Visible = false;
            }

            // report post
            if (this.Get<IPermissions>().Check(this.PageContext.BoardSettings.ReportPostPermissions)
                && !this.PostData.PostDeleted)
            {
                if (!this.PageContext.IsGuest && this.PageContext.MembershipUser != null)
                {
                    this.ReportPost.Visible = this.ReportPost2.Visible = true;

                    this.ReportPost.NavigateUrl = this.ReportPost2.NavigateUrl = BuildLink.GetLink(
                                                      ForumPages.ReportPost,
                                                      "m={0}",
                                                      this.PostData.MessageId);
                }
            }

            // mark post as answer
            if (!this.PostData.PostDeleted && !this.PageContext.IsGuest && this.PageContext.MembershipUser != null
                && this.PageContext.PageUserID.Equals(this.DataSource.TopicOwnerID)
                && !this.PostData.UserId.Equals(this.PageContext.PageUserID))
            {
                this.MarkAsAnswer.Visible = true;

                if (this.PostData.PostIsAnswer)
                {
                    this.MarkAsAnswer.TextLocalizedTag = "MARK_ANSWER_REMOVE";
                    this.MarkAsAnswer.TitleLocalizedTag = "MARK_ANSWER_REMOVE_TITLE";
                    this.MarkAsAnswer.Icon = "minus-square";
                    this.MarkAsAnswer.IconColor = "text-danger";
                }
                else
                {
                    this.MarkAsAnswer.TextLocalizedTag = "MARK_ANSWER";
                    this.MarkAsAnswer.TitleLocalizedTag = "MARK_ANSWER_TITLE";
                    this.MarkAsAnswer.Icon = "check-square";
                    this.MarkAsAnswer.IconColor = "text-success";
                }
            }

            if (this.ReportPost.Visible == false && this.MarkAsAnswer.Visible == false
                                                 && this.ReportPost.Visible == false
                                                 && this.ManageDropPlaceHolder.Visible == false)
            {
                this.ToolsHolder.Visible = false;
            }

            if (this.ThanksDataLiteral.Visible == false
                                                 && this.Thank.Visible == false
                                                 && this.Quote.Visible == false && this.MultiQuote.Visible == false)
            {
                this.Footer.Visible = false;
            }

            base.OnPreRender(e);
        }

        /// <summary>
        /// Marks as answer click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void MarkAsAnswerClick([NotNull] object sender, [NotNull] EventArgs e)
        {
            var messageFlags = new MessageFlags(this.PostData.DataRow.Flags) { IsAnswer = true };

            if (this.PostData.PostIsAnswer)
            {
                // Remove Current Message 
                messageFlags.IsAnswer = false;

                this.GetRepository<Message>().UpdateFlags(this.PostData.MessageId, messageFlags.BitValue);

                this.GetRepository<Topic>().RemoveAnswerMessage(this.PageContext.PageTopicID);
            }
            else
            {
                // Check for duplicates
                var answerMessageId = this.GetRepository<Topic>().GetAnswerMessage(this.PageContext.PageTopicID);

                if (answerMessageId != null)
                {
                    var message = this.GetRepository<Message>().GetById(answerMessageId.Value);

                    var oldMessageFlags = new MessageFlags(message.Flags) { IsAnswer = false };

                    this.GetRepository<Message>().UpdateFlags(message.ID, oldMessageFlags.BitValue);
                }

                messageFlags.IsAnswer = true;

                this.GetRepository<Topic>().SetAnswerMessage(this.PageContext.PageTopicID, this.PostData.MessageId);

                this.GetRepository<Message>().UpdateFlags(this.PostData.MessageId, messageFlags.BitValue);
            }

            BuildLink.Redirect(
                ForumPages.Posts,
                "m={0}&name={1}#post{0}",
                this.PostData.MessageId,
                this.PageContext.PageTopicID);
        }

        /// <summary>
        /// Formats the thanks information.
        /// </summary>
        /// <returns>
        /// The format thanks info.
        /// </returns>
        [NotNull]
        protected string FormatThanksInfo()
        {
            var sb = new StringBuilder();

            sb.AppendFormat("<ol id=\"popover-list-{0}\">", this.PostData.MessageId);
            sb.Append("</ol>");

            return sb.ToString();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit([NotNull] EventArgs e)
        {
            base.OnInit(e);
        }

        /// <summary>
        /// The page_ load.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Page_Load([NotNull] object sender, [NotNull] EventArgs e)
        {
            this.UserProfileLink.UserID = this.PostData.UserId;
            this.UserProfileLink.ReplaceName = this.PageContext.BoardSettings.EnableDisplayName
                ? this.DataSource.DisplayName
                : this.DataSource.UserName;
            this.UserProfileLink.PostfixText = this.DataSource.IP == "NNTP"
                                                   ? this.GetText("EXTERNALUSER")
                                                   : string.Empty;
            this.UserProfileLink.Style = this.DataSource.Style;

            this.UserProfileLink.Suspended = this.DataSource.Suspended;

            if (this.IsGuest)
            {
                return;
            }

            // Set up popup menu if it's not a guest.
            this.SetupPopupMenu();
        }

        /// <summary>
        /// Adds the user reputation.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        protected void AddUserReputation(object sender, EventArgs e)
        {
            if (!this.Get<IReputation>().CheckIfAllowReputationVoting(this.DataSource.ReputationVoteDate))
            {
                return;
            }

            this.AddReputation.Visible = false;
            this.RemoveReputation.Visible = false;

            this.GetRepository<User>().AddPoints(this.PostData.UserId, this.PageContext.PageUserID, 1);

            this.DataSource.ReputationVoteDate = DateTime.UtcNow;

            this.DataSource.Points = this.DataSource.Points + 1;

            this.PageContext.AddLoadMessage(
                this.GetTextFormatted(
                    "REP_VOTE_UP_MSG",
                    this.Get<HttpServerUtilityBase>().HtmlEncode(
                        this.PageContext.BoardSettings.EnableDisplayName
                            ? this.DataSource.DisplayName
                            : this.DataSource.UserName)),
                MessageTypes.success);
        }

        /// <summary>
        /// Removes the user reputation.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        protected void RemoveUserReputation(object sender, EventArgs e)
        {
            if (!this.Get<IReputation>().CheckIfAllowReputationVoting(this.DataSource.ReputationVoteDate))
            {
                return;
            }

            this.AddReputation.Visible = false;
            this.RemoveReputation.Visible = false;

            this.GetRepository<User>().RemovePoints(this.PostData.UserId, this.PageContext.PageUserID, 1);

            this.DataSource.ReputationVoteDate = DateTime.UtcNow;

            this.DataSource.Points = this.DataSource.Points - 1;

            this.PageContext.AddLoadMessage(
                this.GetTextFormatted(
                    "REP_VOTE_DOWN_MSG",
                    this.Get<HttpServerUtilityBase>().HtmlEncode(
                        this.PageContext.BoardSettings.EnableDisplayName
                            ? this.DataSource.DisplayName
                            : this.DataSource.UserName)),
                MessageTypes.success);
        }

        /// <summary>
        /// Shows the IP information.
        /// </summary>
        private void ShowIpInfo()
        {
            // Display admin/moderator only info
            if (!this.PageContext.IsAdmin && (!this.PageContext.BoardSettings.AllowModeratorsViewIPs
                                              || !this.PageContext.ForumModeratorAccess))
            {
                return;
            }

            // We should show IP
            this.IPInfo.Visible = true;
            this.IPHolder.Visible = true;
            var ip = IPHelper.GetIp4Address(this.PostData.DataRow.IP);
            this.IPLink1.HRef = string.Format(this.PageContext.BoardSettings.IPInfoPageURL, ip);
            this.IPLink1.Title = this.GetText("COMMON", "TT_IPDETAILS");
            this.IPLink1.InnerText = this.HtmlEncode(ip);
        }

        /// <summary>
        /// Setup the popup menu.
        /// </summary>
        private void SetupPopupMenu()
        {
            this.UserDropHolder.Controls.Add(
                new ThemeButton
                {
                    Type = ButtonStyle.None,
                    Icon = "th-list",
                    TextLocalizedPage = "PAGE",
                    TextLocalizedTag = "SEARCHUSER",
                    CssClass = "dropdown-item",
                    NavigateUrl = BuildLink.GetLink(
                        ForumPages.Search,
                        "postedby={0}",
                        this.PageContext.BoardSettings.EnableDisplayName
                            ? this.DataSource.DisplayName
                            : this.DataSource.UserName)
                });
            this.UserDropHolder2.Controls.Add(
                new ThemeButton
                {
                    Type = ButtonStyle.None,
                    Icon = "th-list",
                    TextLocalizedPage = "PAGE",
                    TextLocalizedTag = "SEARCHUSER",
                    CssClass = "dropdown-item",
                    NavigateUrl = BuildLink.GetLink(
                        ForumPages.Search,
                        "postedby={0}",
                        this.PageContext.BoardSettings.EnableDisplayName
                            ? this.DataSource.DisplayName
                            : this.DataSource.UserName)
                });

            if (this.PageContext.IsAdmin)
            {
                this.UserDropHolder.Controls.Add(
                    new ThemeButton
                        {
                            Type = ButtonStyle.None,
                            Icon = "cogs",
                            TextLocalizedPage = "POSTS",
                            TextLocalizedTag = "EDITUSER",
                            CssClass = "dropdown-item",
                            NavigateUrl = BuildLink.GetLink(ForumPages.Admin_EditUser, "u={0}", this.PostData.UserId)
                        });
                this.UserDropHolder2.Controls.Add(
                    new ThemeButton
                        {
                            Type = ButtonStyle.None,
                            Icon = "cogs",
                            TextLocalizedPage = "POSTS",
                            TextLocalizedTag = "EDITUSER",
                            CssClass = "dropdown-item",
                            NavigateUrl = BuildLink.GetLink(ForumPages.Admin_EditUser, "u={0}", this.PostData.UserId)
                        });
            }

            if (this.PageContext.PageUserID != this.PostData.UserId)
            {
                if (this.Get<IUserIgnored>().IsIgnored(this.PostData.UserId))
                {
                    var showButton = new ThemeButton
                                         {
                                             Type = ButtonStyle.None,
                                             Icon = "eye",
                                             TextLocalizedPage = "POSTS",
                                             TextLocalizedTag = "TOGGLEUSERPOSTS_SHOW",
                                             CssClass = "dropdown-item"
                                         };

                    showButton.Click += (sender, args) =>
                        {
                            this.Get<IUserIgnored>().RemoveIgnored(this.PostData.UserId);
                            this.Get<HttpResponseBase>().Redirect(this.Get<HttpRequestBase>().RawUrl);
                        };

                    this.UserDropHolder.Controls.Add(showButton);
                }
                else
                {
                    var hideButton = new ThemeButton
                                         {
                                             Type = ButtonStyle.None,
                                             Icon = "eye-slash",
                                             TextLocalizedPage = "POSTS",
                                             TextLocalizedTag = "TOGGLEUSERPOSTS_HIDE",
                                             CssClass = "dropdown-item"
                                         };

                    hideButton.Click += (sender, args) =>
                        {
                            this.Get<IUserIgnored>().AddIgnored(this.PostData.UserId);
                            this.Get<HttpResponseBase>().Redirect(this.Get<HttpRequestBase>().RawUrl);
                        };

                    this.UserDropHolder.Controls.Add(hideButton);
                }
            }

            if (this.PageContext.BoardSettings.EnableBuddyList && this.PageContext.PageUserID != this.PostData.UserId)
            {
                var userBlockFlags = new UserBlockFlags(this.DataSource.BlockFlags); 

                // Should we add the "Add Buddy" item?
                if (!this.Get<IFriends>().IsBuddy(this.PostData.UserId, false) && !this.PageContext.IsGuest
                                                                             && !userBlockFlags
                                                                                 .BlockFriendRequests)
                {
                    var addFriendButton = new ThemeButton
                                              {
                                                  Type = ButtonStyle.None,
                                                  Icon = "user-plus",
                                                  TextLocalizedPage = "BUDDY",
                                                  TextLocalizedTag = "ADDBUDDY",
                                                  CssClass = "dropdown-item"
                                              };

                    var userName = this.PageContext.BoardSettings.EnableDisplayName
                        ? this.DataSource.DisplayName
                        : this.DataSource.UserName;

                    addFriendButton.Click += (sender, args) =>
                        {
                            if (this.Get<IFriends>().AddRequest(this.PostData.UserId))
                            {
                                this.PageContext.AddLoadMessage(
                                    this.GetTextFormatted("NOTIFICATION_BUDDYAPPROVED_MUTUAL", userName),
                                    MessageTypes.success);

                                this.Get<HttpResponseBase>().Redirect(this.Get<HttpRequestBase>().RawUrl);
                            }
                            else
                            {
                                this.PageContext.AddLoadMessage(
                                    this.GetText("NOTIFICATION_BUDDYREQUEST"),
                                    MessageTypes.success);
                            }
                        };

                    this.UserDropHolder.Controls.Add(addFriendButton);
                }
                else if (this.Get<IFriends>().IsBuddy(this.PostData.UserId, true) && !this.PageContext.IsGuest)
                {
                    // Are the users approved buddies? Add the "Remove buddy" item.
                    var removeFriendButton = new ThemeButton
                                                 {
                                                     Type = ButtonStyle.None,
                                                     Icon = "user-times",
                                                     TextLocalizedPage = "BUDDY",
                                                     TextLocalizedTag = "REMOVEBUDDY",
                                                     CssClass = "dropdown-item",
                                                     ReturnConfirmText = this.GetText(
                                                         "FRIENDS",
                                                         "NOTIFICATION_REMOVE")
                                                 };

                    removeFriendButton.Click += (sender, args) =>
                        {
                            this.Get<IFriends>().Remove(this.PostData.UserId);

                            this.Get<HttpResponseBase>().Redirect(this.Get<HttpRequestBase>().RawUrl);

                            this.PageContext.AddLoadMessage(
                                this.GetTextFormatted(
                                    "REMOVEBUDDY_NOTIFICATION",
                                    this.Get<IFriends>().Remove(this.PostData.UserId)),
                                MessageTypes.success);
                        };

                    this.UserDropHolder.Controls.Add(removeFriendButton);
                }
            }

            this.UserDropHolder.Controls.Add(new Panel { CssClass = "dropdown-divider" });
            this.UserDropHolder2.Controls.Add(new Panel { CssClass = "dropdown-divider" });
        }

        /// <summary>
        /// Add Reputation Controls to the User PopMenu
        /// </summary>
        private void AddReputationControls()
        {
            if (this.PageContext.PageUserID != this.PostData.UserId && this.PageContext.BoardSettings.EnableUserReputation
                                                                    && !this.IsGuest && !this.PageContext.IsGuest)
            {
                if (!this.Get<IReputation>().CheckIfAllowReputationVoting(this.DataSource.ReputationVoteDate))
                {
                    return;
                }

                // Check if the User matches minimal requirements for voting up
                if (this.PageContext.User.Points >= this.PageContext.BoardSettings.ReputationMinUpVoting)
                {
                    this.AddReputation.Visible = true;
                }

                // Check if the User matches minimal requirements for voting down
                if (this.PageContext.User.Points < this.PageContext.BoardSettings.ReputationMinDownVoting)
                {
                    return;
                }

                // Check if the Value is 0 or Bellow
                if (this.DataSource.Points > 0 && this.PageContext.BoardSettings.ReputationAllowNegative)
                {
                    this.RemoveReputation.Visible = true;
                }
            }
            else
            {
                this.AddReputation.Visible = false;
                this.RemoveReputation.Visible = false;
            }
        }

        /// <summary>
        /// Do thanks row formatting.
        /// </summary>
        private void FormatThanksRow()
        {
            if (this.PostData.PostDeleted || this.PostData.IsLocked)
            {
                return;
            }

            // Register Javascript
            var addThankBoxHTML =
                this.PageContext.IsMobileDevice ?
                "'<a class=\"btn btn-link\" href=\"javascript:addThanks(' + response.MessageID + ');\" onclick=\"jQuery(this).blur();\" title=' + response.Title + '><span><i class=\"fas fa-heart text-danger fa-fw\"></i></span></a>'" :
                "'<a class=\"btn btn-link\" href=\"javascript:addThanks(' + response.MessageID + ');\" onclick=\"jQuery(this).blur();\" title=' + response.Title + '><span><i class=\"fas fa-heart text-danger fa-fw\"></i>&nbsp;' + response.Text + '</span></a>'";

            var removeThankBoxHTML =
                this.PageContext.IsMobileDevice ?
                "'<a class=\"btn btn-link\" href=\"javascript:removeThanks(' + response.MessageID + ');\" onclick=\"jQuery(this).blur();\" title=' + response.Title + '><span><i class=\"far fa-heart fa-fw\"></i></a>'" :
                "'<a class=\"btn btn-link\" href=\"javascript:removeThanks(' + response.MessageID + ');\" onclick=\"jQuery(this).blur();\" title=' + response.Title + '><span><i class=\"far fa-heart fa-fw\"></i>&nbsp;' + response.Text + '</span></a>'";

            var thanksJs = "{0}{1}{2}".Fmt(
                JavaScriptBlocks.AddThanksJs(removeThankBoxHTML),
                Environment.NewLine,
                JavaScriptBlocks.RemoveThanksJs(addThankBoxHTML));

            this.PageContext.PageElements.RegisterJsBlockStartup("ThanksJs", thanksJs);

            this.Thank.Visible = this.PostData.CanThankPost && !this.PageContext.IsGuest;

            if (this.DataSource.IsThankedByUser)
            {
                this.Thank.NavigateUrl = $"javascript:removeThanks({this.DataSource.MessageID});";

                if (!this.PageContext.IsMobileDevice)
                {
                    this.Thank.Text = this.GetText("BUTTON_THANKSDELETE");
                }

                this.Thank.TitleLocalizedTag = "BUTTON_THANKSDELETE_TT";
                this.Thank.Icon = "heart";
                this.Thank.IconCssClass = "far";
            }
            else
            {
                this.Thank.NavigateUrl = $"javascript:addThanks({this.DataSource.MessageID});";

                if (!this.PageContext.IsMobileDevice)
                {
                    this.Thank.Text = this.GetText("BUTTON_THANKS");
                }

                this.Thank.TitleLocalizedTag = "BUTTON_THANKS_TT";
                this.Thank.Icon = "heart";
                this.Thank.IconCssClass = "fas";
                this.Thank.IconColor = "text-danger";
            }

            var thanksNumber = this.DataSource.ThanksNumber;

            if (thanksNumber == 0)
            {
                return;
            }

            var username = this.HtmlEncode(
                this.PageContext.BoardSettings.EnableDisplayName ? this.DataSource.DisplayName : this.DataSource.UserName);

            var thanksLabelText = thanksNumber == 1
                                      ? this.Get<ILocalization>().GetTextFormatted("THANKSINFOSINGLE", username)
                                      : this.Get<ILocalization>().GetTextFormatted(
                                          "THANKSINFO",
                                          thanksNumber,
                                          username);

            this.ThanksDataLiteral.Text = $@"<a class=""btn btn-link thanks-popover"" 
                           data-toggle=""popover"" 
                           data-trigger=""click hover""
                           data-html=""true""
                           data-messageid=""{this.PostData.MessageId}""
                           data-url=""{BoardInfo.ForumClientFileRoot}{WebApiConfig.UrlPrefix}""
                           title=""{thanksLabelText}"" 
                           data-content=""{this.FormatThanksInfo().ToJsString()}"">
                           <i class=""fa fa-heart"" style=""color:#e74c3c""></i>&nbsp;+{thanksNumber}
                  </a>";
            
            this.ThanksDataLiteral.Visible = true;
        }

        #endregion
    }
}