/* Yet Another Forum.NET
 * Copyright (C) 2003-2005 Bjørnar Henden
 * Copyright (C) 2006-2013 Jaben Cargman
 * Copyright (C) 2014-2019 Ingo Herbote
 * https://www.yetanotherforum.net/
 * 
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at

 * http://www.apache.org/licenses/LICENSE-2.0

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
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Web.UI.WebControls;

    using YAF.Configuration;
    using YAF.Core.BaseControls;
    using YAF.Core.Extensions;
    using YAF.Core.Utilities;
    using YAF.Types;
    using YAF.Types.Constants;
    using YAF.Types.Extensions;
    using YAF.Types.Flags;
    using YAF.Types.Interfaces;
    using YAF.Utils;
    using YAF.Utils.Helpers;

    #endregion

    /// <summary>
    /// The Forum List Control
    /// </summary>
    public partial class ForumList : BaseUserControl
    {
        #region Constants and Fields

        /// <summary>
        ///   The Go to last post Image ToolTip.
        /// </summary>
        private string altLastPost;

        /// <summary>
        /// The Data Source
        /// </summary>
        private IEnumerable dataSource;

        #endregion

        #region Properties

        /// <summary>
        ///   Gets or sets Alt.
        /// </summary>
        [NotNull]
        public string AltLastPost
        {
            get => this.altLastPost.IsNotSet() ? string.Empty : this.altLastPost;

            set => this.altLastPost = value;
        }

        /// <summary>
        ///   Gets or sets DataSource.
        /// </summary>
        public IEnumerable DataSource
        {
            get => this.dataSource;

            set
            {
                this.dataSource = value;
                DataRow[] arr;
                var t = this.dataSource.GetType();
                var dataRows = new List<DataRow>();
                var parents = new List<int>();
                if (t.Name == "DataRowCollection")
                {
                    arr = new DataRow[((DataRowCollection)this.dataSource).Count];
                    ((DataRowCollection)this.dataSource).CopyTo(arr, 0);

                    arr.ForEach(
                        t1 =>
                            {
                                // these are all sub forums related to start page forums
                                if (!t1["ParentID"].IsNullOrEmptyDBField())
                                {
                                    if (this.SubDataSource == null)
                                    {
                                        this.SubDataSource = t1.Table.Clone();
                                    }

                                    var newRow = this.SubDataSource.NewRow();
                                    newRow.ItemArray = t1.ItemArray;

                                    parents.Add(newRow["ForumID"].ToType<int>());

                                    if (parents.Contains(newRow["ParentID"].ToType<int>()))
                                    {
                                        this.SubDataSource.Rows.Add(newRow);
                                    }
                                    else
                                    {
                                        dataRows.Add(t1);
                                    }
                                }
                                else
                                {
                                    dataRows.Add(t1);
                                }
                            });
                }
                else
                {
                    arr = (DataRow[])this.dataSource;

                    arr.ForEach(
                        t1 =>
                            {
                                if (!t1["ParentID"].IsNullOrEmptyDBField())
                                {
                                    if (this.SubDataSource == null)
                                    {
                                        this.SubDataSource = t1.Table.Clone();
                                    }

                                    var newRow = this.SubDataSource.NewRow();
                                    newRow.ItemArray = t1.ItemArray;

                                    this.SubDataSource.Rows.Add(newRow);
                                }
                                else
                                {
                                    dataRows.Add(t1);
                                }
                            });
                }

                this.SubDataSource?.AcceptChanges();

                this.dataSource = dataRows;

                this.ForumList1.DataSource = this.dataSource;
            }
        }

        /// <summary>
        /// Gets or sets the sub data source.
        /// </summary>
        /// <value>
        /// The sub data source.
        /// </value>
        private DataTable SubDataSource { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Provides the "Forum Link Text" for the ForumList control.
        ///   Automatically disables the link if the current user doesn't
        ///   have proper permissions.
        /// </summary>
        /// <param name="row">
        /// Current data row
        /// </param>
        /// <returns>
        /// Forum link text
        /// </returns>
        public string GetForumLink([NotNull] DataRow row)
        {
            var forumID = row["ForumID"].ToType<int>();

            // get the Forum Description
            var output = row["Forum"].ToString();

            if (row["ReadAccess"].ToType<int>() > 0)
            {
                var title = row["Description"].ToString().IsSet()
                                ? row["Description"].ToString()
                                : this.GetText("COMMON", "VIEW_FORUM");

                output = row["RemoteURL"] != DBNull.Value
                             ? $"<a href=\"{row["RemoteURL"]}\" title=\"{this.GetText("COMMON", "VIEW_FORUM")}\" target=\"_blank\">{this.Page.HtmlEncode(output)}&nbsp;<i class=\"fas fa-external-link-alt fa-fw\"></i></a>"
                             : $"<a href=\"{YafBuildLink.GetLink(ForumPages.topics, "f={0}&name={1}", forumID, output)}\" data-toggle=\"tooltip\" title=\"{title}\">{this.Page.HtmlEncode(output)}</a>";
            }
            else
            {
                // no access to this forum
                output = $"{output} {this.GetText("NO_FORUM_ACCESS")}";
            }

            return output;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the ItemCreated event of the ForumList1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void ForumList1_ItemCreated([NotNull] object sender, [NotNull] RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType != ListItemType.Item && e.Item.ItemType != ListItemType.AlternatingItem)
            {
                return;
            }

            var row = (DataRow)e.Item.DataItem;
            var flags = new ForumFlags(row["Flags"]);

            var lastRead = this.Get<IReadTrackCurrentUser>().GetForumTopicRead(
                row["ForumID"].ToType<int>(),
                row["LastTopicID"].ToType<int>(),
                row["LastForumAccess"].ToType<DateTime?>() ?? DateTimeHelper.SqlDbMinTime(),
                row["LastTopicAccess"].ToType<DateTime?>() ?? DateTimeHelper.SqlDbMinTime());

            var lastPosted = row["LastPosted"].ToType<DateTime?>() ?? lastRead;

            if (row["ImageUrl"].ToString().IsNotSet())
            {
                var forumIcon = e.Item.FindControlAs<PlaceHolder>("ForumIcon");

                var icon = new Literal
                               {
                                   Text =
                                       @"<a tabindex=""0"" class=""forum-icon-legend-popvover"" role=""button"" data-toggle=""popover"">
                                                      <i class=""fas fa-comments fa-1x text-success""></i>
                                                  </a>"
                               };

                try
                {
                    if (flags.IsLocked)
                    {
                        icon.Text =
                            @"<a tabindex=""0"" class=""forum-icon-legend-popvover"" role=""button"" data-toggle=""popover"">
                                   <span class=""fa-stack"">
                                       <i class=""fas fa-comments fa-stack-2x text-secondary""></i>
                                       <i class=""fas fa-lock fa-stack-1x text-warning"" style=""position:absolute; bottom:0px !important;text-align:right;line-height: 1em;""></i>
                                   </span></a>";
                    }
                    else if (lastPosted > lastRead && row["ReadAccess"].ToType<int>() > 0)
                    {
                        icon.Text =
                            @"<a tabindex=""0"" class=""forum-icon-legend-popvover"" role=""button"" data-toggle=""popover"">
                                   <span class=""fa-stack""><i class=""fas fa-comments fa-2x text-success""></i></span>
                               </a>";
                    }
                    else
                    {
                        icon.Text =
                            @"<a tabindex=""0"" class=""forum-icon-legend-popvover"" role=""button"" data-toggle=""popover"">
                                  <span class=""fa-stack"">
                                      <i class=""fas fa-comments fa-2x text-secondary""></i>
                                  </span>
                              </a>";
                    }
                }
                finally
                {
                    forumIcon.Controls.Add(icon);
                }
            }
            else
            {
                var forumImage = e.Item.FindControlAs<Image>("ForumImage1");
                if (forumImage != null)
                {
                    forumImage.ImageUrl =
                        $"{YafForumInfo.ForumClientFileRoot}{YafBoardFolders.Current.Forums}/{row["ImageUrl"]}";

                    // Highlight custom icon images and add tool tips to them. 
                    try
                    {
                        if (flags.IsLocked)
                        {
                            forumImage.CssClass = "forum_customimage_locked";
                            forumImage.AlternateText = this.GetText("ICONLEGEND", "FORUM_LOCKED");
                            forumImage.ToolTip = this.GetText("ICONLEGEND", "FORUM_LOCKED");
                        }
                        else if (lastPosted > lastRead)
                        {
                            forumImage.CssClass = "forum_customimage_newposts";
                            forumImage.AlternateText = this.GetText("ICONLEGEND", "NEW_POSTS");
                            forumImage.ToolTip = this.GetText("ICONLEGEND", "NEW_POSTS");
                        }
                        else
                        {
                            forumImage.CssClass = "forum_customimage_nonewposts";
                            forumImage.AlternateText = this.GetText("ICONLEGEND", "NO_NEW_POSTS");
                            forumImage.ToolTip = this.GetText("ICONLEGEND", "NO_NEW_POSTS");
                        }

                        forumImage.Visible = true;
                    }
                    catch
                    {
                        forumImage.Visible = false;
                    }
                }
            }

            if (!this.Get<YafBoardSettings>().ShowModeratorList)
            {
                return;
            }

            if (row["RemoteURL"] != DBNull.Value)
            {
                return;
            }

            var modList = e.Item.FindControlAs<ForumModeratorList>("ForumModeratorListMob");

            var dra = row.GetChildRows("FK_Moderator_Forum");

            if (dra.GetLength(0) <= 0)
            {
                return;
            }

            modList.DataSource = dra;
            modList.Visible = true;
            modList.DataBind();
        }

        /// <summary>
        /// Gets the moderated.
        /// </summary>
        /// <param name="row">The Data row.</param>
        /// <returns>
        /// The get moderated.
        /// </returns>
        protected bool GetModerated([NotNull] object row)
        {
            return ((DataRow)row)["Flags"].BinaryAnd(ForumFlags.Flags.IsModerated);
        }

        // Suppress rendering of footer if there is one or more 

        /// <summary>
        /// Gets the moderators footer.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <returns>
        /// The get moderators footer.
        /// </returns>
        [NotNull]
        protected string GetModeratorsFooter([NotNull] Repeater sender)
        {
            if (sender.DataSource is DataRow[] rows && rows.Length < 1)
            {
                return "-";
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the sub Forums.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>Returns the Sub Forums</returns>
        protected IEnumerable GetSubForums([NotNull] DataRow row)
        {
            if (!this.HasSubForums(row))
            {
                return null;
            }

            var arrayList = new ArrayList();

            this.SubDataSource.Rows.Cast<DataRow>()
                .Where(dataRow => row["ForumID"].ToType<int>() == dataRow["ParentID"].ToType<int>())
                .Where(subRow => arrayList.Count < this.Get<YafBoardSettings>().SubForumsInForumList)
                .ForEach(value => arrayList.Add(value));

            this.SubDataSource.AcceptChanges();

            return arrayList;
        }

        /// <summary>
        /// Gets the viewing.
        /// </summary>
        /// <param name="row">
        /// The row.
        /// </param>
        /// <returns>
        /// The get viewing.
        /// </returns>
        protected string GetViewing([NotNull] DataRow row)
        {
            var viewing = row["Viewing"].ToType<int>();

            return viewing > 0
                       ? $"<i class=\"far fa-eye text-secondary\" title=\"{this.GetTextFormatted("VIEWING", viewing)}\"></i> {viewing}"
                       : string.Empty;
        }

        /// <summary>
        /// Determines whether the specified row has sub forums.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <returns>
        /// The has sub forums.
        /// </returns>
        protected bool HasSubForums([NotNull] DataRow row)
        {
            return this.SubDataSource != null && this.SubDataSource.Rows.Cast<DataRow>().Any(
                       dataRow => row["ForumID"].ToType<int>() == dataRow["ParentID"].ToType<int>());
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Page_Load([NotNull] object sender, [NotNull] EventArgs e)
        {
            this.AltLastPost = this.GetText("DEFAULT", "GO_LAST_POST");
        }

        /// <summary>
        /// The On PreRender event.
        /// </summary>
        /// <param name="e">
        /// the Event Arguments
        /// </param>
        protected override void OnPreRender([NotNull] EventArgs e)
        {
            var iconLegend = this.LoadControl($"{YafForumInfo.ForumServerFileRoot}controls/ForumIconLegend.ascx")
                .RenderToString();

            // setup jQuery and DatePicker JS...
            this.PageContext.PageElements.RegisterJsBlockStartup(
                "ForumIconLegendPopoverJs",
                JavaScriptBlocks.ForumIconLegendPopoverJs(
                    iconLegend.Replace("\n", string.Empty).Replace("\r", string.Empty)));

            base.OnPreRender(e);
        }

        /// <summary>
        /// Gets the Posts string
        /// </summary>
        /// <param name="row">
        /// The row.
        /// </param>
        /// <returns>
        /// Returns the Posts string
        /// </returns>
        protected string Posts([NotNull] DataRow row)
        {
            return row["RemoteURL"] == DBNull.Value ? $"{row["Posts"]:N0}" : "-";
        }

        /// <summary>
        /// Gets the Topics string
        /// </summary>
        /// <param name="row">
        /// The row.
        /// </param>
        /// <returns>
        /// Returns the Topics string
        /// </returns>
        protected string Topics([NotNull] DataRow row)
        {
            return row["RemoteURL"] == DBNull.Value ? $"{row["Topics"]:N0}" : "-";
        }

        #endregion
    }
}