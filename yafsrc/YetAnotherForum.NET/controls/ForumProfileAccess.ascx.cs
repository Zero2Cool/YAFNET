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
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Web;

    using YAF.Core.BaseControls;
    using YAF.Core.Extensions;
    using YAF.Core.Model;
    using YAF.Types;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Types.Models;
    using YAF.Utils;

    #endregion

    /// <summary>
    /// The forum profile access.
    /// </summary>
    public partial class ForumProfileAccess : BaseUserControl
    {
        #region Methods

        /// <summary>
        /// The page_ load.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load([NotNull] object sender, [NotNull] EventArgs e)
        {
            if (!this.PageContext.IsAdmin && !this.PageContext.IsForumModerator)
            {
                return;
            }

            var userID = Security.StringToIntOrRedirect(this.Get<HttpRequestBase>().QueryString.GetFirstOrDefault("u"));

            using (var dt2 = this.GetRepository<ForumAccess>()
                .UserAccessMasksAsDataTable(this.PageContext.PageBoardID, userID))
            {
                var html = new StringBuilder();
                var lastForumId = 0;

                dt2.Rows.Cast<DataRow>().ForEach(
                    row =>
                        {
                            if (lastForumId != row["ForumID"].ToType<int>())
                            {
                                if (lastForumId != 0)
                                {
                                    html.AppendFormat("</li>");
                                }

                                html.AppendFormat(
                                    "<li class=\"list-group-item\"><span class=\"font-weight-bold\">{0}:</span>&nbsp;",
                                    this.HtmlEncode(row["ForumName"]));
                                lastForumId = row["ForumID"].ToType<int>();
                            }

                            html.AppendFormat(
                                "<span class=\"badge badge-pill badge-info mr-1\">{0}</span>",
                                row["AccessMaskName"]);
                        });

                if (lastForumId != 0)
                {
                    html.AppendFormat("</li>");
                }

                this.AccessMaskRow.Text = html.ToString();
            }
        }

        #endregion
    }
}