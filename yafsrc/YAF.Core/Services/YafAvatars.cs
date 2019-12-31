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

    using System;
    using System.Web;

    using YAF.Configuration;
    using YAF.Core.UsersRoles;
    using YAF.Types;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Utils;

    #endregion

    /// <summary>
    /// The YAF avatars.
    /// </summary>
    public class YafAvatars : IAvatars
    {
        #region Constants and Fields

        /// <summary>
        /// The YAF board settings.
        /// </summary>
        private readonly YafBoardSettings _yafBoardSettings;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="YafAvatars"/> class.
        /// </summary>
        /// <param name="boardSettings">
        /// The board settings.
        /// </param>
        public YafAvatars(YafBoardSettings boardSettings)
        {
            this._yafBoardSettings = boardSettings;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The get avatar url for current user.
        /// </summary>
        /// <returns>
        /// Returns the Avatar Url 
        /// </returns>
        public string GetAvatarUrlForCurrentUser()
        {
            return this.GetAvatarUrlForUser(YafContext.Current.CurrentUserData);
        }

        /// <summary>
        /// The get avatar url for user.
        /// </summary>
        /// <param name="userId">
        /// The user id. 
        /// </param>
        /// <returns>
        /// Returns the Avatar Url 
        /// </returns>
        public string GetAvatarUrlForUser(int userId)
        {
            try
            {
                var userData = new CombinedUserDataHelper(userId);

                return this.GetAvatarUrlForUser(userData);
            }
            catch (Exception)
            {
                // Return NoAvatar Image if there something wrong with the user
                return $"{YafForumInfo.ForumClientFileRoot}images/noavatar.svg";
            }
        }

        /// <summary>
        /// The get avatar url for user.
        /// </summary>
        /// <param name="userData">
        /// The user data. 
        /// </param>
        /// <returns>
        /// Returns the Avatar Url 
        /// </returns>
        public string GetAvatarUrlForUser([NotNull] IUserData userData)
        {
            CodeContracts.VerifyNotNull(userData, "userData");

            var getUserEmail = new Func<string>(
                () =>
                    {
                        string userEmail;
                        try
                        {
                            userEmail = userData.Email;
                        }
                        catch (Exception)
                        {
                            userEmail = string.Empty;
                        }

                        return userEmail;
                    });

            return this.GetAvatarUrlForUser(
                userData.UserID,
                userData.Avatar,
                userData.HasAvatarImage,
                this._yafBoardSettings.AvatarGravatar ? getUserEmail() : string.Empty);
        }

        /// <summary>
        /// The get avatar url for user.
        /// </summary>
        /// <param name="userId">
        /// The user Id. 
        /// </param>
        /// <param name="avatarString">
        /// The avatarString. 
        /// </param>
        /// <param name="hasAvatarImage">
        /// The hasAvatarImage. 
        /// </param>
        /// <param name="email">
        /// The email. 
        /// </param>
        /// <returns>
        /// Returns the Avatar Url 
        /// </returns>
        public string GetAvatarUrlForUser(int userId, string avatarString, bool hasAvatarImage, string email)
        {
            var avatarUrl = string.Empty;

            if (this._yafBoardSettings.AvatarUpload && hasAvatarImage)
            {
                avatarUrl = $"{YafForumInfo.ForumClientFileRoot}resource.ashx?u={userId}";
            }
            else if (avatarString.IsSet())
            {
                // Took out PageContext.BoardSettings.AvatarRemote
                avatarUrl =
                    $"{YafForumInfo.ForumClientFileRoot}resource.ashx?url={HttpUtility.UrlEncode(avatarString)}&width={this._yafBoardSettings.AvatarWidth}&height={this._yafBoardSettings.AvatarHeight}";
            }
            else if (this._yafBoardSettings.AvatarGravatar && email.IsSet())
            {
                const string GravatarBaseUrl = "https://www.gravatar.com/avatar/";

                // JoeOuts added 8/17/09 for Gravatar use
                var gravatarUrl =
                    $@"{GravatarBaseUrl}{email.StringToHexBytes()}.jpg?r={this._yafBoardSettings.GravatarRating}&s={this._yafBoardSettings.AvatarWidth}";

                avatarUrl =
                    $@"{YafForumInfo.ForumClientFileRoot}resource.ashx?url={HttpUtility.UrlEncode(gravatarUrl)}&width={this._yafBoardSettings.AvatarWidth}&height={this._yafBoardSettings.AvatarHeight}";
            }

            // Return NoAvatar Image is no Avatar available for that user.
            if (avatarUrl.IsNotSet())
            {
                avatarUrl = $"{YafForumInfo.ForumClientFileRoot}images/noavatar.svg";
            }

            return avatarUrl;
        }

        #endregion
    }
}