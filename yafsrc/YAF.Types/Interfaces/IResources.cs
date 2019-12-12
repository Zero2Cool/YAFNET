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
namespace YAF.Types.Interfaces
{
    using System.Web;

    /// <summary>
    /// The Resources interface.
    /// </summary>
    public interface IResources
    {
        /// <summary>
        /// Gets the forum user info as JSON string for the hover cards
        /// </summary>
        /// <param name="context">The context.</param>
        void GetUserInfo([NotNull] HttpContext context);

        /// <summary>
        /// Gets the list of all Custom BB Codes
        /// </summary>
        /// <param name="context">The context.</param>
        void GetCustomBBCodes([NotNull] HttpContext context);

        /// <summary>
        /// Get all Mentioned Users
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        void GetMentionUsers([NotNull] HttpContext context);

        /// <summary>
        /// Gets the twitter user info as JSON string for the hover cards
        /// </summary>
        /// <param name="context">The context.</param>
        void GetTwitterUserInfo([NotNull] HttpContext context);

        /// <summary>
        /// The get response local avatar.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        void GetResponseLocalAvatar([NotNull] HttpContext context);

        /// <summary>
        /// The get response captcha.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        void GetResponseCaptcha([NotNull] HttpContext context);

        /// <summary>
        /// The get response remote avatar.
        /// </summary>
        /// <param name="context">
        /// The context.
        /// </param>
        void GetResponseRemoteAvatar([NotNull] HttpContext context);
    }
}