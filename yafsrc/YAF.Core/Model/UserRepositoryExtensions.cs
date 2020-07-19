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
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.Linq;

    using ServiceStack.OrmLite;

    using YAF.Core.Context;
    using YAF.Core.Extensions;
    using YAF.Types;
    using YAF.Types.Extensions;
    using YAF.Types.Extensions.Data;
    using YAF.Types.Interfaces;
    using YAF.Types.Interfaces.Data;
    using YAF.Types.Models;
    using YAF.Types.Models.Identity;
    using YAF.Utils.Helpers;

    /// <summary>
    /// The User Repository Extensions
    /// </summary>
    public static class UserRepositoryExtensions
    {
        /// <summary>
        /// List the reporters as data table.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageId">
        /// The message id.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static List<Tuple<MessageReportedAudit, User>> MessageReporters(
            this IRepository<User> repository,
            [NotNull] int messageId)
        {
            var expression = OrmLiteConfig.DialectProvider.SqlExpression<MessageReportedAudit>();

            expression.Join<User>((m, u) => u.ID == m.UserID)
                .Where<MessageReportedAudit>(m => m.MessageID == messageId);

            return repository.DbAccess.Execute(db => db.Connection.SelectMulti<MessageReportedAudit, User>(expression));
        }

        /// <summary>
        /// List the reporters as data table.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageId">
        /// The message id.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static List<Tuple<User, MessageReportedAudit>> MessageReporter(
            this IRepository<User> repository,
            [NotNull] int messageId,
            [NotNull] int userId)
        {
            var expression = OrmLiteConfig.DialectProvider.SqlExpression<User>();

            expression.Join<MessageReportedAudit>((user, m) => m.UserID == userId && m.MessageID == messageId)
                .Where<User>(user => user.ID == userId);

            return repository.DbAccess.Execute(db => db.Connection.SelectMulti<User, MessageReportedAudit>(expression));
        }

        /// <summary>
        /// The user_activity_rank.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardId">
        /// The board id.
        /// </param>
        /// <param name="startDate">
        /// The start date.
        /// </param>
        /// <param name="displayNumber">
        /// The display number.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static DataTable ActivityRankAsDataTable(
            this IRepository<User> repository,
            [NotNull] int boardId,
            [NotNull] DateTime startDate,
            [NotNull] int displayNumber)
        {
            return repository.DbFunction.GetData.user_activity_rank(
                BoardID: boardId,
                StartDate: startDate,
                DisplayNumber: displayNumber);
        }

        /// <summary>
        /// Add Reputation Points to the specified user id.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userID">
        /// The user ID.
        /// </param>
        /// <param name="fromUserID">
        /// From user ID.
        /// </param>
        /// <param name="points">
        /// The points.
        /// </param>
        public static void AddPoints(
            this IRepository<User> repository,
            [NotNull] int userID,
            [CanBeNull] int? fromUserID,
            [NotNull] int points)
        {
            repository.DbFunction.Scalar.user_addpoints(
                UserID: userID,
                FromUserID: fromUserID,
                UTCTIMESTAMP: DateTime.UtcNow,
                Points: points);
        }

        /// <summary>
        /// Remove Reputation Points from the specified user id.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userID">
        /// The user ID.
        /// </param>
        /// <param name="fromUserID">
        /// From user ID.
        /// </param>
        /// <param name="points">
        /// The points.
        /// </param>
        public static void RemovePoints(
            this IRepository<User> repository,
            [NotNull] int userID,
            [CanBeNull] int? fromUserID,
            [NotNull] int points)
        {
            repository.DbFunction.Scalar.user_removepoints(
                UserID: userID,
                FromUserID: fromUserID,
                UTCTIMESTAMP: DateTime.UtcNow,
                Points: points);
        }

        /// <summary>
        /// Update the Admin User
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardID">
        /// The board id.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="displayName">
        /// The display Name.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <param name="rankID">
        /// The rank id.
        /// </param>
        public static void AdminSave(
            this IRepository<User> repository,
            [NotNull] int boardID,
            [NotNull] int userId,
            [NotNull] string name,
            [NotNull] string displayName,
            [NotNull] string email,
            [NotNull] int flags,
            [NotNull] int rankID)
        {
            repository.UpdateOnly(
                () => new User
                {
                    BoardID = boardID,
                    Name = name,
                    DisplayName = displayName,
                    Email = email,
                    Flags = flags,
                    RankID = rankID
                },
                u => u.ID == userId);
        }

        /// <summary>
        /// Approves the User
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userID">
        /// The user id.
        /// </param>
        public static void Approve(this IRepository<User> repository, [NotNull] int userID)
        {
            repository.DbFunction.Scalar.user_approve(UserID: userID);
        }

        /// <summary>
        /// Approves all unapproved users
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardID">
        /// The board id.
        /// </param>
        public static void ApproveAll(this IRepository<User> repository, [NotNull] int boardID)
        {
            repository.DbFunction.Scalar.user_approveall(BoardID: boardID);
        }

        /// <summary>
        /// The user_AspNet.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardID">
        /// The board id.
        /// </param>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="displayName">
        /// The display Name.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="providerUserKey">
        /// The provider user key.
        /// </param>
        /// <param name="isApproved">
        /// The is approved.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int AspNet(
            this IRepository<User> repository,
            int boardID,
            [NotNull] string userName,
            [NotNull] string displayName,
            [NotNull] string email,
            [NotNull] object providerUserKey,
            [NotNull] bool isApproved)
        {
            try
            {
                return repository.DbFunction.Scalar.user_aspnet(
                    BoardID: boardID,
                    UserName: userName,
                    DisplayName: displayName,
                    Email: email,
                    ProviderUserKey: providerUserKey,
                    IsApproved: isApproved,
                    UTCTIMESTAMP: DateTime.UtcNow);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        /// <summary>
        /// Delete the User
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userID">
        /// The user id.
        /// </param>
        public static void Delete(this IRepository<User> repository, [NotNull] int userID)
        {
            repository.DbFunction.Scalar.user_delete(UserID: userID);
        }

        /// <summary>
        /// Remove User Avatar
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        public static void DeleteAvatar(this IRepository<User> repository, [NotNull] int userId) =>
            repository.UpdateOnly(
                () => 
                    new User
                    {
                        AvatarImage = null, Avatar = null, AvatarImageType = null
                    },
                u => u.ID == userId);

        /// <summary>
        /// Deletes all unapproved users older than x days
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardID">
        /// The board id.
        /// </param>
        /// <param name="days">
        /// The days.
        /// </param>
        public static void DeleteOld(this IRepository<User> repository, [NotNull] int boardID, [NotNull] int days)
        {
            repository.DbFunction.Scalar.user_deleteold(BoardID: boardID, Days: days, UTCTIMESTAMP: DateTime.UtcNow);
        }

        /// <summary>
        /// The watch mail list as data table.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="topicId">
        /// The topic id.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static DataTable WatchMailListAsDataTable(
            this IRepository<User> repository,
            [NotNull] int topicId,
            [NotNull] int userId) =>
            repository.DbFunction.GetData.mail_list(TopicID: topicId, UserID: userId, UTCTIMESTAMP: DateTime.UtcNow);

        /// <summary>
        /// Gets all Emails from User in Group
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="groupID">
        /// The group id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<string> GroupEmails(
            this IRepository<User> repository,
            [NotNull] int groupID)
        {
             var expression = OrmLiteConfig.DialectProvider.SqlExpression<User>();

             expression.Join<UserGroup>((u, b) => b.UserID == u.ID).Join<UserGroup, Group>((b, c) => c.ID == b.GroupID)
                 .Where<User, UserGroup, Group>(
                     (a, b, c) => b.GroupID == groupID && (c.Flags & 2) == 0 && a.Email != null)
                 .Select(u => new { u.Email });

            return repository.DbAccess.Execute(db => db.Connection.SqlList<string>(expression));
        }

        /// <summary>
        /// Gets the user id from the Provider User Key
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardID">
        /// The board id.
        /// </param>
        /// <param name="providerUserKey">
        /// The provider user key.
        /// </param>
        /// <returns>
        /// Returns the User Id
        /// </returns>
        public static int GetUserId(this IRepository<User> repository, int boardID, [NotNull] string providerUserKey)
        {
            var user = repository.GetSingle(u => u.BoardID == boardID && u.ProviderUserKey == providerUserKey);

            return user?.ID ?? 0;
        }

        /// <summary>
        /// Returns data about albums: allowed number of images and albums
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userID">
        /// The userID
        /// </param>
        /// <param name="boardID">
        /// The boardID
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static DataTable AlbumsDataAsDataTable(
            this IRepository<User> repository,
            [NotNull] int userID,
            [NotNull] int boardID) =>
            repository.DbFunction.GetData.user_getalbumsdata(BoardID: boardID, UserID: userID);

        /// <summary>
        /// Returns data about allowed signature tags and character limits
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userID">
        /// The userID
        /// </param>
        /// <param name="boardID">
        /// The boardID
        /// </param>
        /// <returns>
        /// Data Table
        /// </returns>
        public static DataRow SignatureDataAsDataRow(
            this IRepository<User> repository,
            [NotNull] int userID,
            [NotNull] int boardID) =>
            repository.DbFunction.GetAsDataTable(t => t.user_getsignaturedata(BoardID: boardID, UserID: userID))
                .GetFirstRow();

        /// <summary>
        /// Gets the Guest User
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardId">
        /// The board Id.
        /// </param>
        /// <returns>
        /// Returns the Guest User Id
        /// </returns>
        public static User GetGuestUser(this IRepository<User> repository, [NotNull] int boardId) =>
            repository.GetSingle(u => u.BoardID == boardId && u.IsGuest == true);

        /// <summary>
        /// To return a rather rarely updated active user data
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userID">
        /// The UserID. It is always should have a positive &gt; 0 value.
        /// </param>
        /// <param name="boardID">
        /// The board ID.
        /// </param>
        /// <param name="showPendingBuddies">
        /// The show Pending Buddies.
        /// </param>
        /// <param name="showUnreadPMs">
        /// The show Unread PMs.
        /// </param>
        /// <param name="showUserAlbums">
        /// The show User Albums.
        /// </param>
        /// <param name="styledNicks">
        /// If styles should be returned.
        /// </param>
        /// <returns>
        /// A DataRow, it should never return a null value.
        /// </returns>
        public static DataRow LazyDataRow(
            this IRepository<User> repository,
            [NotNull] int userID,
            [NotNull] int boardID,
            bool showPendingBuddies,
            bool showUnreadPMs,
            bool showUserAlbums,
            bool styledNicks)
        {
            var tries = 0;

            while (true)
            {
                try
                {
                    return repository.DbFunction.GetData.user_lazydata(
                        UserID: userID,
                        BoardID: boardID,
                        ShowPendingBuddies: showPendingBuddies,
                        ShowUnreadPMs: showUnreadPMs,
                        ShowUserAlbums: showUserAlbums,
                        ShowUserStyle: styledNicks).Rows[0];
                }
                catch (SqlException x)
                {
                    if (x.Number == 1205 && tries < 3)
                    {
                        // Transaction (Process ID XXX) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.
                    }
                    else
                    {
                        throw new ApplicationException($"Sql Exception with error number {x.Number} (Tries={tries}", x);
                    }
                }

                ++tries;
            }
        }

        /// <summary>
        /// Gets the User List
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardID">
        /// The board id.
        /// </param>
        /// <param name="userID">
        /// The user id.
        /// </param>
        /// <param name="approved">
        /// The approved.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static DataTable ListAsDataTable(
            this IRepository<User> repository,
            [NotNull] int boardID,
            [NotNull] int? userID,
            [NotNull] bool approved) =>
            repository.ListAsDataTable(boardID, userID, approved, null, null, false);

        /// <summary>
        /// The user_list.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardID">
        /// The board id.
        /// </param>
        /// <param name="userID">
        /// The user id.
        /// </param>
        /// <param name="approved">
        /// The approved.
        /// </param>
        /// <param name="groupID">
        /// The group id.
        /// </param>
        /// <param name="rankID">
        /// The rank id.
        /// </param>
        /// <param name="useStyledNicks">
        /// Return style info.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static DataTable ListAsDataTable(
            this IRepository<User> repository,
            [NotNull] int boardID,
            [NotNull] int? userID,
            [NotNull] bool approved,
            [NotNull] object groupID,
            [NotNull] object rankID,
            [CanBeNull] bool useStyledNicks) =>
            repository.DbFunction.GetData.user_list(
                BoardID: boardID,
                UserID: userID,
                Approved: approved,
                GroupID: groupID,
                RankID: rankID,
                StyledNicks: useStyledNicks,
                UTCTIMESTAMP: DateTime.UtcNow);

        /// <summary>
        /// The user_list20members.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardId">
        /// The board id.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="approved">
        /// The approved.
        /// </param>
        /// <param name="groupId">
        /// The group id.
        /// </param>
        /// <param name="rankId">
        /// The rank id.
        /// </param>
        /// <param name="useStyledNicks">
        /// Return style info.
        /// </param>
        /// <param name="literals">
        /// The literals.
        /// </param>
        /// <param name="exclude">
        /// The exclude.
        /// </param>
        /// <param name="beginsWith">
        /// The begins with.
        /// </param>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="pageSize">
        /// The page size.
        /// </param>
        /// <param name="sortName">
        /// The sort Name.
        /// </param>
        /// <param name="sortRank">
        /// The sort Rank.
        /// </param>
        /// <param name="sortJoined">
        /// The sort Joined.
        /// </param>
        /// <param name="sortPosts">
        /// The sort Posts.
        /// </param>
        /// <param name="sortLastVisit">
        /// The sort Last Visit.
        /// </param>
        /// <param name="numPosts">
        /// The number of Posts.
        /// </param>
        /// <param name="numPostCompare">
        /// The number of Post Compare.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static DataTable ListMembersAsDataTable(
            this IRepository<User> repository,
            [NotNull] int boardId,
            [NotNull] int? userId,
            [NotNull] bool approved,
            [NotNull] object groupId,
            [NotNull] object rankId,
            [NotNull] bool useStyledNicks,
            [NotNull] string literals,
            [NotNull] bool exclude,
            [NotNull] bool beginsWith,
            [NotNull] int pageIndex,
            [NotNull] int pageSize,
            [NotNull] int? sortName,
            [NotNull] int? sortRank,
            [NotNull] int? sortJoined,
            [NotNull] int? sortPosts,
            [NotNull] int? sortLastVisit,
            [NotNull] int numPosts,
            [NotNull] int numPostCompare) =>
            repository.DbFunction.GetData.user_listmembers(
                BoardID: boardId,
                UserID: userId,
                Approved: approved,
                GroupID: groupId,
                RankID: rankId,
                StyledNicks: useStyledNicks,
                Literals: literals,
                Exclude: exclude,
                BeginsWith: beginsWith,
                PageIndex: pageIndex,
                PageSize: pageSize,
                SortName: sortName,
                SortRank: sortRank,
                SortJoined: sortJoined,
                SortPosts: sortPosts,
                SortLastVisit: sortLastVisit,
                NumPosts: numPosts,
                NumPostsCompare: numPostCompare);

        /// <summary>
        /// Create NNTP User
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardID">
        /// The board id.
        /// </param>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="timeZone">
        /// The time Zone.
        /// </param>
        /// <returns>
        /// Returns the User ID of the created user.
        /// </returns>
        public static int CreateNntpUser(
            this IRepository<User> repository,
            [NotNull] int boardID,
            [NotNull] string userName,
            [NotNull] string email,
            int? timeZone) =>
            repository.DbFunction.Scalar.user_nntp(
                BoardID: boardID,
                UserName: userName,
                Email: email,
                TimeZone: timeZone,
                UTCTIMESTAMP: DateTime.UtcNow);

        /// <summary>
        /// The user_save.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userID">
        /// The user id.
        /// </param>
        /// <param name="boardID">
        /// The board id.
        /// </param>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="displayName">
        /// the display name.
        /// </param>
        /// <param name="email">
        /// The email.
        /// </param>
        /// <param name="timeZone">
        /// The time zone.
        /// </param>
        /// <param name="languageFile">
        /// The language file.
        /// </param>
        /// <param name="culture">
        /// the user culture
        /// </param>
        /// <param name="themeFile">
        /// The theme File.
        /// </param>
        /// <param name="hideUser">
        /// The hide User.
        /// </param>
        public static void Save(
            this IRepository<User> repository,
            [NotNull] int userID,
            [NotNull] int boardID,
            [NotNull] object userName,
            [NotNull] object displayName,
            [NotNull] object email,
            [NotNull] object timeZone,
            [NotNull] object languageFile,
            [NotNull] object culture,
            [NotNull] object themeFile,
            [NotNull] bool? hideUser) =>
            repository.DbFunction.Scalar.user_save(
                UserID: userID,
                BoardID: boardID,
                UserName: userName,
                DisplayName: displayName,
                Email: email,
                TimeZone: timeZone,
                LanguageFile: languageFile,
                Culture: culture,
                ThemeFile: themeFile,
                Approved: null,
                HideUser: hideUser,
                UTCTIMESTAMP: DateTime.UtcNow);

        /// <summary>
        /// Save the User Avatar
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userID">
        /// The user id.
        /// </param>
        /// <param name="avatarUrl">
        /// The avatar url.
        /// </param>
        /// <param name="stream">
        /// The stream.
        /// </param>
        /// <param name="avatarImageType">
        /// The avatar image type.
        /// </param>
        public static void SaveAvatar(
            this IRepository<User> repository,
            [NotNull] int userID,
            [CanBeNull] string avatarUrl,
            [CanBeNull] Stream stream,
            [CanBeNull] string avatarImageType)
        {
            if (avatarUrl == null)
            {
                byte[] data = null;

                if (stream != null)
                {
                    data = new byte[stream.Length];
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.Read(data, 0, stream.Length.ToType<int>());
                }

                repository.UpdateOnly(
                    () => new User { Avatar = avatarUrl, AvatarImage = data, AvatarImageType = avatarImageType },
                    u => u.ID == userID);
            }
            else
            {
                repository.UpdateOnly(
                    () => new User { Avatar = avatarUrl, AvatarImage = null, AvatarImageType = null },
                    u => u.ID == userID);
            }
        }

        /// <summary>
        /// Saves the notification type for a user
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="privateNotification">
        /// The pm Notification.
        /// </param>
        /// <param name="autoWatchTopics">
        /// The auto Watch Topics.
        /// </param>
        /// <param name="notificationType">
        /// The notification type.
        /// </param>
        /// <param name="dailyDigest">
        /// The daily Digest.
        /// </param>
        public static void SaveNotification(
            this IRepository<User> repository,
            [NotNull] int userId,
            [NotNull] bool privateNotification,
            [NotNull] bool autoWatchTopics,
            [CanBeNull] int? notificationType,
            [NotNull] bool dailyDigest)
        {
            repository.UpdateOnly(
                () => new User
                {
                    PMNotification = privateNotification,
                    AutoWatchTopics = autoWatchTopics,
                    NotificationType = notificationType,
                    DailyDigest = dailyDigest
                },
                u => u.ID == userId);
        }

        /// <summary>
        /// Sets the user roles
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardID">
        /// The board id.
        /// </param>
        /// <param name="providerUserKey">
        /// The provider user key.
        /// </param>
        /// <param name="role">
        /// The role.
        /// </param>
        public static void SetRole(
            this IRepository<User> repository,
            int boardID,
            [NotNull] string providerUserKey,
            [NotNull] string role)
        {
            repository.DbFunction.Scalar.user_setrole(BoardID: boardID, ProviderUserKey: providerUserKey, Role: role);
        }

        /// <summary>
        /// Gets the List of Administrators
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="useStyledNicks">
        /// The use styled nicks.
        /// </param>
        /// <param name="boardId">
        /// The board identifier.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<User> ListAdmins(
            this IRepository<User> repository,
            bool? useStyledNicks = null,
            int? boardId = null)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<User>();

            expression.Join<vaccess>((u, v) => v.UserID == u.ID).Where<vaccess, User>(
                (v, u) => u.BoardID == (boardId ?? repository.BoardID) && u.IsGuest == false && v.IsAdmin &&
                          v.ForumID == 0).OrderBy<User>(u => u.DisplayName);

            return repository.DbAccess.Execute(db => db.Connection.Select(expression));
        }

        /// <summary>
        /// Gets all Unapproved Users by Board Id.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardId">
        /// The board id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<User> UnApprovedUsers(this IRepository<User> repository, int? boardId = null)
        {
            return repository.Get(u => u.BoardID == (boardId ?? repository.BoardID) && !u.UserFlags.IsApproved);
        }

        /// <summary>
        /// Finds the user typed.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="filter">if set to <c>true</c> [filter].</param>
        /// <param name="boardId">The board identifier.</param>
        /// <param name="userName">Name of the user.</param>
        /// <param name="email">The email.</param>
        /// <param name="displayName">The display name.</param>
        /// <param name="notificationType">Type of the notification.</param>
        /// <param name="dailyDigest">The daily digest.</param>
        /// <returns>Returns List of found user(s).</returns>
        public static IList<User> FindUserTyped(
            this IRepository<User> repository,
            bool filter,
            int? boardId = null,
            string userName = null,
            string email = null,
            string displayName = null,
            int? notificationType = null,
            bool? dailyDigest = null)
        {
            return repository.SqlList(
                "user_find",
                new
                {
                    BoardID = boardId ?? repository.BoardID,
                    Filter = filter,
                    UserName = userName,
                    Email = email,
                    DisplayName = displayName,
                    NotificationType = notificationType,
                    DailyDigest = dailyDigest
                });
        }

        /// <summary>
        /// Gets the user signature.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>Returns the user points</returns>
        public static string GetSignature(this IRepository<User> repository, int userId)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            return repository.GetById(userId).Signature;
        }

        /// <summary>
        /// Saves the signature.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="signature">The signature.</param>
        public static void SaveSignature(
            this IRepository<User> repository,
            [NotNull] int userId,
            [CanBeNull] string signature)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            repository.UpdateOnly(() => new User { Signature = signature }, u => u.ID == userId);
        }

        /// <summary>
        /// Gets the user points.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>Returns the user points</returns>
        public static int GetPoints(this IRepository<User> repository, int userId)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            return repository.GetById(userId).Points;
        }

        /// <summary>
        /// Sets the points.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="points">The points.</param>
        public static void SetPoints(this IRepository<User> repository, [NotNull] int userId, [NotNull] int points)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            repository.UpdateOnly(() => new User { Points = points }, u => u.ID == userId);
        }

        /// <summary>
        /// Suspends the User
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="suspend">The suspend.</param>
        /// <param name="suspendReason">The suspend reason.</param>
        /// <param name="suspendBy">The suspend by.</param>
        public static void Suspend(
            this IRepository<User> repository,
            [NotNull] int userId,
            [NotNull] DateTime? suspend = null,
            string suspendReason = null,
            [NotNull] int suspendBy = 0)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            repository.UpdateOnly(
                () => new User { Suspended = suspend, SuspendedReason = suspendReason, SuspendedBy = suspendBy },
                u => u.ID == userId);
        }

        /// <summary>
        /// Updates Block Flags for the User.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        public static void UpdateBlockFlags(this IRepository<User> repository, int userId, int flags)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            repository.UpdateOnly(() => new User { BlockFlags = flags }, u => u.ID == userId);
        }

        /// <summary>
        /// The get board user.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="boardId">
        /// The board id.
        /// </param>
        /// <returns>
        /// The <see cref="Tuple"/>.
        /// </returns>
        public static Tuple<User, AspNetUsers, Rank, vaccess> GetBoardUser(
            this IRepository<User> repository,
            int userId,
            int? boardId = null)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<User>();

            expression.Join<vaccess>((u, v) => v.UserID == u.ID).Join<AspNetUsers>((u, a) => a.Id == u.ProviderUserKey)
                .Join<Rank>((u, r) => r.ID == u.RankID).Where<vaccess, User>(
                    (v, u) => u.ID == userId && u.BoardID == (boardId ?? repository.BoardID) && (u.Flags & 2) == 2 && v.ForumID == 0);

            return repository.DbAccess.Execute(db => db.Connection.SelectMulti<User, AspNetUsers, Rank, vaccess>(expression)).FirstOrDefault();
        }

        /// <summary>
        /// Get the list of recently logged in users.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="timeSinceLastLogin">
        /// The time since last login in minutes.
        /// </param>
        /// <param name="useStyledNicks">
        /// The use Styled Nicks.
        /// </param>
        /// <returns>
        /// The list of users in Data table format.
        /// </returns>
        public static DataTable GetRecentUsersAsDataTable(
            this IRepository<User> repository,
            [NotNull] int timeSinceLastLogin,
            [NotNull] bool useStyledNicks)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            var users = repository.DbFunction.GetData.recent_users(
                BoardID: repository.BoardID,
                TimeSinceLastLogin: timeSinceLastLogin,
                StyledNicks: useStyledNicks);

            if (!useStyledNicks)
            {
                return users;
            }

            BoardContext.Current.Get<IStyleTransform>().DecodeStyleByTable(users, true);

            return users;
        }

        /// <summary>
        /// Gets the forum moderators as data table.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="useStyledNicks">
        /// The use styled nicks.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static DataTable GetForumModeratorsAsDataTable(
            this IRepository<User> repository,
            [NotNull] bool useStyledNicks)
        {
            CodeContracts.VerifyNotNull(repository, "repository");

            return repository.DbFunction.GetAsDataTable(
                x => x.forum_moderators(repository.BoardID, useStyledNicks));
        }
    }
}