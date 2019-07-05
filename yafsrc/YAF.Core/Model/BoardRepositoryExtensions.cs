/* Yet Another Forum.NET
 * Copyright (C) 2003-2005 Bjørnar Henden
 * Copyright (C) 2006-2013 Jaben Cargman
 * Copyright (C) 2014-2019 Ingo Herbote
 * http://www.yetanotherforum.net/
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
namespace YAF.Core.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using ServiceStack.OrmLite;

    using YAF.Core.Extensions;
    using YAF.Types;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Types.Interfaces.Data;
    using YAF.Types.Models;

    /// <summary>
    ///     The board repository extensions.
    /// </summary>
    public static class BoardRepositoryExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The system_initialize.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="forumName">
        /// The forum name.
        /// </param>
        /// <param name="timeZone">
        /// The time zone.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <param name="languageFile">
        /// The language File.
        /// </param>
        /// <param name="forumEmail">
        /// The forum email.
        /// </param>
        /// <param name="forumBaseUrlMask">
        /// The forum base URL mask.
        /// </param>
        /// <param name="smtpServer">
        /// The SMTP server.
        /// </param>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="userEmail">
        /// The user email.
        /// </param>
        /// <param name="providerUserKey">
        /// The provider user key.
        /// </param>
        /// <param name="rolePrefix">
        /// The role Prefix.
        /// </param>
        public static void SystemInitialize(
            this IRepository<Board> repository,
            [NotNull] string forumName,
            [NotNull] string timeZone,
            [NotNull] string culture,
            [NotNull] string languageFile,
            [NotNull] string forumEmail,
            [NotNull] string forumBaseUrlMask,
            [NotNull] string smtpServer,
            [NotNull] string userName,
            [NotNull] string userEmail,
            [NotNull] object providerUserKey,
            [NotNull] string rolePrefix)
        {
            repository.DbFunction.Scalar.system_initialize(
                Name: forumName,
                TimeZone: timeZone,
                Culture: culture,
                LanguageFile: languageFile,
                ForumEmail: forumEmail,
                ForumBaseUrlMask: forumBaseUrlMask,
                SmtpServer: string.Empty,
                User: userName,
                UserEmail: userEmail,
                UserKey: providerUserKey,
                RolePrefix: rolePrefix,
                UTCTIMESTAMP: DateTime.UtcNow);
        }

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardName">
        /// The board name.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <param name="languageFile">
        /// The language file.
        /// </param>
        /// <param name="membershipAppName">
        /// The membership app name.
        /// </param>
        /// <param name="rolesAppName">
        /// The roles app name.
        /// </param>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="userEmail">
        /// The user email.
        /// </param>
        /// <param name="userKey">
        /// The user key.
        /// </param>
        /// <param name="isHostAdmin">
        /// The is host admin.
        /// </param>
        /// <param name="rolePrefix">
        /// The role prefix.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int Create(
            this IRepository<Board> repository, 
            string boardName, 
            string culture, 
            string languageFile, 
            string membershipAppName, 
            string rolesAppName, 
            string userName, 
            string userEmail, 
            string userKey, 
            bool isHostAdmin, 
            string rolePrefix)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            var boardId =
                (int)
                repository.DbFunction.Scalar.board_create(
                    BoardName: boardName, 
                    Culture: culture, 
                    LanguageFile: languageFile, 
                    MembershipAppName: membershipAppName, 
                    RolesAppName: rolesAppName, 
                    UserName: userName, 
                    UserEmail: userEmail, 
                    UserKey: userKey, 
                    IsHostAdmin: isHostAdmin, 
                    RolePrefix: rolePrefix, 
                    UTCTIMESTAMP: DateTime.UtcNow);

            repository.FireNew(boardId);

            return boardId;
        }

        /// <summary>
        /// The list.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardID">
        /// The board id.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static DataTable List(this IRepository<Board> repository, int? boardID = null)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            return repository.DbFunction.GetData.board_list(BoardID: boardID);
        }

        /// <summary>
        /// The list typed.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardID">
        /// The board id.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public static IList<Board> ListTyped(this IRepository<Board> repository, int? boardID = null)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            return boardID.HasValue
                ? new List<Board> { repository.GetById(boardID.Value) }
                : repository.DbAccess.Execute(cmd => cmd.Connection.Select<Board>());
        }

        /// <summary>
        /// Gets the post stats.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="boardID">The board id.</param>
        /// <param name="styledNicks">The styled nicks.</param>
        /// <param name="showNoCountPosts">The show no count posts.</param>
        /// <returns>
        /// The <see cref="DataTable" />.
        /// </returns>
        public static DataRow PostStats(this IRepository<Board> repository, int boardID, bool styledNicks, bool showNoCountPosts)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            var dt =
                (DataTable)
                repository.DbFunction.GetData.board_poststats(
                    BoardID: boardID, StyledNicks: styledNicks, ShowNoCountPosts: showNoCountPosts, GetDefaults: 0);

            if (dt.HasRows())
            {
                return dt.Rows[0];
            }

            dt =
                (DataTable)
                repository.DbFunction.GetData.board_poststats(
                    BoardID: boardID, StyledNicks: styledNicks, ShowNoCountPosts: showNoCountPosts, GetDefaults: 1);

            return dt.HasRows() ? dt.Rows[0] : null;
        }

        /// <summary>
        /// Re-Sync the Board
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardID">
        /// The board id.
        /// </param>
        public static void ReSync(this IRepository<Board> repository, int? boardID = null)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            repository.DbFunction.Query.board_resync(BoardID: boardID);
        }

        /// <summary>
        /// The save.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="boardID">The board id.</param>
        /// <param name="name">The name.</param>
        /// <param name="languageFile">The language file.</param>
        /// <param name="culture">The culture.</param>
        /// <param name="allowThreaded">The allow threaded.</param>
        public static void Save(
            this IRepository<Board> repository,
            int boardID,
            string name,
            string languageFile,
            string culture,
            bool allowThreaded)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            repository.DbFunction.Query.board_save(
                BoardID: boardID,
                Name: name,
                LanguageFile: languageFile,
                Culture: culture,
                AllowThreaded: allowThreaded);

            repository.FireUpdated(boardID);
        }

        /// <summary>
        /// The stats.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardID">
        /// The board id.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static DataRow Stats(this IRepository<Board> repository, int? boardID)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            return ((DataTable)repository.DbFunction.GetData.board_stats(BoardID: boardID)).Rows[0];
        }

        /// <summary>
        /// Gets the User Stats.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardID">
        /// The board id.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static DataRow UserStats(this IRepository<Board> repository, int? boardID)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            return ((DataTable)repository.DbFunction.GetData.board_userstats(BoardID: boardID)).Rows[0];
        }

        #endregion
    }
}