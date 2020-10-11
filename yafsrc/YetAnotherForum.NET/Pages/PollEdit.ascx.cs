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
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.UI.WebControls;

    using YAF.Core.BasePages;
    using YAF.Core.Extensions;
    using YAF.Core.Model;
    using YAF.Types.Constants;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Types.Models;
    using YAF.Utils;
    using YAF.Utils.Helpers;
    using YAF.Web.Extensions;

    #endregion

    /// <summary>
    /// The Poll Edit Page.
    /// </summary>
    public partial class PollEdit : ForumPage
    {
        #region Constants and Fields

        /// <summary>
        ///   Table with choices
        /// </summary>
        private Topic topicInfo;

        /// <summary>
        /// The topic unapproved.
        /// </summary>
        private bool topicUnapproved;

        /// <summary>
        /// The forum id.
        /// </summary>
        private int forumId;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PollEdit"/> class. 
        ///   Initializes a new instance of the ReportPost class.
        /// </summary>
        public PollEdit()
            : base("POLLEDIT")
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets PollID.
        /// </summary>
        protected int? PollId { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// The cancel_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        protected void Cancel_Click(object sender, EventArgs eventArgs)
        {
            this.ReturnToPage();
        }

        /// <summary>
        /// The is input verified.
        /// </summary>
        /// <returns>
        /// Return if input is verified.
        /// </returns>
        protected bool IsInputVerified()
        {
            if (this.Question.Text.Trim().Length == 0)
            {
                this.PageContext.AddLoadMessage(this.GetText("POLLEDIT", "NEED_QUESTION"), MessageTypes.warning);
                return false;
            }

            this.Question.Text = HtmlHelper.StripHtml(this.Question.Text);

            var count =
                (from RepeaterItem ri in this.ChoiceRepeater.Items
                    select ri.FindControlAs<TextBox>("PollChoice").Text.Trim()).Count(value => value.IsSet());

            if (count < 2)
            {
                this.PageContext.AddLoadMessage(this.GetText("POLLEDIT", "NEED_CHOICES"), MessageTypes.warning);
                return false;
            }

            // Set default value
            if (this.PollExpire.Text.IsNotSet() && this.IsClosedBoundCheckBox.Checked)
            {
                this.PollExpire.Text = "1";
            }

            return true;
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
        protected void Page_Load(object sender, EventArgs e)
        {
            this.InitializeVariables();

            this.PollObjectRow1.Visible =
                (this.PageContext.IsAdmin || this.PageContext.BoardSettings.AllowUsersImagedPoll) &&
                this.PageContext.ForumPollAccess;
        }

        /// <summary>
        /// The save poll_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="eventArgs">
        /// The event args.
        /// </param>
        protected void SavePoll_Click(object sender, EventArgs eventArgs)
        {
            if (!this.PageContext.ForumPollAccess || !this.IsInputVerified())
            {
                return;
            }

            if (this.CreateOrUpdatePoll())
            {
                this.ReturnToPage();
            }
        }

        /// <summary>
        /// Adds page links to the page
        /// </summary>
        protected override void CreatePageLinks()
        {
            this.PageLinks.AddRoot();
        }

        /// <summary>
        /// Checks access rights for the page
        /// </summary>
        private void CheckAccess()
        {
            if (this.forumId > 0 && !this.PageContext.ForumPollAccess)
            {
                BuildLink.RedirectInfoPage(InfoMessage.AccessDenied);
            }
        }

        /// <summary>
        /// The save poll.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private bool CreateOrUpdatePoll()
        {
            DateTime? datePollExpire = null;

            if (this.PollExpire.Text.IsSet())
            {
                datePollExpire = DateTime.UtcNow.AddDays(this.PollExpire.Text.ToType<int>());
            }

            // we are just using existing poll
            if (this.PollId != null)
            {
                this.GetRepository<Poll>().Update(
                    this.PollId.Value,
                    this.Question.Text,
                    datePollExpire,
                    this.IsClosedBoundCheckBox.Checked,
                    this.AllowMultipleChoicesCheckBox.Checked,
                    this.ShowVotersCheckBox.Checked,
                    this.QuestionObjectPath.Text);

                this.ChoiceRepeater.Items.Cast<RepeaterItem>().ForEach(
                    item =>
                    {
                        var choiceId = item.FindControlAs<HiddenField>("PollChoiceID").Value;

                        var choiceName = item.FindControlAs<TextBox>("PollChoice").Text.Trim();
                        var choiceObjectPath = item.FindControlAs<TextBox>("ObjectPath").Text.Trim();

                        if (choiceId.IsNotSet() && choiceName.IsSet())
                        {
                            // add choice
                            this.GetRepository<Choice>().AddChoice(this.PollId.Value, choiceName, choiceObjectPath);
                        }
                        else if (choiceId.IsSet() && choiceName.IsSet())
                        {
                            // update choice
                            this.GetRepository<Choice>().UpdateChoice(choiceId.ToType<int>(), choiceName, choiceObjectPath);
                        }
                        else if (choiceId.IsSet() && choiceName.IsNotSet())
                        {
                            // remove choice
                            this.GetRepository<Choice>().DeleteById(choiceId.ToType<int>());
                        }
                    });

                return true;
            }

            // Create New Poll
            var newPollId = this.GetRepository<Poll>().Create(
                this.PageContext.PageUserID,
                this.Question.Text,
                datePollExpire,
                this.IsClosedBoundCheckBox.Checked,
                this.AllowMultipleChoicesCheckBox.Checked,
                this.ShowVotersCheckBox.Checked,
                this.QuestionObjectPath.Text);

            this.ChoiceRepeater.Items.Cast<RepeaterItem>().ForEach(
                item =>
                {
                    var choiceName = item.FindControlAs<TextBox>("PollChoice").Text.Trim();
                    var choiceObjectPath = item.FindControlAs<TextBox>("ObjectPath").Text.Trim();

                    if (choiceName.IsSet())
                    {
                        // add choice
                        this.GetRepository<Choice>().AddChoice(newPollId, choiceName, choiceObjectPath);
                    }
                });

            // Attach Poll to topic
            this.GetRepository<Topic>().AttachPoll(this.topicInfo.ID, newPollId);

            return true;
        }

        /// <summary>
        /// Initializes Poll UI
        /// </summary>
        /// <param name="pollId">
        /// The poll Id.
        /// </param>
        private void InitPollUI(int? pollId)
        {
            this.AllowMultipleChoicesCheckBox.Text = this.GetText("POLL_MULTIPLECHOICES");
            this.ShowVotersCheckBox.Text = this.GetText("POLL_SHOWVOTERS");
            this.IsClosedBoundCheckBox.Text = this.GetText("pollgroup_closedbound");

            List<Choice> choices;

            if (pollId.HasValue)
            {
                // we edit existing poll 
                var pollAndChoices = this.GetRepository<Poll>().GetPollAndChoices(this.PollId.Value);

                var poll = pollAndChoices.FirstOrDefault().Item1;

                if (poll.UserID != this.PageContext.PageUserID &&
                    !this.PageContext.IsAdmin && !this.PageContext.ForumModeratorAccess)
                {
                    BuildLink.RedirectInfoPage(InfoMessage.Invalid);
                }

                this.IsClosedBoundCheckBox.Checked = poll.PollFlags.IsClosedBound;
                this.AllowMultipleChoicesCheckBox.Checked = poll.PollFlags.AllowMultipleChoice;
                this.ShowVotersCheckBox.Checked = poll.PollFlags.ShowVoters;
                this.Question.Text = poll.Question;
                this.QuestionObjectPath.Text = poll.ObjectPath;

                if (poll.Closes.HasValue)
                {
                    var closing = poll.Closes.Value - DateTime.UtcNow;

                    this.PollExpire.Text = (closing.TotalDays + 1).ToType<int>().ToString();
                }
                else
                {
                    this.PollExpire.Text = string.Empty;
                }

                choices = pollAndChoices.Select(c => c.Item2).ToList();

                var count = this.PageContext.BoardSettings.AllowedPollChoiceNumber - 1 - choices.Count;

                if (count > 0)
                {
                    for (var i = 0; i <= count; i++)
                    {
                        var choice = new Choice { ID = i };

                        choices.Add(choice);
                    }
                }
            }
            else
            {
                // A new poll is created
                if (!this.CanCreatePoll())
                {
                    BuildLink.RedirectInfoPage(InfoMessage.AccessDenied);
                }

                // clear the fields...
                this.PollExpire.Text = string.Empty;
                this.Question.Text = string.Empty;

                choices = new List<Choice>();

                // we add dummy rows to data table to fill in repeater empty fields   
                var dummyRowsCount = this.PageContext.BoardSettings.AllowedPollChoiceNumber - 1;
                for (var i = 0; i <= dummyRowsCount; i++)
                {
                    var choice = new Choice { ID = i };

                    choices.Add(choice);
                }
            }

            // Bind choices repeater
            this.ChoiceRepeater.DataSource = choices;
            this.ChoiceRepeater.DataBind();

            // Show controls
            this.SavePoll.Visible = true;
            this.Cancel.Visible = true;
            this.PollRowExpire.Visible = true;
            this.IsClosedBound.Visible =
                this.PageContext.BoardSettings.AllowUsersHidePollResults || this.PageContext.IsAdmin ||
                this.PageContext.IsForumModerator;
            this.tr_AllowMultipleChoices.Visible = this.PageContext.BoardSettings.AllowMultipleChoices ||
                                                   this.PageContext.IsAdmin || this.PageContext.ForumModeratorAccess;
        }

        /// <summary>
        /// Initializes page context query variables.
        /// </summary>
        private void InitializeVariables()
        {
            // we return to a forum (used when a topic should be approved)
            if (this.Get<HttpRequestBase>().QueryString.Exists("f"))
            {
                this.topicUnapproved = true;

                this.forumId =
                    Security.StringToIntOrRedirect(this.Get<HttpRequestBase>().QueryString.GetFirstOrDefault("f"));
            }

            if (this.Get<HttpRequestBase>().QueryString.Exists("t"))
            {
                var topicId =
                    Security.StringToIntOrRedirect(this.Get<HttpRequestBase>().QueryString.GetFirstOrDefault("t"));
                this.topicInfo = this.GetRepository<Topic>().GetById(topicId);

                if (this.forumId > 0)
                {
                    this.PageLinks.AddForum(this.forumId);
                }

                if (this.topicInfo != null)
                {
                    this.PageLinks.AddTopic(this.topicInfo.TopicName, this.topicInfo.ID);
                }
            }

            // Check if the user has the page access and variables are correct. 
            this.CheckAccess();

            // handle poll
            if (this.Get<HttpRequestBase>().QueryString.Exists("p"))
            {
                // edit existing poll
                this.PollId =
                    Security.StringToIntOrRedirect(this.Get<HttpRequestBase>().QueryString.GetFirstOrDefault("p"));
                this.InitPollUI(this.PollId);

                this.PageLinks.AddLink(this.GetText("POLLEDIT", "EDITPOLL"), string.Empty);

                this.Header.LocalizedTag = "EDITPOLL";
            }
            else
            {
                // new poll
                this.InitPollUI(null);

                this.PageLinks.AddLink(this.GetText("POLLEDIT", "CREATEPOLL"), string.Empty);

                this.Header.LocalizedTag = "CREATEPOLL";
            }
        }

        /// <summary>
        /// The return to page.
        /// </summary>
        private void ReturnToPage()
        {
            if (this.topicUnapproved)
            {
                // Tell user that his message will have to be approved by a moderator
                BuildLink.Redirect(ForumPages.Info, "i=1");
            }

            BuildLink.Redirect(ForumPages.Posts, "t={0}&name={1}", this.topicInfo.ID, this.topicInfo.TopicName);
        }

        /// <summary>
        /// Checks if a user can create poll.
        /// </summary>
        /// <returns>
        /// The can create poll.
        /// </returns>
        private bool CanCreatePoll()
        {
            if (this.topicInfo != null)
            {
                return true;
            }

            // admins can add any number of polls
            if (this.PageContext.IsAdmin || this.PageContext.ForumModeratorAccess)
            {
                return true;
            }

            if (!this.PageContext.ForumPollAccess)
            {
                return false;
            }

            if (this.PageContext.ForumPollAccess)
            {
                return true;
            }

            return this.PageContext.BoardSettings.AllowedPollChoiceNumber > 0;
        }

        #endregion
    }
}