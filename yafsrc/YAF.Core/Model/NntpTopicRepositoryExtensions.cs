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
namespace YAF.Core.Model
{
    #region Using

    using System;

    using YAF.Types;
    using YAF.Types.Interfaces.Data;
    using YAF.Types.Models;

    #endregion

    /// <summary>
    ///     The NntpTopic repository extensions.
    /// </summary>
    public static class NntpTopicRepositoryExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The save message.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="nntpForumId">
        /// The nntp forum id.
        /// </param>
        /// <param name="topic">
        /// The topic.
        /// </param>
        /// <param name="body">
        /// The body.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="ip">
        /// The IP Address.
        /// </param>
        /// <param name="posted">
        /// The posted.
        /// </param>
        /// <param name="externalMessageId">
        /// The external message id.
        /// </param>
        /// <param name="referenceMessageId">
        /// The reference message id.
        /// </param>
        public static void SaveMessage(
            this IRepository<NntpTopic> repository,
            [NotNull] int nntpForumId,
            [NotNull] string topic,
            [NotNull] string body,
            [NotNull] int userId,
            [NotNull] string userName,
            [NotNull] string ip,
            [NotNull] DateTime posted,
            [NotNull] string externalMessageId,
            [NotNull] string referenceMessageId)
        {
            CodeContracts.VerifyNotNull(repository);

            repository.DbFunction.Scalar.nntptopic_savemessage(
                NntpForumID: nntpForumId,
                Topic: topic,
                Body: body,
                UserID: userId,
                UserName: userName,
                IP: ip,
                Posted: posted,
                ExternalMessageId: externalMessageId,
                ReferenceMessageId: referenceMessageId,
                UTCTIMESTAMP: DateTime.UtcNow);
        }

        #endregion
    }
}