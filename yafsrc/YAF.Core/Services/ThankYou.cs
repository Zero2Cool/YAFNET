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

namespace YAF.Core.Services
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;
    using System.Web;

    using YAF.Configuration;
    using YAF.Core.Context;
    using YAF.Core.Extensions;
    using YAF.Core.Model;
    using YAF.Types;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Types.Models;
    using YAF.Types.Objects;
    using YAF.Utils;

    /// <summary>
    ///  ThankYou Class to handle Thanks
    /// </summary>
    public class ThankYou : IThankYou, IHaveServiceLocator
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ThankYou"/> class.
        /// </summary>
        /// <param name="serviceLocator">
        /// The service locator.
        /// </param>
        public ThankYou(IServiceLocator serviceLocator)
        {
            this.ServiceLocator = serviceLocator;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets ServiceLocator.
        /// </summary>
        public IServiceLocator ServiceLocator { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates an instance of the thank you object from the current information.
        /// </summary>
        /// <param name="username">
        /// The Current Username
        /// </param>
        /// <param name="textTag">
        /// Button Text
        /// </param>
        /// <param name="titleTag">
        /// Button  Title
        /// </param>
        /// <param name="messageId">
        /// The message Id.
        /// </param>
        /// <returns>
        /// Returns ThankYou Info
        /// </returns>
        [NotNull]
        public ThankYouInfo CreateThankYou(
            [NotNull] string username,
            [NotNull] string textTag,
            [NotNull] string titleTag,
            int messageId)
        {
            return new ThankYouInfo
                       {
                           MessageID = messageId,
                           ThanksInfo = this.Get<IThankYou>().ThanksInfo(username, messageId),
                           Text = this.Get<ILocalization>().GetText("BUTTON", textTag),
                           Title = this.Get<ILocalization>().GetText("BUTTON", titleTag)
                       };
        }

        /// <summary>
        /// This method returns a string which shows how many times users have
        ///   thanked the message with the provided messageID. Returns an empty string.
        /// </summary>
        /// <param name="username">
        /// The username.
        /// </param>
        /// <param name="messageId">
        /// The Message ID.
        /// </param>
        /// <returns>
        /// The thanks number.
        /// </returns>
        public string ThanksInfo([NotNull] string username, int messageId)
        {
            var thanksNumber = this.GetRepository<Thanks>().Count(t => t.MessageID == messageId);

            if (thanksNumber == 0)
            {
                return "&nbsp;";
            }

            var thanksText = this.Get<ILocalization>()
                .GetTextFormatted("THANKSINFO", thanksNumber, username);

            var thanks = this.GetThanks(messageId);

            return $@"<a class=""btn btn-sm btn-link thanks-popover"" 
                           data-toggle=""popover"" 
                           data-trigger=""click hover""
                           data-html=""true""
                           title=""{thanksText}"" 
                           data-content=""{thanks.Replace("\"", "'")}"">
                               <i class=""fa fa-heart"" style= ""color:#e74c3c""></i>&nbsp;+{thanksNumber}</a>";
        }

        /// <summary>
        ///     Adds the Thanks info to a dataTable
        /// </summary>
        /// <param name="dataRows"> The data Rows. </param>
        public void AddThanksInfo(IEnumerable<DataRow> dataRows)
        {
            var postRows = dataRows.ToList();
            var messageIds = postRows.Select(x => x.Field<int>("MessageID"));

            // Initialize the "IsThankedByUser" column.
            postRows.ForEach(x => x["IsThankedByUser"] = false);

            // Initialize the "Thank Info" column.
            postRows.ForEach(x => x["ThanksInfo"] = string.Empty);

            // Iterate through all the thanks relating to this topic and make appropriate
            // changes in columns.
            var allThanks = this.GetRepository<Thanks>().MessageGetAllThanks(messageIds.ToDelimitedString(",")).ToList();

            allThanks.Where(t => t.FromUserID != null && t.FromUserID == BoardContext.Current.PageUserID)
                .SelectMany(thanks => postRows.Where(x => x.Field<int>("MessageID") == thanks.MessageID)).ForEach(
                    f =>
                    {
                        f["IsThankedByUser"] = "true";
                        f.AcceptChanges();
                    });

            var thanksFieldNames = new[] { "ThanksFromUserNumber", "ThanksToUserNumber", "ThanksToUserPostsNumber" };

            postRows.ForEach(
                postRow =>
                {
                    var messageId = postRow.Field<int>("MessageID");

                    postRow["MessageThanksNumber"] =
                        allThanks.Count(t => t.FromUserID != null && t.MessageID == messageId);

                    var thanksFiltered = allThanks.Where(t => t.MessageID == messageId).ToList();

                    if (thanksFiltered.Any())
                    {
                        var thanksItem = thanksFiltered.First();

                        postRow["ThanksFromUserNumber"] = thanksItem.ThanksFromUserNumber ?? 0;
                        postRow["ThanksToUserNumber"] = thanksItem.ThanksToUserNumber ?? 0;
                        postRow["ThanksToUserPostsNumber"] = thanksItem.ThanksToUserPostsNumber ?? 0;
                    }
                    else
                    {
                        var row = postRow;
                        thanksFieldNames.ForEach(f => row[f] = 0);
                    }

                    // load all all thanks info into a special column...
                    postRow["ThanksInfo"] = thanksFiltered.Where(t => t.FromUserID != null)
                        .Select(x => $"{x.FromUserID.Value}|{x.ThanksDate}").ToDelimitedString(",");

                    postRow.AcceptChanges();
                });
        }

        /// <summary>
        /// This method returns a string containing the HTML code for
        ///   showing the the post footer. the HTML content is the name of users
        ///   who thanked the post and the date they thanked.
        /// </summary>
        /// <param name="messageId">
        /// The message Id.
        /// </param>
        /// <returns>
        /// The get thanks.
        /// </returns>
        [NotNull]
        private string GetThanks([NotNull] int messageId)
        {
            var filler = new StringBuilder();

            var thanks = this.GetRepository<Thanks>().MessageGetThanksList(messageId);

            filler.Append("<ol>");

            thanks.ForEach(
                dr =>
                    {
                        var name = this.Get<HttpServerUtilityBase>()
                                           .HtmlEncode(this.Get<IUserDisplayName>().GetName(dr.Item2));

                        // vzrus: quick fix for the incorrect link. URL rewriting don't work :(
                        filler.AppendFormat(
                            @"<li class=""list-inline-item""><a id=""{0}"" href=""{1}""><u>{2}</u></a>",
                            dr.Item2.ID,
                            BuildLink.GetUserProfileLink(dr.Item2.ID, name),
                            name);

                        if (this.Get<BoardSettings>().ShowThanksDate)
                        {
                            filler.AppendFormat(
                                " {0}",
                                this.Get<ILocalization>().GetTextFormatted(
                                    "ONDATE",
                                    this.Get<IDateTime>().FormatDateShort(dr.Item1.ThanksDate)));
                        }

                        filler.Append("</li>");
                    });

            filler.Append("</ol>");

            return filler.ToString();
        }

        #endregion
    }
}