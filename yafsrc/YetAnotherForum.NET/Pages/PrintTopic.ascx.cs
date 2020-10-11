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
    using System.Web;

    using YAF.Core.BasePages;
    using YAF.Core.Extensions;
    using YAF.Core.Model;
    using YAF.Types;
    using YAF.Types.Extensions;
    using YAF.Types.Flags;
    using YAF.Types.Interfaces;
    using YAF.Types.Models;
    using YAF.Types.Objects.Model;
    using YAF.Utils;
    using YAF.Utils.Helpers;
    using YAF.Web.Extensions;

    #endregion

    /// <summary>
    /// Print topic Page.
    /// </summary>
    public partial class PrintTopic : ForumPage
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "PrintTopic" /> class.
        /// </summary>
        public PrintTopic()
            : base("PRINTTOPIC")
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the print body.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns>
        /// The get print body.
        /// </returns>
        protected string GetPrintBody([NotNull] object o)
        {
            var row = (PagedMessage)o;

            var message = row.Message;

            message = this.Get<IFormatMessage>().Format(row.MessageID, message, new MessageFlags(row.Flags));

            // Remove HIDDEN Text
            message = this.Get<IFormatMessage>().RemoveHiddenBBCodeContent(message);

            message = this.Get<IFormatMessage>().RemoveCustomBBCodes(message);

            return message;
        }

        /// <summary>
        /// Gets the print header.
        /// </summary>
        /// <param name="o">The o.</param>
        /// <returns>
        /// The get print header.
        /// </returns>
        protected string GetPrintHeader([NotNull] object o)
        {
            var row = (PagedMessage)o;
            return
                $"<strong>{this.GetText("postedby")}: {(this.PageContext.BoardSettings.EnableDisplayName ? row.DisplayName : row.UserName)}</strong> - {this.Get<IDateTime>().FormatDateTime(row.Posted)}";
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
            if (!this.Get<HttpRequestBase>().QueryString.Exists("t") || !this.PageContext.ForumReadAccess)
            {
                BuildLink.AccessDenied();
            }

            this.ShowToolBar = false;

            if (this.IsPostBack)
            {
                return;
            }

            var showDeleted = false;
            var userId = 0;
            if (this.PageContext.BoardSettings.ShowDeletedMessagesToAll)
            {
                showDeleted = true;
            }

            if (!showDeleted && (this.PageContext.BoardSettings.ShowDeletedMessages &&
                                 !this.PageContext.BoardSettings.ShowDeletedMessagesToAll
                                 || this.PageContext.IsAdmin ||
                                 this.PageContext.ForumModeratorAccess))
            {
                userId = this.PageContext.PageUserID;
            }

            var posts = this.GetRepository<Message>().PostListPaged(
                this.PageContext.PageTopicID,
                this.PageContext.PageUserID,
                userId,
                !this.PageContext.IsCrawler,
                showDeleted,
                DateTimeHelper.SqlDbMinTime(),
                DateTime.UtcNow,
                0,
                500,
                -1);

            this.Posts.DataSource = posts;

            this.DataBind();
        }

        /// <summary>
        /// Create the Page links.
        /// </summary>
        protected override void CreatePageLinks()
        {
            if (this.PageContext.Settings.LockedForum == 0)
            {
                this.PageLinks.AddRoot();
                this.PageLinks.AddCategory(this.PageContext.PageCategoryName, this.PageContext.PageCategoryID);
            }

            this.PageLinks.AddForum(this.PageContext.PageForumID);
            this.PageLinks.AddTopic(this.PageContext.PageTopicName, this.PageContext.PageTopicID);
        }

        #endregion
    }
}