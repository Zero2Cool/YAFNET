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
    using System;
    using System.Dynamic;
    using System.Linq;

    using ServiceStack.OrmLite;

    using YAF.Core.Context;
    using YAF.Core.Extensions;
    using YAF.Types;
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
        /// The create.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardName">
        /// The board name.
        /// </param>
        /// <param name="boardEmail">
        /// The board Email.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <param name="languageFile">
        /// The language file.
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
            [NotNull] string boardName,
            [NotNull] string boardEmail,
            [NotNull] string culture,
            [NotNull] string languageFile,
            [NotNull] string userName,
            [NotNull] string userEmail,
            [NotNull] string userKey,
            [NotNull] bool isHostAdmin,
            [CanBeNull] string rolePrefix)
        {
            CodeContracts.VerifyNotNull(repository);

            // -- Board
            var newBoardId = repository.Insert(new Board { Name = boardName });

            BoardContext.Current.GetRepository<Registry>().Save("culture", culture);
            BoardContext.Current.GetRepository<Registry>().Save("language", languageFile);

            // -- Rank
            var rankIDAdmin = BoardContext.Current.GetRepository<Rank>().Insert(
                new Rank
                {
                    BoardID = newBoardId,
                    Name = "Administration",
                    Flags = 0,
                    MinPosts = 0,
                    PMLimit = 2147483647,
                    Style = "font-size: 8pt; color: #811334",
                    SortOrder = 0
                });

            var rankIDGuest = BoardContext.Current.GetRepository<Rank>().Insert(
                new Rank
                {
                    BoardID = newBoardId,
                    Name = "Guest",
                    Flags = 0,
                    MinPosts = 0,
                    PMLimit = 0,
                    SortOrder = 100
                });

            BoardContext.Current.GetRepository<Rank>().Insert(
                new Rank
                {
                    BoardID = newBoardId,
                    Name = "Newbie",
                    Flags = 0,
                    MinPosts = 3,
                    PMLimit = 0,
                    SortOrder = 3
                });

            BoardContext.Current.GetRepository<Rank>().Insert(
                new Rank
                {
                    BoardID = newBoardId,
                    Name = "Member",
                    Flags = 2,
                    MinPosts = 10,
                    PMLimit = 30,
                    SortOrder = 2
                });

            BoardContext.Current.GetRepository<Rank>().Insert(
                new Rank
                {
                    BoardID = newBoardId,
                    Name = "Advanced Member",
                    Flags = 2,
                    MinPosts = 30,
                    PMLimit = 100,
                    SortOrder = 1
                });

            // -- AccessMask
            var accessMaskIDAdmin = BoardContext.Current.GetRepository<AccessMask>().Insert(
                new AccessMask { BoardID = newBoardId, Name = "Admin Access", Flags = 1023 + 1024, SortOrder = 4 });

            BoardContext.Current.GetRepository<AccessMask>().Insert(
                new AccessMask { BoardID = newBoardId, Name = "Moderator Access", Flags = 487 + 1024, SortOrder = 3 });

            var accessMaskIDMember = BoardContext.Current.GetRepository<AccessMask>().Insert(
                new AccessMask { BoardID = newBoardId, Name = "Member Access", Flags = 423 + 1024, SortOrder = 2 });

            var accessMaskIDReadOnly = BoardContext.Current.GetRepository<AccessMask>().Insert(
                new AccessMask { BoardID = newBoardId, Name = "Read Only Access", Flags = 1, SortOrder = 1 });

            BoardContext.Current.GetRepository<AccessMask>().Insert(
                new AccessMask { BoardID = newBoardId, Name = "No Access", Flags = 0, SortOrder = 0 });

            // -- Group
            var groupIDAdmin = BoardContext.Current.GetRepository<Group>().Insert(
                new Group
                {
                    BoardID = newBoardId,
                    Name = $"{rolePrefix}Administrators",
                    Flags = 1,
                    PMLimit = 2147483647,
                    Style = "font-size: 8pt; color: red",
                    SortOrder = 0,
                    UsrSigChars = 256,
                    UsrSigBBCodes = "URL,IMG,SPOILER,QUOTE",
                    UsrAlbums = 10,
                    UsrAlbumImages = 120
                });

            var groupIDGuest = BoardContext.Current.GetRepository<Group>().Insert(
                new Group
                {
                    BoardID = newBoardId,
                    Name = "Guests",
                    Flags = 2,
                    PMLimit = 0,
                    Style = "font-style: italic; font-weight: bold; color: #0c7333",
                    SortOrder = 1,
                    UsrSigChars = 0,
                    UsrSigBBCodes = null,
                    UsrAlbums = 0,
                    UsrAlbumImages = 0
                });

            var groupIDMember = BoardContext.Current.GetRepository<Group>().Insert(
                new Group
                {
                    BoardID = newBoardId,
                    Name = $"{rolePrefix}Registered",
                    Flags = 4,
                    PMLimit = 100,
                    SortOrder = 2,
                    UsrSigChars = 128,
                    UsrSigBBCodes = "URL,IMG,SPOILER,QUOTE",
                    UsrAlbums = 5,
                    UsrAlbumImages = 30
                });

            // -- User (GUEST)
            var userIDGuest = BoardContext.Current.GetRepository<User>().Insert(
                new User
                {
                    BoardID = newBoardId,
                    RankID = rankIDGuest,
                    Name = "Guest",
                    DisplayName = "Guest",
                    Password = "na",
                    Joined = DateTime.Now,
                    LastVisit = DateTime.Now,
                    NumPosts = 0,
                    TimeZone = TimeZoneInfo.Local.Id,
                    Email = boardEmail,
                    Flags = 6
                });

            var userFlags = 2;

            if (isHostAdmin)
            {
                userFlags = 3;
            }

            // -- User (ADMIN)
            var userIDAdmin = BoardContext.Current.GetRepository<User>().Insert(
                new User
                {
                    BoardID = newBoardId,
                    RankID = rankIDAdmin,
                    Name = userName,
                    DisplayName = userName,
                    Password = "na",
                    ProviderUserKey = userKey,
                    Joined = DateTime.Now,
                    LastVisit = DateTime.Now,
                    NumPosts = 0,
                    TimeZone = TimeZoneInfo.Local.Id,
                    Email = boardEmail,
                    Flags = userFlags
                });

            // -- UserGroup
            BoardContext.Current.GetRepository<UserGroup>().Insert(
                new UserGroup { UserID = userIDAdmin, GroupID = groupIDAdmin });

            BoardContext.Current.GetRepository<UserGroup>().Insert(
                new UserGroup { UserID = userIDGuest, GroupID = groupIDGuest });

            // -- Category
            var categoryID = BoardContext.Current.GetRepository<Category>().Insert(
                new Category { BoardID = newBoardId, Name = "Test Category", SortOrder = 1 });

            // -- Forum
            var forumID = BoardContext.Current.GetRepository<Forum>().Insert(
                new Forum
                {
                    CategoryID = categoryID,
                    Name = "Test Forum",
                    Description = "A test forum",
                    SortOrder = 1,
                    NumTopics = 0,
                    NumPosts = 0,
                    Flags = 4
                });

            // -- ForumAccess
            BoardContext.Current.GetRepository<ForumAccess>().Insert(
                new ForumAccess { GroupID = groupIDAdmin, ForumID = forumID, AccessMaskID = accessMaskIDAdmin });

            BoardContext.Current.GetRepository<ForumAccess>().Insert(
                new ForumAccess { GroupID = groupIDGuest, ForumID = forumID, AccessMaskID = accessMaskIDReadOnly });

            BoardContext.Current.GetRepository<ForumAccess>().Insert(
                new ForumAccess { GroupID = groupIDMember, ForumID = forumID, AccessMaskID = accessMaskIDMember });

            repository.FireNew(newBoardId);

            return newBoardId;
        }

        /// <summary>
        /// Gets the post stats.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardId">
        /// The board id.
        /// </param>
        /// <param name="showNoCountPosts">
        /// The show no count posts.
        /// </param>
        /// <returns>
        /// The <see cref="dynamic"/>.
        /// </returns>
        public static dynamic PostStats(this IRepository<Board> repository, int boardId, bool showNoCountPosts)
        {
            CodeContracts.VerifyNotNull(repository);

            var data = repository.DbAccess.Execute(
                db =>
                {
                    var expression = OrmLiteConfig.DialectProvider.SqlExpression<Message>();

                    expression.Join<Topic>((a, b) => b.ID == a.TopicID).Join<Topic, Forum>((b, c) => c.ID == b.ForumID)
                        .Join<Forum, Category>((c, d) => d.ID == c.CategoryID).Join<User>((a, e) => e.ID == a.UserID);

                    expression.Where<Message, Topic, Forum, Category>(
                        (a, b, c, d) => (a.Flags & 24) == 16 && b.IsDeleted == false && d.BoardID == boardId);

                    if (!showNoCountPosts)
                    {
                        expression.And<Forum>(c => c.IsNoCount == false);
                    }

                    expression.OrderByDescending(a => a.Posted).Limit(1);

                    // -- count Posts
                    var countPostsExpression = db.Connection.From<Message>();

                    countPostsExpression.Join<Topic>((a, b) => b.ID == a.TopicID)
                        .Join<Topic, Forum>((b, c) => c.ID == b.ForumID)
                        .Join<Forum, Category>((b, c) => c.ID == b.CategoryID);

                    countPostsExpression.Where<Category>(c => c.BoardID == boardId);

                    var countPostsSql = countPostsExpression.Select(Sql.Count("1")).ToMergedParamsSelectStatement();

                    // -- count Topics
                    var countTopicsExpression = db.Connection.From<Topic>();

                    countTopicsExpression.Join<Forum>((a, b) => b.ID == a.ForumID)
                        .Join<Forum, Category>((b, c) => c.ID == b.CategoryID);

                    countTopicsExpression.Where<Category>(c => c.BoardID == boardId);

                    var countTopicsSql = countTopicsExpression.Select(Sql.Count("1")).ToMergedParamsSelectStatement();

                    // -- count Forums
                    var countForumsExpression = db.Connection.From<Forum>();

                    countForumsExpression.Join<Category>((a, b) => b.ID == a.CategoryID);

                    countForumsExpression.Where<Category>(x => x.BoardID == boardId);

                    var countForumsSql = countForumsExpression.Select(Sql.Count("1")).ToMergedParamsSelectStatement();

                    expression.Select<Message, User>(
                        (a, e) => new
                        {
                            Posts = Sql.Custom($"({countPostsSql})"),
                            Topics = Sql.Custom($"({countTopicsSql})"),
                            Forums = Sql.Custom($"({countForumsSql})"),
                            LastPost = a.Posted,
                            LastUserID = a.UserID,
                            LastUser = e.Name,
                            LastUserDisplayName = e.DisplayName,
                            LastUserStyle = e.UserStyle,
                            LastUserSuspended = e.Suspended
                        });

                    return db.Connection.Select<dynamic>(expression);
                });

            if (data != null && data.Any())
            {
                return data.FirstOrDefault();
            }

            var linkDate = DateTime.UtcNow;

            var topics  = BoardContext.Current.GetRepository<Topic>().Get(
                x => x.TopicMovedID != null && x.LinkDate != null && x.LinkDate < linkDate);

            topics.ForEach(t => BoardContext.Current.GetRepository<Topic>().Delete(t.ForumID, t.ID, true));

            dynamic stats = new ExpandoObject();

            // Get defaults
            stats.Posts = 0;
            stats.Topics = 0;
            stats.Forums = 1;
            stats.LastPost = null;
            stats.LastUserID = null;
            stats.LastUser = null;
            stats.LastUserDisplayName = null;
            stats.LastUserStyle = string.Empty;
            stats.LastUserSuspended = null;

            return stats;
        }

        /// <summary>
        /// Save Board Settings (Name, Culture and Language File)
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="boardId">The board id.</param>
        /// <param name="name">The name.</param>
        /// <param name="languageFile">The language file.</param>
        /// <param name="culture">The culture.</param>
        public static void Save(
            this IRepository<Board> repository,
            int boardId,
            string name,
            string languageFile,
            string culture)
        {
            CodeContracts.VerifyNotNull(repository);

            BoardContext.Current.GetRepository<Registry>().Save("culture", culture, boardId);
            BoardContext.Current.GetRepository<Registry>().Save("language", languageFile, boardId);

            repository.UpdateOnly(() => new Board { Name = name }, board => board.ID == boardId);

            repository.FireUpdated(boardId);
        }

        /// <summary>
        /// The stats.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardId">
        /// The board Id.
        /// </param>
        /// <returns>
        /// The <see cref="dynamic"/>.
        /// </returns>
        public static dynamic Stats(this IRepository<Board> repository, int boardId)
        {
            CodeContracts.VerifyNotNull(repository);

            return repository.DbAccess.Execute(
                db =>
                {
                    var expression = OrmLiteConfig.DialectProvider.SqlExpression<User>();

                    expression.Where(u => u.BoardID == boardId);

                    // -- count Posts
                    var countPostsExpression = db.Connection.From<Message>();

                    countPostsExpression.Join<Topic>((a, b) => b.ID == a.TopicID)
                        .Join<Topic, Forum>((b, c) => c.ID == b.ForumID)
                        .Join<Forum, Category>((b, c) => c.ID == b.CategoryID);

                    countPostsExpression.Where<Category>(c => c.BoardID == boardId);

                    var countPostsSql = countPostsExpression.Select(Sql.Count("1")).ToMergedParamsSelectStatement();

                    // -- count Topics
                    var countTopicsExpression = db.Connection.From<Topic>();

                    countTopicsExpression.Join<Forum>((a, b) => b.ID == a.ForumID)
                        .Join<Forum, Category>((b, c) => c.ID == b.CategoryID);

                    countTopicsExpression.Where<Category>(c => c.BoardID == boardId);

                    var countTopicsSql = countTopicsExpression.Select(Sql.Count("1")).ToMergedParamsSelectStatement();

                    // -- count Users
                    var countUsersExpression = expression;

                    countUsersExpression.Where(u => u.IsApproved == true && u.BoardID == boardId);

                    var countUsersSql = countUsersExpression.Select(Sql.Count("1")).ToMergedParamsSelectStatement();

                    expression.Select<User>(
                        x => new
                        {
                            Posts = Sql.Custom($"({countPostsSql})"),
                            Topics = Sql.Custom($"({countTopicsSql})"),
                            Users = Sql.Custom($"({countUsersSql})"),
                            BoardStart = Sql.Min(x.Joined)
                        });

                    return db.Connection.Select<dynamic>(expression);
                }).FirstOrDefault();
        }

        /// <summary>
        /// The delete board.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardId">
        /// The board id.
        /// </param>
        public static void DeleteBoard(this IRepository<Board> repository, int boardId)
        {
            CodeContracts.VerifyNotNull(repository);

            // --- Delete all forums of the board
            var forums = BoardContext.Current.GetRepository<Forum>().ListAll(boardId);

            forums.ForEach(f => BoardContext.Current.GetRepository<Forum>().Delete(f.Item1.ID));

            // --- Delete user(s)
            var users = BoardContext.Current.GetRepository<User>().Get(u => u.BoardID == boardId);

            users.ForEach(u => BoardContext.Current.GetRepository<User>().Delete(u.ID));

            // --- Delete Group
            var groups = BoardContext.Current.GetRepository<Group>().Get(g => g.BoardID == boardId);

            groups.ForEach(
                g =>
                {
                    BoardContext.Current.GetRepository<GroupMedal>().Delete(x => x.GroupID == g.ID);
                    BoardContext.Current.GetRepository<ForumAccess>().Delete(x => x.GroupID == g.ID);
                    BoardContext.Current.GetRepository<UserGroup>().Delete(x => x.GroupID == g.ID);
                    BoardContext.Current.GetRepository<Group>().Delete(x => x.ID == g.ID);
                });

            BoardContext.Current.GetRepository<Category>().Delete(x => x.BoardID == boardId);
            BoardContext.Current.GetRepository<ActiveAccess>().Delete(x => x.BoardID == boardId);
            BoardContext.Current.GetRepository<Active>().Delete(x => x.BoardID == boardId);
            BoardContext.Current.GetRepository<Rank>().Delete(x => x.BoardID == boardId);
            BoardContext.Current.GetRepository<AccessMask>().Delete(x => x.BoardID == boardId);
            BoardContext.Current.GetRepository<BBCode>().Delete(x => x.BoardID == boardId);
            BoardContext.Current.GetRepository<Medal>().Delete(x => x.BoardID == boardId);
            BoardContext.Current.GetRepository<Replace_Words>().Delete(x => x.BoardID == boardId);
            BoardContext.Current.GetRepository<Spam_Words>().Delete(x => x.BoardID == boardId);
            BoardContext.Current.GetRepository<NntpServer>().Delete(x => x.BoardID == boardId);
            BoardContext.Current.GetRepository<BannedIP>().Delete(x => x.BoardID == boardId);
            BoardContext.Current.GetRepository<BannedName>().Delete(x => x.BoardID == boardId);
            BoardContext.Current.GetRepository<BannedEmail>().Delete(x => x.BoardID == boardId);
            BoardContext.Current.GetRepository<Registry>().Delete(x => x.BoardID == boardId);
            BoardContext.Current.GetRepository<Board>().Delete(x => x.ID == boardId);
        }

        #endregion
    }
}