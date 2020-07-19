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

    using YAF.Configuration;
    
    using YAF.Core.BasePages;
    using YAF.Core.Extensions;
    using YAF.Types;
    using YAF.Types.Constants;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Types.Models;
    using YAF.Utils;
    using YAF.Web.Extensions;

    #endregion

    /// <summary>
    /// the album page.
    /// </summary>
    public partial class Album : ForumPage
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the Album class.
        /// </summary>
        public Album()
            : base("ALBUM")
        {
        }

        #endregion

        /// <summary>
        ///   Gets user ID of edited user.
        /// </summary>
        protected int CurrentUserID =>
            Security.StringToIntOrRedirect(this.Get<HttpRequestBase>().QueryString.GetFirstOrDefault("u"));

        /// <summary>
        /// The album id.
        /// </summary>
        protected int AlbumID =>
            Security.StringToIntOrRedirect(this.Get<HttpRequestBase>().QueryString.GetFirstOrDefault("a"));

        #region Methods

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load([NotNull] object sender, [NotNull] EventArgs e)
        {
            if (!this.Get<BoardSettings>().EnableAlbum)
            {
                BuildLink.AccessDenied();
            }

            if (!this.Get<HttpRequestBase>().QueryString.Exists("u")
                || !this.Get<HttpRequestBase>().QueryString.Exists("a"))
            {
                BuildLink.AccessDenied();
            }

            var displayName = this.Get<IUserDisplayName>().GetName(this.CurrentUserID);

            var album = this.GetRepository<UserAlbum>().GetById(this.AlbumID);

            // Generate the page links.
            this.PageLinks.Clear();
            this.PageLinks.AddRoot();
            this.PageLinks.AddUser(this.CurrentUserID, displayName);
            this.PageLinks.AddLink(this.GetText("ALBUMS"), BuildLink.GetLink(ForumPages.Albums, "u={0}", this.CurrentUserID));
            this.PageLinks.AddLink(this.GetText("TITLE"), string.Empty);

            // Set the title text.
            this.LocalizedLabel1.Param0 = this.Server.HtmlEncode(displayName);
            this.LocalizedLabel1.Param1 =
                this.Server.HtmlEncode(album.Title);

            // Initialize the Album Image List control.
            this.AlbumImageList1.UserID = this.CurrentUserID;
            this.AlbumImageList1.UserAlbum = album;

            this.EditAlbums.Visible = this.PageContext.PageUserID == this.CurrentUserID;
        }

        /// <summary>
        /// Create the Page links.
        /// </summary>
        protected override void CreatePageLinks()
        {
        }

        /// <summary>
        /// Go Back to Albums Page
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Back_Click(object sender, EventArgs e)
        {
            BuildLink.Redirect(
                ForumPages.Albums,
                "u={0}",
                this.Get<HttpRequestBase>().QueryString.GetFirstOrDefault("u"));
        }

        /// <summary>
        /// Redirect to the edit album page.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void EditAlbums_Click([NotNull] object sender, [NotNull] EventArgs e)
        {
            BuildLink.Redirect(
                ForumPages.EditAlbumImages,
                "a={0}",
                this.Get<HttpRequestBase>().QueryString.GetFirstOrDefault("a"));
        }

        #endregion
    }
}