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
    using System.Web.UI.WebControls;

    using YAF.Core.BaseControls;
    using YAF.Core.Extensions;
    using YAF.Core.Helpers;
    using YAF.Core.Model;
    using YAF.Core.Utilities;
    using YAF.Types;
    using YAF.Types.Constants;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Types.Models;
    using YAF.Utils;
    using YAF.Utils.Helpers;
    using YAF.Web.Controls;

    #endregion

    /// <summary>
    /// The profile Timeline
    /// </summary>
    public partial class ProfileTimeline : BaseUserControl
    {
        #region Methods

        /// <summary>
        /// Gets or sets the item count.
        /// </summary>
        protected int ItemCount { get; set; }

        /// <summary>
        /// Registers the needed Java Scripts
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> object that contains the event data.</param>
        protected override void OnPreRender([NotNull] EventArgs e)
        {
            this.PageContext.PageElements.RegisterJsBlock("dropDownToggleJs", JavaScriptBlocks.DropDownToggleJs());

            base.OnPreRender(e);
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
            this.CreatedTopic.Text = this.GetText("CREATED_TOPIC");
            this.CreatedReply.Text = this.GetText("CREATED_REPLY");
            this.GivenThanks.Text = this.GetText("GIVEN_THANKS");

            if (this.IsPostBack)
            {
                return;
            }

            this.PageSize.DataSource = StaticDataHelper.PageEntries();
            this.PageSize.DataTextField = "Name";
            this.PageSize.DataValueField = "Value";
            this.PageSize.DataBind();

            var previousPageSize = this.Get<ISession>().UserActivityPageSize;

            if (previousPageSize.HasValue)
            {
                // look for value previously selected
                var sinceItem = this.PageSize.Items.FindByValue(previousPageSize.Value.ToString());

                // and select it if found
                if (sinceItem != null)
                {
                    this.PageSize.SelectedIndex = this.PageSize.Items.IndexOf(sinceItem);
                }
            }

            this.BindData();
        }

        /// <summary>
        /// The get last item class.
        /// </summary>
        /// <param name="itemIndex">
        /// The item index.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected string GetLastItemClass(int itemIndex)
        {
            return itemIndex == this.ItemCount - 1 ? string.Empty : "border-right";
        }

        /// <summary>
        /// The activity stream_ on item data bound.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void ActivityStream_OnItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            var activity = (Tuple<Activity, Topic>)e.Item.DataItem;

            var card = e.Item.FindControlAs<Panel>("Card");
            var iconLabel = e.Item.FindControlAs<Label>("Icon");
            var title = e.Item.FindControlAs<Literal>("Title");
            var messageHolder = e.Item.FindControlAs<PlaceHolder>("Message");
            var displayDateTime = e.Item.FindControlAs<DisplayDateTime>("DisplayDateTime");
            var markRead = e.Item.FindControlAs<ThemeButton>("MarkRead");

            var message = string.Empty;
            var icon = string.Empty;

            var topicLink = new ThemeButton
            {
                NavigateUrl = BuildLink.GetTopicLink(activity.Item2.ID, activity.Item2.TopicName),
                Type = ButtonStyle.None,
                Text = activity.Item2.TopicName,
                Icon = "comment",
                IconCssClass = "far"
            };

            if (activity.Item1.ActivityFlags.CreatedTopic)
            {
                topicLink.NavigateUrl = BuildLink.GetTopicLink(activity.Item1.TopicID.Value, activity.Item2.TopicName);
                title.Text = this.GetText("ACCOUNT", "CREATED_TOPIC");
                icon = "comment";
                message = this.GetTextFormatted("CREATED_TOPIC_MSG", topicLink.RenderToString());
            }

            if (activity.Item1.ActivityFlags.CreatedReply)
            {
                title.Text = this.GetText("ACCOUNT", "CREATED_REPLY");
                icon = "comment";
                message = this.GetTextFormatted("CREATED_REPLY_MSG", topicLink.RenderToString());
            }

            if (activity.Item1.ActivityFlags.GivenThanks)
            {
                var user = this.GetRepository<User>().GetById(activity.Item1.FromUserID.Value);

                var userLink = new UserLink
                {
                    UserID = activity.Item1.FromUserID.Value,
                    Suspended = user.Suspended,
                    Style = user.UserStyle,
                    ReplaceName = user.DisplayOrUserName()
                };

                title.Text = this.GetText("ACCOUNT", "GIVEN_THANKS");
                icon = "heart";
                message = this.GetTextFormatted(
                    "GIVEN_THANKS_MSG",
                    userLink.RenderToString(),
                    topicLink.RenderToString());
            }

            var notify = activity.Item1.Notification ? "text-success" : "text-secondary";

            card.CssClass = activity.Item1.Notification ? "card shadow" : "card";

            iconLabel.Text = $@"<i class=""fas fa-circle fa-stack-2x {notify}""></i>
               <i class=""fas fa-{icon} fa-stack-1x fa-inverse""></i>";

            displayDateTime.DateTime = activity.Item1.Created;

            messageHolder.Controls.Add(new Literal { Text = message });

            if (!activity.Item1.Notification)
            {
                return;
            }

            markRead.CommandArgument = activity.Item1.MessageID.Value.ToString();
            markRead.Visible = true;
        }

        /// <summary>
        /// The pager top_ page change.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void PagerTop_PageChange([NotNull] object sender, [NotNull] EventArgs e)
        {
            // rebind
            this.BindData();
        }

        /// <summary>
        /// The activity stream_ on item command.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void ActivityStream_OnItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName != "read")
            {
                return;
            }

            this.GetRepository<Activity>().UpdateNotification(
                this.PageContext.PageUserID,
                e.CommandArgument.ToType<int>());

            this.BindData();
        }

        /// <summary>
        /// The update filter click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void UpdateFilterClick(object sender, EventArgs e)
        {
            this.BindData();
        }

        /// <summary>
        /// Reset Filter
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void ResetClick(object sender, EventArgs e)
        {
            this.CreatedTopic.Checked = true;
            this.CreatedReply.Checked = true;
            this.GivenThanks.Checked = true;

            this.BindData();
        }

        /// <summary>
        /// The page size selected index changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void PageSizeSelectedIndexChanged(object sender, EventArgs e)
        {
            this.Get<ISession>().UserActivityPageSize = this.PageSize.SelectedValue.ToType<int>();

            this.BindData();
        }

        /// <summary>
        /// The bind data.
        /// </summary>
        private void BindData()
        {
            this.PagerTop.PageSize = this.PageSize.SelectedValue.ToType<int>();

            var stream = this.GetRepository<Activity>().Timeline(this.PageContext.PageUserID);

            if (!this.CreatedTopic.Checked)
            {
                stream.RemoveAll(a => a.Item1.CreatedTopic);
            }

            if (!this.CreatedReply.Checked)
            {
                stream.RemoveAll(a => a.Item1.CreatedReply);
            }

            if (!this.GivenThanks.Checked)
            {
                stream.RemoveAll(a => a.Item1.GivenThanks);
            }

            var paged = stream.Skip(this.PagerTop.CurrentPageIndex * this.PagerTop.PageSize)
                .Take(this.PagerTop.PageSize).ToList();

            this.ActivityStream.DataSource = paged;

            this.ItemCount = paged.Any() ? paged.Count : 0;

            this.DataBind();
        }

        #endregion
    }
}