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
    #region Using

    using System.Web;
    using System.Web.Hosting;

    using YAF.Configuration;
    using YAF.Core.Context;
    using YAF.Core.Services.Startup;
    using YAF.Types;
    using YAF.Types.Constants;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Types.Interfaces.Identity;
    using YAF.Utils;
    using YAF.Utils.Helpers;

    #endregion

    /// <summary>
    /// The permissions.
    /// </summary>
    public class Permissions : IPermissions, IHaveServiceLocator
    {
        #region Properties

        /// <summary>
        /// Gets or sets ServiceLocator.
        /// </summary>
        public IServiceLocator ServiceLocator { get; set; }

        #endregion

        #region Implemented Interfaces

        #region IPermissions

        /// <summary>
        /// Check Viewing Permissions
        /// </summary>
        /// <param name="permission">
        /// The permission.
        /// </param>
        /// <returns>
        /// The check.
        /// </returns>
        public bool Check(ViewPermissions permission)
        {
            return permission switch
                {
                    ViewPermissions.Everyone => true,
                    ViewPermissions.RegisteredUsers => !BoardContext.Current.IsGuest,
                    _ => BoardContext.Current.IsAdmin
                };
        }

        /// <summary>
        /// The handle request.
        /// </summary>
        /// <param name="permission">
        /// The permission.
        /// </param>
        public void HandleRequest(ViewPermissions permission)
        {
            var noAccess = true;

            if (this.Check(permission))
            {
                return;
            }

            if (permission == ViewPermissions.RegisteredUsers)
            {
                if (!Config.AllowLoginAndLogoff && this.Get<BoardSettings>().CustomLoginRedirectUrl.IsSet())
                {
                    var loginRedirectUrl = this.Get<BoardSettings>().CustomLoginRedirectUrl;

                    if (loginRedirectUrl.Contains("{0}"))
                    {
                        // process for return url..
                        loginRedirectUrl = string.Format(
                            loginRedirectUrl, HttpUtility.UrlEncode(
                                General.GetSafeRawUrl(this.Get<HttpRequestBase>().Url.ToString())));
                    }

                    // allow custom redirect...
                    this.Get<HttpResponseBase>().Redirect(loginRedirectUrl);
                    noAccess = false;
                }
                else if (!Config.AllowLoginAndLogoff && Config.IsDotNetNuke)
                {
                    // automatic DNN redirect...
                    var appPath = HostingEnvironment.ApplicationVirtualPath;
                    if (!appPath.EndsWith("/"))
                    {
                        appPath += "/";
                    }

                    // redirect to DNN login...
                    this.Get<HttpResponseBase>().Redirect(
                        $"{appPath}Login.aspx?ReturnUrl={HttpUtility.UrlEncode(General.GetSafeRawUrl())}");
                    noAccess = false;
                }
                else if (Config.AllowLoginAndLogoff)
                {
                    BuildLink.Redirect(
                        ForumPages.Account_Login,
                        "ReturnUrl={0}",
                        HttpUtility.UrlEncode(General.GetSafeRawUrl()));
                    noAccess = false;
                }
            }

            // fall-through with no access...
            if (noAccess)
            {
                BuildLink.AccessDenied();
            }
        }

        /// <summary>
        /// Checks the access rights.
        /// </summary>
        /// <param name="boardId">The board id.</param>
        /// <param name="messageId">The message id.</param>
        /// <returns>
        /// The check access rights.
        /// </returns>
        public bool CheckAccessRights([NotNull] int boardId, [NotNull] int messageId)
        {
            if (messageId.Equals(0))
            {
                return true;
            }

            // Find user name
            var user = this.Get<IAspNetUsersHelper>().GetUser();

            var browser =
                $"{HttpContext.Current.Request.Browser.Browser} {HttpContext.Current.Request.Browser.Version}";
            var platform = HttpContext.Current.Request.Browser.Platform;
            var isMobileDevice = HttpContext.Current.Request.Browser.IsMobileDevice;
            var userAgent = HttpContext.Current.Request.UserAgent;

            // try and get more verbose platform name by ref and other parameters             
            UserAgentHelper.Platform(
                userAgent,
                this.Get<HttpRequestBase>().Browser.Crawler,
                ref platform,
                ref browser,
                out var isSearchEngine,
                out var doNotTrack);

            this.Get<StartupInitializeDb>().Run();

            string userKey = null;

            if (user != null)
            {
                userKey = user.Id;
            }

            var pageRow = BoardContext.Current.Get<DataBroker>().GetPageLoad(
                HttpContext.Current.Session.SessionID,
                boardId,
                userKey,
                HttpContext.Current.Request.GetUserRealIPAddress(),
                HttpContext.Current.Request.FilePath,
                HttpContext.Current.Request.QueryString.ToString(),
                browser,
                platform,
                null,
                null,
                null,
                messageId,
                isSearchEngine, // don't track if this is a search engine
                isMobileDevice,
                doNotTrack);

            return pageRow.DownloadAccess || pageRow.ModeratorAccess;
        }

        #endregion

        #endregion
    }
}