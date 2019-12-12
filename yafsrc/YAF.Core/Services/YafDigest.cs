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
namespace YAF.Core.Services
{
    #region Using

    using System.Net;

    using YAF.Configuration;
    using YAF.Core.Extensions;
    using YAF.Core.Model;
    using YAF.Types;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Types.Models;

    #endregion

    /// <summary>
    /// The YAF digest.
    /// </summary>
    public class YafDigest : IDigest, IHaveServiceLocator
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YafDigest"/> class.
        /// </summary>
        /// <param name="serviceLocator">
        /// The service locator.
        /// </param>
        public YafDigest([NotNull] IServiceLocator serviceLocator)
        {
            this.ServiceLocator = serviceLocator;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the ServiceLocator.
        /// </summary>
        public IServiceLocator ServiceLocator { get; }

        #endregion

        #region Implemented Interfaces

        #region IDigest

        /// <summary>
        /// Gets the digest HTML.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="boardSettings">The board settings.</param>
        /// <param name="showErrors">if set to <c>true</c> [show errors].</param>
        /// <returns>
        /// The get digest html.
        /// </returns>
        public string GetDigestHtml(int userId, object boardSettings, bool showErrors = false)
        {
            var request = (HttpWebRequest)WebRequest.Create(this.GetDigestUrl(userId, boardSettings, showErrors));

            var digestHtml = string.Empty;

            // set timeout to max 45 seconds
            request.Timeout = 45 * 1000;

            var response = request.GetResponse().ToClass<HttpWebResponse>();

            if (response.StatusCode == HttpStatusCode.OK)
            {
                digestHtml = response.GetResponseStream().AsString();
            }

            return digestHtml;
        }

        /// <summary>
        /// Gets the digest URL.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="boardSettings">The board settings.</param>
        /// <returns>
        /// The get digest url.
        /// </returns>
        public string GetDigestUrl(int userId, object boardSettings)
        {
            return this.GetDigestUrl(userId, boardSettings, false);
        }

        /// <summary>
        /// Gets the digest URL.
        /// </summary>
        /// <param name="userId">The user id.</param>
        /// <param name="boardSettings">The board settings.</param>
        /// <param name="showErrors">Show errors creating the digest.</param>
        /// <returns>
        /// The get digest url.
        /// </returns>
        public string GetDigestUrl(int userId, object boardSettings, bool showErrors)
        {
            var yafBoardSettings = boardSettings as YafBoardSettings;

            return
                $"{yafBoardSettings.BaseUrlMask}{BaseUrlBuilder.AppPath}digest.aspx?token={yafBoardSettings.WebServiceToken}&userid={userId}&boardid={yafBoardSettings.BoardID}&showerror={showErrors.ToString().ToLower()}";
        }

        /// <summary>
        /// Sends the digest html to the email/name specified.
        /// </summary>
        /// <param name="digestHtml">The digest html.</param>
        /// <param name="forumName">The forum name.</param>
        /// <param name="forumEmail">The forum email.</param>
        /// <param name="toEmail">The to email.</param>
        /// <param name="toName">The to name.</param>
        /// <param name="sendQueued">The send queued.</param>
        public void SendDigest(
            [NotNull] string digestHtml,
            [NotNull] string forumName,
            [NotNull] string forumEmail,
            [NotNull] string toEmail,
            [CanBeNull] string toName,
            bool sendQueued)
        {
            CodeContracts.VerifyNotNull(digestHtml, "digestHtml");
            CodeContracts.VerifyNotNull(forumName, "forumName");
            CodeContracts.VerifyNotNull(forumEmail, "forumEmail");
            CodeContracts.VerifyNotNull(toEmail, "toEmail");

            var subject = this.Get<ILocalization>().GetTextFormatted("SUBJECT", forumName);

            if (sendQueued)
            {
                // queue to send...
                this.GetRepository<Mail>().Create(
                    forumEmail,
                    forumName,
                    toEmail,
                    toName,
                    subject,
                    "You must have HTML Email Viewer to View.",
                    digestHtml);
            }
            else
            {
                // send direct...
                this.Get<ISendMail>().Send(
                    forumEmail,
                    forumName,
                    toEmail,
                    toName,
                    forumEmail,
                    forumName,
                    subject,
                    "You must have HTML Email Viewer to View.",
                    digestHtml);
            }
        }

        #endregion

        #endregion
    }
}