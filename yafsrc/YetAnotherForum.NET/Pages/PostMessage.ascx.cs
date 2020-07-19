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

namespace YAF.Pages
{
    #region Using

    using System;
    using System.Linq;
    using System.Web;
    
    using YAF.Configuration;
    using YAF.Core.BaseModules;
    using YAF.Core.BasePages;
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
    using YAF.Types.Objects;
    using YAF.Utils;
    using YAF.Utils.Helpers;
    using YAF.Web.Extensions;

    using ListItem = System.Web.UI.WebControls.ListItem;

    #endregion

    /// <summary>
    /// The post message Page.
    /// </summary>
    public partial class PostMessage : ForumPage
    {
        #region Constants and Fields

        /// <summary>
        ///   The forum editor.
        /// </summary>
        private ForumEditor forumEditor;

        /// <summary>
        ///   The owner user id.
        /// </summary>
        private int ownerUserId;

        /// <summary>
        ///   The Spam Approved Indicator
        /// </summary>
        private bool spamApproved = true;

        /// <summary>
        /// The edit or quoted message.
        /// </summary>
        private Tuple<Topic, Message, User, Forum> editOrQuotedMessage;

        /// <summary>
        ///   The forum.
        /// </summary>
        private Topic topic;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PostMessage"/> class.
        /// </summary>
        public PostMessage()
            : base("POSTMESSAGE")
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Gets EditMessageID.
        /// </summary>
        protected int? EditMessageId => this.Get<HttpRequestBase>().QueryString.GetFirstOrDefaultAsInt("m");

        /// <summary>
        ///   Gets or sets the PollId if the topic has a poll attached
        /// </summary>
        protected int? PollId { get; set; }

        /// <summary>
        ///   Gets Quoted Message ID.
        /// </summary>
        protected int? QuotedMessageId => this.Get<HttpRequestBase>().QueryString.GetFirstOrDefaultAsInt("q");

        /// <summary>
        ///   Gets TopicID.
        /// </summary>
        protected int? TopicId => this.Get<HttpRequestBase>().QueryString.GetFirstOrDefaultAsInt("t");

        #endregion

        #region Methods

        /// <summary>
        /// Canceling Posting New Message Or editing Message.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Cancel_Click([NotNull] object sender, [NotNull] EventArgs e)
        {
            if (this.TopicId != null || this.EditMessageId.HasValue)
            {
                // reply to existing topic or editing of existing topic
                BuildLink.Redirect(
                    ForumPages.Posts,
                    "t={0}&name={1}",
                    this.PageContext.PageTopicID,
                    this.PageContext.PageTopicName);
            }
            else
            {
                // new topic -- cancel back to forum
                BuildLink.Redirect(
                    ForumPages.Topics,
                    "f={0}&name={1}",
                    this.PageContext.PageForumID,
                    this.PageContext.PageForumName);
            }
        }

        /// <summary>
        /// The get poll id.
        /// </summary>
        /// <returns>
        /// Returns the Poll Id
        /// </returns>
        protected int? GetPollID()
        {
            return this.PollId;
        }

        /// <summary>
        /// Verifies the user isn't posting too quickly, if so, tells them to wait.
        /// </summary>
        /// <returns>
        /// True if there is a delay in effect.
        /// </returns>
        protected bool IsPostReplyDelay()
        {
            // see if there is a post delay
            if (this.PageContext.IsAdmin || this.PageContext.ForumModeratorAccess
                || this.PageContext.BoardSettings.PostFloodDelay <= 0)
            {
                return false;
            }

            // see if they've past that delay point
            if (this.Get<ISession>().LastPost
                <= DateTime.UtcNow.AddSeconds(-this.PageContext.BoardSettings.PostFloodDelay)
                || this.EditMessageId.HasValue)
            {
                return false;
            }

            this.PageContext.AddLoadMessage(
                this.GetTextFormatted(
                    "wait",
                    (this.Get<ISession>().LastPost
                     - DateTime.UtcNow.AddSeconds(-this.PageContext.BoardSettings.PostFloodDelay)).Seconds),
                MessageTypes.warning);
            return true;
        }

        /// <summary>
        /// Handles verification of the PostReply. Adds java script message if there is a problem.
        /// </summary>
        /// <returns>
        /// true if everything is verified
        /// </returns>
        protected bool IsPostReplyVerified()
        {
            // To avoid posting whitespace(s) or empty messages
            var postedMessage = this.forumEditor.Text.Trim();

            if (postedMessage.IsNotSet())
            {
                this.PageContext.AddLoadMessage(this.GetText("ISEMPTY"), MessageTypes.warning);
                return false;
            }

            // No need to check whitespace if they are actually posting something
            if (this.PageContext.BoardSettings.MaxPostSize > 0
                && this.forumEditor.Text.Length >= this.PageContext.BoardSettings.MaxPostSize)
            {
                this.PageContext.AddLoadMessage(this.GetText("ISEXCEEDED"), MessageTypes.warning);
                return false;
            }

            // Check if the Entered Guest Username is not too long
            if (this.FromRow.Visible && this.From.Text.Trim().Length > 100)
            {
                this.PageContext.AddLoadMessage(this.GetText("GUEST_NAME_TOOLONG"), MessageTypes.warning);

                this.From.Text = this.From.Text.Substring(100);
                return false;
            }

            if (this.SubjectRow.Visible && this.TopicSubjectTextBox.Text.IsNotSet())
            {
                this.PageContext.AddLoadMessage(this.GetText("NEED_SUBJECT"), MessageTypes.warning);
                return false;
            }

            if (!this.Get<IPermissions>().Check(this.PageContext.BoardSettings.AllowCreateTopicsSameName)
                && this.GetRepository<Topic>().CheckForDuplicateTopic(this.TopicSubjectTextBox.Text.Trim()) && this.TopicId == null
                && !this.EditMessageId.HasValue)
            {
                this.PageContext.AddLoadMessage(this.GetText("SUBJECT_DUPLICATE"), MessageTypes.warning);
                return false;
            }

            if ((!this.PageContext.IsGuest || !this.PageContext.BoardSettings.EnableCaptchaForGuests)
                && (!this.PageContext.BoardSettings.EnableCaptchaForPost || this.PageContext.IsCaptchaExcluded)
                || CaptchaHelper.IsValid(this.tbCaptcha.Text.Trim()))
            {
                return true;
            }

            this.PageContext.AddLoadMessage(this.GetText("BAD_CAPTCHA"), MessageTypes.danger);
            return false;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnInit([NotNull] EventArgs e)
        {
            if (this.PageContext.ForumUploadAccess)
            {
                this.PageContext.PageElements.AddScriptReference("FileUploadScript");

#if DEBUG
                this.PageContext.PageElements.RegisterCssIncludeContent("jquery.fileupload.comb.css");
#else
                this.PageContext.PageElements.RegisterCssIncludeContent("jquery.fileupload.comb.min.css");
#endif
            }

            this.forumEditor = ForumEditorHelper.GetCurrentForumEditor();
            this.forumEditor.MaxCharacters = this.PageContext.BoardSettings.MaxPostSize;

            this.EditorLine.Controls.Add(this.forumEditor);

            base.OnInit(e);
        }

        /// <summary>
        /// Registers the java scripts
        /// </summary>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected override void OnPreRender([NotNull] EventArgs e)
        {
            // setup jQuery and Jquery Ui Tabs.
            this.PageContext.PageElements.RegisterJsBlock(
                "GetBoardTagsJs",
                JavaScriptBlocks.GetBoardTagsJs(this.Tags.ClientID));

            base.OnPreRender(e);
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load([NotNull] object sender, [NotNull] EventArgs e)
        {
            if (this.PageContext.PageForumID == 0)
            {
                BuildLink.AccessDenied();
            }

            if (this.Get<HttpRequestBase>()["t"] == null && this.Get<HttpRequestBase>()["m"] == null
                                                         && !this.PageContext.ForumPostAccess)
            {
                BuildLink.AccessDenied();
            }

            if (this.Get<HttpRequestBase>()["t"] != null && !this.PageContext.ForumReplyAccess)
            {
                BuildLink.AccessDenied();
            }

            this.topic = this.GetRepository<Topic>().GetById(this.PageContext.PageTopicID);

            // we reply to a post with a quote
            if (this.QuotedMessageId.HasValue)
            {
                var quotedMessage =
                    this.GetRepository<Message>().GetMessage(this.QuotedMessageId.Value);

                if (quotedMessage != null)
                {
                    if (this.Get<HttpRequestBase>().QueryString.Exists("text"))
                    {
                        var quotedMessageText =
                            this.Server.UrlDecode(this.Get<HttpRequestBase>().QueryString.GetFirstOrDefault("text"));

                        quotedMessage.Item2.MessageText =
                            HtmlHelper.StripHtml(HtmlHelper.CleanHtmlString(quotedMessageText));
                    }

                    if (quotedMessage.Item2.TopicID != this.PageContext.PageTopicID)
                    {
                        BuildLink.AccessDenied();
                    }

                    if (!this.CanQuotePostCheck(this.topic))
                    {
                        BuildLink.AccessDenied();
                    }
                }

                this.editOrQuotedMessage = quotedMessage;
            }
            else if (this.EditMessageId.HasValue)
            {
                var editMessage = this.GetRepository<Message>().GetMessage(this.EditMessageId.Value);

                if (editMessage != null)
                {
                    this.ownerUserId = editMessage.Item1.UserID;

                    if (!this.CanEditPostCheck(editMessage.Item2, this.topic))
                    {
                        BuildLink.AccessDenied();
                    }
                }

                this.editOrQuotedMessage = editMessage;

                // we edit message and should transfer both the message ID and TopicID for PageLinks.
                this.PollList.EditMessageId = this.EditMessageId.Value;
            }

            this.PollId = this.topic.PollID;
            this.PollList.TopicId = this.topic.ID;

            this.HandleUploadControls();

            if (!this.IsPostBack)
            {
                var normal = new ListItem(this.GetText("normal"), "0");

                normal.Attributes.Add(
                    "data-content",
                    $"<span class='select2-image-select-icon'><i class='far fa-comment fa-fw text-secondary'></i>&nbsp;{this.GetText("normal")}</span>");

                this.Priority.Items.Add(normal);

                var sticky = new ListItem(this.GetText("sticky"), "1");

                sticky.Attributes.Add(
                    "data-content",
                    $"<span class='select2-image-select-icon'><i class='fas fa-thumbtack fa-fw text-secondary'></i>&nbsp;{this.GetText("sticky")}</span>");

                this.Priority.Items.Add(sticky);

                var announcement = new ListItem(this.GetText("announcement"), "2");

                announcement.Attributes.Add(
                    "data-content",
                    $"<span class='select2-image-select-icon'><i class='fas fa-bullhorn fa-fw text-secondary'></i>&nbsp;{this.GetText("announcement")}</span>");

                this.Priority.Items.Add(announcement);

                this.Priority.SelectedIndex = 0;

                // Allow the Styling of Topic Titles only for Mods or Admins
                if (this.PageContext.BoardSettings.UseStyledTopicTitles
                    && (this.PageContext.ForumModeratorAccess || this.PageContext.IsAdmin))
                {
                    this.StyleRow.Visible = true;
                }
                else
                {
                    this.StyleRow.Visible = false;
                }

                this.EditReasonRow.Visible = false;

                this.PriorityRow.Visible = this.PageContext.ForumPriorityAccess;

                // update options...
                this.PostOptions1.Visible = !this.PageContext.IsGuest;
                this.PostOptions1.PersistentOptionVisible =
                    this.PageContext.IsAdmin || this.PageContext.ForumModeratorAccess;
                this.PostOptions1.WatchOptionVisible = !this.PageContext.IsGuest;
                this.PostOptions1.PollOptionVisible = false;

                if (!this.PageContext.IsGuest)
                {
                    this.PostOptions1.WatchChecked = this.PageContext.PageTopicID > 0
                        ? this.GetRepository<WatchTopic>().Check(this.PageContext.PageUserID, this.PageContext.PageTopicID).HasValue
                        : this.PageContext.CurrentUser.AutoWatchTopics;
                }

                if (this.PageContext.IsGuest && this.PageContext.BoardSettings.EnableCaptchaForGuests
                    || this.PageContext.BoardSettings.EnableCaptchaForPost && !this.PageContext.IsCaptchaExcluded)
                {
                    this.imgCaptcha.ImageUrl = $"{BoardInfo.ForumClientFileRoot}resource.ashx?c=1";
                    this.tr_captcha1.Visible = true;
                    this.tr_captcha2.Visible = true;
                }

                if (this.PageContext.Settings.LockedForum == 0)
                {
                    this.PageLinks.AddRoot();
                    this.PageLinks.AddCategory(this.PageContext.PageCategoryName, this.PageContext.PageCategoryID);
                }

                this.PageLinks.AddForum(this.PageContext.PageForumID);

                // check if it's a reply to a topic...
                if (this.TopicId != null)
                {
                    this.InitReplyToTopic();

                    this.PollList.TopicId = this.TopicId.ToType<int>();
                }

                if (this.editOrQuotedMessage != null)
                {
                    if (this.QuotedMessageId.HasValue)
                    {
                        if (this.Get<ISession>().MultiQuoteIds != null)
                        {
                            var quoteId = this.Get<HttpRequestBase>().QueryString.GetFirstOrDefault("q").ToType<int>();
                            var multiQuote = new MultiQuote { MessageID = quoteId, TopicID = this.PageContext.PageTopicID };

                            if (
                                !this.Get<ISession>()
                                    .MultiQuoteIds.Any(m => m.MessageID.Equals(quoteId)))
                            {
                                this.Get<ISession>()
                                    .MultiQuoteIds.Add(
                                        multiQuote);
                            }

                            var messages = this.GetRepository<Message>().GetByIds(
                                this.Get<ISession>().MultiQuoteIds.Select(i => i.MessageID));

                            messages.ForEach(this.InitQuotedReply);

                            // Clear Multi-quotes
                            this.Get<ISession>().MultiQuoteIds = null;
                        }
                        else
                        {
                            this.InitQuotedReply(this.editOrQuotedMessage.Item2);
                        }

                        this.PollList.TopicId = this.TopicId.ToType<int>();
                    }
                    else if (this.EditMessageId.HasValue)
                    {
                        // editing a message...
                        this.InitEditedPost(this.editOrQuotedMessage.Item2);
                        this.PollList.EditMessageId = this.EditMessageId.Value;
                    }
                }

                // form user is only for "Guest"
                this.From.Text = this.Get<IUserDisplayName>().GetName(this.PageContext.PageUserID);
                if (!this.PageContext.IsGuest)
                {
                    this.FromRow.Visible = false;
                }
            }

            this.PollList.PollId = this.PollId;
        }

        /// <summary>
        /// The post reply handle edit post.
        /// </summary>
        /// <returns>
        /// Returns the Message Id
        /// </returns>
        protected Message PostReplyHandleEditPost()
        {
            if (!this.PageContext.ForumEditAccess)
            {
                BuildLink.AccessDenied();
            }

            // Update Tags
            if (this.TagsHolder.Visible)
            {
                this.GetRepository<TopicTag>().Delete(t => t.TopicID == this.PageContext.PageTopicID);

                if (this.Tags.Text.IsSet())
                {
                    var tags = this.Tags.Text.Split(',');

                    var boardTags = this.GetRepository<Tag>().GetByBoardId();

                    tags.ForEach(
                        tag =>
                            {
                                var existTag = boardTags.FirstOrDefault(t => t.TagName == tag);

                                if (existTag != null)
                                {
                                    // add to topic
                                    this.GetRepository<TopicTag>().Add(
                                        existTag.ID,
                                        this.PageContext.PageTopicID);
                                }
                                else
                                {
                                    // save new Tag
                                    var newTagId = this.GetRepository<Tag>().Add(tag);

                                    // add to topic
                                    this.GetRepository<TopicTag>().Add(newTagId, this.PageContext.PageTopicID);
                                }
                            });
                }
            }

            var subjectSave = string.Empty;
            var descriptionSave = string.Empty;
            var stylesSave = string.Empty;

            if (this.TopicSubjectTextBox.Enabled)
            {
                subjectSave = this.TopicSubjectTextBox.Text;
            }

            if (this.TopicDescriptionTextBox.Enabled)
            {
                descriptionSave = this.TopicDescriptionTextBox.Text;
            }

            if (this.TopicStylesTextBox.Enabled)
            {
                stylesSave = this.TopicStylesTextBox.Text;
            }

            var editMessage = this.GetRepository<Message>().GetById(this.EditMessageId.Value);

            // Mek Suggestion: This should be removed, resetting flags on edit is a bit lame.
            // Ederon : now it should be better, but all this code around forum/topic/message flags needs revamp
            // retrieve message flags
            var messageFlags = new MessageFlags(editMessage.Flags)
            {
                IsHtml =
                    this
                    .forumEditor
                    .UsesHTML,
                IsBBCode
                    =
                    this
                    .forumEditor
                    .UsesBBCode,
                IsPersistent
                    =
                    this
                    .PostOptions1
                    .PersistentChecked
            };

            editMessage.Flags = messageFlags.BitValue;

            var isModeratorChanged = this.PageContext.PageUserID != this.ownerUserId;

            this.GetRepository<Message>().Update(
                editMessage.ID,
                this.Priority.SelectedValue.ToType<int>(),
                this.forumEditor.Text.Trim(),
                descriptionSave.Trim(),
                string.Empty,
                stylesSave.Trim(),
                subjectSave.Trim(),
                this.HtmlEncode(this.ReasonEditor.Text),
                isModeratorChanged,
                this.PageContext.IsAdmin || this.PageContext.ForumModeratorAccess,
                this.editOrQuotedMessage,
                this.PageContext.PageUserID);

            this.UpdateWatchTopic(this.PageContext.PageUserID, this.PageContext.PageTopicID);

            // remove cache if it exists...
            this.Get<IDataCache>()
                .Remove(string.Format(Constants.Cache.FirstPostCleaned, this.PageContext.PageBoardID, this.TopicId));

            return editMessage;
        }

        /// <summary>
        /// The post reply handle reply to topic.
        /// </summary>
        /// <param name="isSpamApproved">
        /// The is Spam Approved.
        /// </param>
        /// <returns>
        /// Returns the Message Id.
        /// </returns>
        protected int PostReplyHandleReplyToTopic(bool isSpamApproved)
        {
            if (!this.PageContext.ForumReplyAccess)
            {
                BuildLink.AccessDenied();
            }

            // Check if Forum is Moderated
            var isForumModerated = false;

            var forumInfo = this.GetRepository<Forum>()
                .List(this.PageContext.PageBoardID, this.PageContext.PageForumID).FirstOrDefault();

            if (forumInfo != null)
            {
                isForumModerated = this.CheckForumModerateStatus(forumInfo, false);
            }

            // If Forum is Moderated
            if (isForumModerated)
            {
                isSpamApproved = false;
            }

            // Bypass Approval if Admin or Moderator
            if (this.PageContext.IsAdmin || this.PageContext.ForumModeratorAccess)
            {
                isSpamApproved = true;
            }

            object replyTo = this.QuotedMessageId ?? -1;

            // make message flags
            var messageFlags = new MessageFlags
            {
                IsHtml = this.forumEditor.UsesHTML,
                IsBBCode = this.forumEditor.UsesBBCode,
                IsPersistent = this.PostOptions1.PersistentChecked,
                IsApproved = isSpamApproved
            };

            var messageId = this.GetRepository<Message>().SaveNew(
                this.TopicId.Value,
                this.PageContext.PageUserID,
                this.forumEditor.Text,
                this.User != null ? null : this.From.Text,
                this.Get<HttpRequestBase>().GetUserRealIPAddress(),
                DateTime.UtcNow,
                replyTo.ToType<int>(),
                messageFlags);

            this.UpdateWatchTopic(this.PageContext.PageUserID, this.PageContext.PageTopicID);

            if (!messageFlags.IsApproved)
            {
                return messageId;
            }

            this.Get<IDataCache>().Remove(Constants.Cache.BoardStats);
            this.Get<IDataCache>().Remove(Constants.Cache.BoardUserStats);

            return messageId;
        }

        /// <summary>
        /// Handles the PostReply click including: Replying, Editing and New post.
        /// </summary>
        /// <param name="sender">
        /// The Sender Object.
        /// </param>
        /// <param name="e">
        /// The Event Arguments.
        /// </param>
        protected void PostReply_Click([NotNull] object sender, [NotNull] EventArgs e)
        {
            if (!this.IsPostReplyVerified())
            {
                return;
            }

            if (this.IsPostReplyDelay())
            {
                return;
            }

            var isPossibleSpamMessage = false;

            // Check for SPAM
            if (!this.PageContext.IsAdmin && !this.PageContext.ForumModeratorAccess
                && !this.PageContext.BoardSettings.SpamServiceType.Equals(0))
            {
                // Check content for spam
                if (
                    this.Get<ISpamCheck>().CheckPostForSpam(
                        this.PageContext.IsGuest ? this.From.Text : this.PageContext.PageUserName,
                        this.Get<HttpRequestBase>().GetUserRealIPAddress(),
                        BBCodeHelper.StripBBCode(
                            HtmlHelper.StripHtml(HtmlHelper.CleanHtmlString(this.forumEditor.Text)))
                            .RemoveMultipleWhitespace(),
                        this.PageContext.IsGuest ? null : this.PageContext.MembershipUser.Email,
                        out var spamResult))
                {
                    switch (this.PageContext.BoardSettings.SpamMessageHandling)
                    {
                        case 0:
                            this.Logger.Log(
                                this.PageContext.PageUserID,
                                "Spam Message Detected",
                                $"Spam Check detected possible SPAM posted by User: {(this.PageContext.IsGuest ? this.From.Text : this.PageContext.PageUserName)}",
                                EventLogTypes.SpamMessageDetected);
                            break;
                        case 1:
                            this.spamApproved = false;
                            isPossibleSpamMessage = true;
                            this.Logger.Log(
                                this.PageContext.PageUserID,
                                "Spam Message Detected",
                                $"Spam Check detected possible SPAM ({spamResult}) posted by User: {(this.PageContext.IsGuest ? this.From.Text : this.PageContext.PageUserName)}, it was flagged as unapproved post.",
                                EventLogTypes.SpamMessageDetected);
                            break;
                        case 2:
                            this.Logger.Log(
                                this.PageContext.PageUserID,
                                "Spam Message Detected",
                                $"Spam Check detected possible SPAM ({spamResult}) posted by User: {(this.PageContext.IsGuest ? this.From.Text : this.PageContext.PageUserName)}, post was rejected",
                                EventLogTypes.SpamMessageDetected);
                            this.PageContext.AddLoadMessage(this.GetText("SPAM_MESSAGE"), MessageTypes.danger);
                            return;
                        case 3:
                            this.Logger.Log(
                                this.PageContext.PageUserID,
                                "Spam Message Detected",
                                $"Spam Check detected possible SPAM ({spamResult}) posted by User: {(this.PageContext.IsGuest ? this.From.Text : this.PageContext.PageUserName)}, user was deleted and banned",
                                EventLogTypes.SpamMessageDetected);

                            this.Get<IAspNetUsersHelper>().DeleteAndBanUser(
                                this.PageContext.PageUserID,
                                this.PageContext.MembershipUser,
                                this.PageContext.CurrentUser.IP);

                            return;
                    }
                }
            }

            if (this.Get<ISpamCheck>().ContainsSpamUrls(this.forumEditor.Text))
            {
                return;
            }

            // update the last post time...
            this.Get<ISession>().LastPost = DateTime.UtcNow.AddSeconds(30);

            int? messageId = null;

            var isApproved = true;

            if (this.TopicId != null)
            {
                // Reply to topic
                messageId = this.PostReplyHandleReplyToTopic(this.spamApproved);

                isApproved = this.spamApproved;
            }
            else if (this.EditMessageId.HasValue)
            {
                // Edit existing post
                var editMessage = this.PostReplyHandleEditPost();

                // Check if message is approved
                isApproved = editMessage.MessageFlags.IsApproved;

                messageId = editMessage.ID;
            }

            // vzrus^ the poll access controls are enabled and this is a new topic - we add the variables
            var attachPollParameter = string.Empty;
            var returnForum = string.Empty;

            if (this.PageContext.ForumPollAccess && this.PostOptions1.PollOptionVisible && this.TopicId != null)
            {
                // new topic poll token
                attachPollParameter = $"&t={this.TopicId}";

                // new return forum poll token
                returnForum = $"&f={this.PageContext.PageForumID}";
            }

            // Create notification emails
            if (isApproved)
            {
                if (this.EditMessageId == null)
                {
                    this.Get<ISendNotification>().ToWatchingUsers(messageId.Value);
                }

                if (this.EditMessageId == null && !this.PageContext.IsGuest && this.PageContext.CurrentUser.Activity)
                {
                    // Handle Mentions
                    BBCodeHelper.FindMentions(this.forumEditor.Text).ForEach(
                        user =>
                            {
                                var userId = this.Get<IUserDisplayName>().GetId(user).Value;

                                if (userId != this.PageContext.PageUserID)
                                {
                                    this.Get<IActivityStream>().AddMentionToStream(
                                        userId,
                                        this.TopicId.Value,
                                        messageId.ToType<int>(),
                                        this.PageContext.PageUserID);
                                }
                            });

                    // Handle User Quoting
                    BBCodeHelper.FindUserQuoting(this.forumEditor.Text).ForEach(
                        user =>
                            {
                                var userId = this.Get<IUserDisplayName>().GetId(user).Value;

                                if (userId != this.PageContext.PageUserID)
                                {
                                    this.Get<IActivityStream>().AddQuotingToStream(
                                        userId,
                                        this.TopicId.Value,
                                        messageId.ToType<int>(),
                                        this.PageContext.PageUserID);
                                }
                            });

                    this.Get<IActivityStream>().AddReplyToStream(
                        Config.IsDotNetNuke ? this.PageContext.PageForumID : this.PageContext.PageUserID,
                        this.TopicId.Value,
                        messageId.ToType<int>(),
                        this.PageContext.PageTopicName,
                        this.forumEditor.Text);
                }

                if (attachPollParameter.IsNotSet() || !this.PostOptions1.PollChecked)
                {
                    // regular redirect...
                    BuildLink.Redirect(ForumPages.Posts, "m={0}&name={1}#post{0}", messageId, this.PageContext.PageTopicName);
                }
                else
                {
                    // poll edit redirect...
                    BuildLink.Redirect(ForumPages.PollEdit, "{0}", attachPollParameter);
                }
            }
            else
            {
                // Not Approved
                if (this.PageContext.BoardSettings.EmailModeratorsOnModeratedPost)
                {
                    // not approved, notify moderators
                    this.Get<ISendNotification>()
                        .ToModeratorsThatMessageNeedsApproval(
                            this.PageContext.PageForumID,
                            messageId.ToType<int>(),
                            isPossibleSpamMessage);
                }

                // 't' variable is required only for poll and this is a attach poll token for attachments page
                if (!this.PostOptions1.PollChecked)
                {
                    attachPollParameter = string.Empty;
                }

                // Tell user that his message will have to be approved by a moderator
                var url = BuildLink.GetForumLink(this.PageContext.PageForumID, this.PageContext.PageForumName);

                if (this.PageContext.PageTopicID > 0 && this.topic.NumPosts > 1)
                {
                    url = BuildLink.GetTopicLink(this.PageContext.PageTopicID, this.PageContext.PageTopicName);
                }

                if (attachPollParameter.Length <= 0)
                {
                    BuildLink.Redirect(ForumPages.Info, "i=1&url={0}", this.Server.UrlEncode(url));
                }
                else
                {
                    BuildLink.Redirect(ForumPages.PollEdit, "&ra=1{0}{1}", attachPollParameter, returnForum);
                }
            }
        }

        /// <summary>
        /// Previews the new Message
        /// </summary>
        /// <param name="sender">
        /// The Sender Object.
        /// </param>
        /// <param name="e">
        /// The Event Arguments.
        /// </param>
        protected void Preview_Click([NotNull] object sender, [NotNull] EventArgs e)
        {
            this.PreviewRow.Visible = true;

            this.PreviewMessagePost.MessageFlags = new MessageFlags
                                                       {
                                                           IsHtml = this.forumEditor.UsesHTML,
                                                           IsBBCode = this.forumEditor.UsesBBCode
                                                       };

            this.PreviewMessagePost.Message = this.forumEditor.Text;
        }

        /// <summary>
        /// The can edit post check.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="topicInfo">
        /// The topic Info.
        /// </param>
        /// <returns>
        /// Returns if user can edit post check.
        /// </returns>
        private bool CanEditPostCheck([NotNull] Message message, Topic topicInfo)
        {
            var postLocked = false;

            if (!this.PageContext.IsAdmin && this.PageContext.BoardSettings.LockPosts > 0)
            {
                var edited = message.Edited.Value;

                if (edited.AddDays(this.PageContext.BoardSettings.LockPosts) < DateTime.UtcNow)
                {
                    postLocked = true;
                }
            }

            // get  forum information
            var forumInfo = this.GetRepository<Forum>().GetById(this.PageContext.PageForumID);

            // Ederon : 9/9/2007 - moderator can edit in locked topics
            return !postLocked && !forumInfo.ForumFlags.IsLocked
                               && !topicInfo.TopicFlags.IsLocked
                               && message.UserID == this.PageContext.PageUserID
                    || this.PageContext.ForumModeratorAccess && this.PageContext.ForumEditAccess;
        }

        /// <summary>
        /// Determines whether this instance [can quote post check] the specified topic info.
        /// </summary>
        /// <param name="topicInfo">
        /// The topic info.
        /// </param>
        /// <returns>
        /// The can quote post check.
        /// </returns>
        private bool CanQuotePostCheck(Topic topicInfo)
        {
            // get topic and forum information
            var forumInfo = this.GetRepository<Forum>()
                .List(this.PageContext.PageBoardID, this.PageContext.PageForumID).FirstOrDefault();

            if (topicInfo == null || forumInfo == null)
            {
                return false;
            }

            // Ederon : 9/9/2007 - moderator can reply to locked topics
            return !forumInfo.ForumFlags.IsLocked
                    && !topicInfo.TopicFlags.IsLocked || this.PageContext.ForumModeratorAccess
                   && this.PageContext.ForumReplyAccess;
        }

        /// <summary>
        /// The initializes the edited post.
        /// </summary>
        /// <param name="currentMessage">
        /// The current message.
        /// </param>
        private void InitEditedPost([NotNull] Message currentMessage)
        {
            /*if (this.forumEditor.UsesHTML && currentMessage.Flags.IsBBCode)
            {
                // If the message is in YafBBCode but the editor uses HTML, convert the message text to HTML
                currentMessage.Message = this.Get<IBBCode>().ConvertBBCodeToHtmlForEdit(currentMessage.Message);
            }*/

            if (this.forumEditor.UsesBBCode && currentMessage.MessageFlags.IsHtml)
            {
                // If the message is in HTML but the editor uses YafBBCode, convert the message text to BBCode
                currentMessage.MessageText = this.Get<IBBCode>().ConvertHtmlToBBCodeForEdit(currentMessage.MessageText);
            }

            this.forumEditor.Text = currentMessage.MessageText;

            if (this.forumEditor.UsesHTML && currentMessage.MessageFlags.IsBBCode)
            {
                this.forumEditor.Text = this.Get<IBBCode>().FormatMessageWithCustomBBCode(
                    this.forumEditor.Text,
                    currentMessage.MessageFlags,
                    currentMessage.UserID,
                    currentMessage.ID);
            }

            this.Title.Text = this.GetText("EDIT");
            this.PostReply.TextLocalizedTag = "SAVE";
            this.PostReply.TextLocalizedPage = "COMMON";

            // add topic link...
            this.PageLinks.AddTopic(this.Server.HtmlDecode(this.PageContext.PageTopicName), this.PageContext.PageTopicID);

            // editing..
            this.PageLinks.AddLink(this.GetText("EDIT"));

            this.TopicSubjectTextBox.Text = this.Server.HtmlDecode(this.PageContext.PageTopicName);
            this.TopicDescriptionTextBox.Text = this.Server.HtmlDecode(this.topic.Description);

            if (this.topic.UserID == currentMessage.UserID.ToType<int>()
                || this.PageContext.ForumModeratorAccess)
            {
                // allow editing of the topic subject
                this.TopicSubjectTextBox.Enabled = true;
            }
            else
            {
                this.TopicSubjectTextBox.Enabled = false;
                this.TopicDescriptionTextBox.Enabled = false;
            }

            // Allow the Styling of Topic Titles only for Mods or Admins
            if (this.PageContext.BoardSettings.UseStyledTopicTitles
                && (this.PageContext.ForumModeratorAccess || this.PageContext.IsAdmin))
            {
                this.StyleRow.Visible = true;
            }
            else
            {
                this.StyleRow.Visible = false;
                this.TopicStylesTextBox.Enabled = false;
            }

            this.TopicStylesTextBox.Text = this.topic.Styles;

            this.Priority.SelectedItem.Selected = false;
            this.Priority.Items.FindByValue(this.topic.Priority.ToString()).Selected = true;

            this.EditReasonRow.Visible = true;
            this.ReasonEditor.Text = this.Server.HtmlDecode(currentMessage.EditReason);
            this.PostOptions1.PersistentChecked = currentMessage.MessageFlags.IsPersistent;

            var topicsList = this.GetRepository<TopicTag>().List(this.PageContext.PageTopicID);

            if (topicsList.Any())
            {
                this.Tags.Text = topicsList.Select(t => t.Item2.TagName).ToDelimitedString(",");
            }
        }

        /// <summary>
        /// Initializes the quoted reply.
        /// </summary>
        /// <param name="message">
        /// The current TypedMessage.
        /// </param>
        private void InitQuotedReply(Message message)
        {
            var messageContent = message.MessageText;

            if (this.PageContext.BoardSettings.RemoveNestedQuotes)
            {
                messageContent = this.Get<IFormatMessage>().RemoveNestedQuotes(messageContent);
            }

            /*if (this.forumEditor.UsesHTML && message.Flags.IsBBCode)
            {
                // If the message is in YafBBCode but the editor uses HTML, convert the message text to HTML
                messageContent = this.Get<IBBCode>().ConvertBBCodeToHtmlForEdit(messageContent);
            }*/

            if (this.forumEditor.UsesBBCode && message.MessageFlags.IsHtml)
            {
                // If the message is in HTML but the editor uses YafBBCode, convert the message text to BBCode
                messageContent = this.Get<IBBCode>().ConvertHtmlToBBCodeForEdit(messageContent);
            }

            // Ensure quoted replies have bad words removed from them
            messageContent = this.Get<IBadWordReplace>().Replace(messageContent);

            // Remove HIDDEN Text
            messageContent = this.Get<IFormatMessage>().RemoveHiddenBBCodeContent(messageContent);

            // Quote the original message
            this.forumEditor.Text +=
                $"[quote={this.Get<IUserDisplayName>().GetName(message.UserID.ToType<int>())};{message.ID}]{messageContent}[/quote]\r\n"
                    .TrimStart();

            /*if (this.forumEditor.UsesHTML && message.Flags.IsBBCode)
            {
                // If the message is in YafBBCode but the editor uses HTML, convert the message text to HTML
                this.forumEditor.Text = this.Get<IBBCode>().ConvertBBCodeToHtmlForEdit(this.forumEditor.Text);

                this.forumEditor.Text = this.Get<IBBCode>().FormatMessageWithCustomBBCode(
                    this.forumEditor.Text,
                    message.Flags,
                    message.UserID,
                    message.MessageID);
            }*/
        }

        /// <summary>
        /// Initializes a reply to the current topic.
        /// </summary>
        private void InitReplyToTopic()
        {
            // Ederon : 9/9/2007 - moderators can reply in locked topics
            if (this.topic.TopicFlags.IsLocked && !this.PageContext.ForumModeratorAccess)
            {
                var urlReferrer = this.Get<HttpRequestBase>().UrlReferrer;

                if (urlReferrer != null)
                {
                    this.Get<HttpResponseBase>().Redirect(urlReferrer.ToString());
                }
            }

            this.PriorityRow.Visible = false;
            this.SubjectRow.Visible = false;
            this.DescriptionRow.Visible = false;
            this.StyleRow.Visible = false;
            this.TagsHolder.Visible = false;

            this.Title.Text = this.GetText("reply");

            // add topic link...
            this.PageLinks.AddTopic(this.topic.TopicName, this.TopicId.ToType<int>());

            // add "reply" text...
            this.PageLinks.AddLink(this.GetText("reply"));

            this.HandleUploadControls();

            // show the last posts AJAX frame...
            this.LastPosts1.Visible = true;
            this.LastPosts1.TopicID = this.TopicId.Value.ToType<int>();
        }

        /// <summary>
        /// Updates Watch Topic based on controls/settings for user...
        /// </summary>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <param name="topicId">
        /// The topic Id.
        /// </param>
        private void UpdateWatchTopic(int userId, int topicId)
        {
            var topicWatchedID = this.GetRepository<WatchTopic>().Check(userId, topicId);

            if (topicWatchedID.HasValue && !this.PostOptions1.WatchChecked)
            {
                // unsubscribe...
                this.GetRepository<WatchTopic>().DeleteById(topicWatchedID.Value);
            }
            else if (!topicWatchedID.HasValue && this.PostOptions1.WatchChecked)
            {
                // subscribe to this topic...
                this.GetRepository<WatchTopic>().Add(userId, topicId);
            }
        }

        /// <summary>
        /// Checks the forum moderate status.
        /// </summary>
        /// <param name="forumInfo">The forum information.</param>
        /// <param name="isNewTopic">if set to <c>true</c> [is new topic].</param>
        /// <returns>
        /// Returns if the forum needs to be moderated
        /// </returns>
        private bool CheckForumModerateStatus(Forum forumInfo, bool isNewTopic)
        {
            // User Moderate override
            if (this.PageContext.Moderated)
            {
                return true;
            }

            var forumModerated = forumInfo.ForumFlags.IsModerated;

            if (!forumModerated)
            {
                return false;
            }

            if (forumInfo.IsModeratedNewTopicOnly && !isNewTopic)
            {
                return false;
            }

            if (!forumInfo.ModeratedPostCount.HasValue || this.PageContext.IsGuest)
            {
                return true;
            }

            var moderatedPostCount = forumInfo.ModeratedPostCount;

            return !(this.PageContext.CurrentUser.NumPosts >= moderatedPostCount);
        }

        /// <summary>
        /// Handles the upload controls.
        /// </summary>
        private void HandleUploadControls()
        {
            this.forumEditor.UserCanUpload = this.PageContext.ForumUploadAccess;
            this.UploadDialog.Visible = this.PageContext.ForumUploadAccess;
        }

#endregion
    }
}