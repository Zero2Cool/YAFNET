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
    
    using ServiceStack.OrmLite;

    using YAF.Core.Extensions;
    using YAF.Types;
    using YAF.Types.Interfaces;
    using YAF.Types.Interfaces.Data;
    using YAF.Types.Models;

    /// <summary>
    /// The Buddy repository extensions.
    /// </summary>
    public static class BuddyRepositoryExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// Adds a buddy request. (Should be approved later by "ToUserID")
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="fromUserId">
        /// The from User Id.
        /// </param>
        /// <param name="toUserId">
        /// The to User Id.
        /// </param>
        /// <returns>
        /// The name of the second user + Whether this request is approved or not.
        /// </returns>
        [NotNull]
        public static bool AddRequest(
            this IRepository<Buddy> repository,
            [NotNull] int fromUserId,
            [NotNull] int toUserId)
        {
            CodeContracts.VerifyNotNull(repository);

            if (repository.Exists(x => x.FromUserID == fromUserId && x.ToUserID == toUserId))
            {
                return false;
            }

            if (repository.Exists(x => x.FromUserID == toUserId && x.ToUserID == fromUserId))
            {
                repository.Insert(
                    new Buddy
                    {
                        FromUserID = fromUserId, ToUserID = toUserId, Approved = true, Requested = DateTime.UtcNow
                    });

                repository.UpdateOnly(
                    () => new Buddy { Approved = true },
                    b => b.FromUserID == toUserId && b.ToUserID == fromUserId);

                return true;
            }

            repository.Insert(
                new Buddy
                {
                    FromUserID = fromUserId,
                    ToUserID = toUserId,
                    Approved = false,
                    Requested = DateTime.UtcNow
                });

            return false;
        }

        /// <summary>
        /// Approves a buddy request.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="fromUserId">
        /// The from User Id.
        /// </param>
        /// <param name="toUserId">
        /// The to User Id.
        /// </param>
        /// <param name="mutual">
        /// Should the requesting user (ToUserID) be added to FromUserID's buddy list too?
        /// </param>
        /// <returns>
        /// the name of the second user.
        /// </returns>
        [NotNull]
        public static bool ApproveRequest(
            this IRepository<Buddy> repository,
            [NotNull] int fromUserId,
            [NotNull] int toUserId,
            [NotNull] bool mutual)
        {
            CodeContracts.VerifyNotNull(repository);

            if (!repository.Exists(x => x.FromUserID == fromUserId && x.ToUserID == toUserId))
            {
                return false;
            }

            repository.UpdateOnly(
                () => new Buddy { Approved = true },
                b => b.FromUserID == fromUserId && b.ToUserID == toUserId);

            if (!mutual)
            {
                return true;
            }

            if (!repository.Exists(x => x.FromUserID == toUserId && x.ToUserID == fromUserId))
            {
                repository.Insert(
                    new Buddy
                    {
                        FromUserID = toUserId, ToUserID = fromUserId, Approved = true, Requested = DateTime.UtcNow
                    });
            }

            return true;
        }

        /// <summary>
        /// Denies a friend request.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="fromUserId">
        /// The from User Id.
        /// </param>
        /// <param name="toUserId">
        /// The to User Id.
        /// </param>
        [NotNull]
        public static void DenyRequest(
            this IRepository<Buddy> repository,
            [NotNull] int fromUserId,
            [NotNull] int toUserId)
        {
            CodeContracts.VerifyNotNull(repository);

            repository.Delete(b => b.FromUserID == fromUserId && b.ToUserID == toUserId);
        }

        /// <summary>
        /// Removes the "ToUserID" from "FromUserID"'s buddy list.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="fromUserId">
        /// The from user id.
        /// </param>
        /// <param name="toUserId">
        /// The to user id.
        /// </param>
        [NotNull]
        public static void Remove(
            this IRepository<Buddy> repository,
            [NotNull] int fromUserId,
            [NotNull] int toUserId)
        {
            CodeContracts.VerifyNotNull(repository);

            repository.Delete(x => x.FromUserID == fromUserId && x.ToUserID == toUserId);
            
            repository.FireDeleted();
        }

        /// <summary>
        /// Gets all the buddies of a certain user.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="fromUserId">From user identifier.</param>
        /// <returns>
        /// The <see cref="List" /> containing the buddy list.
        /// </returns>
        public static List<dynamic> ListAll(this IRepository<Buddy> repository, [NotNull] int fromUserId)
        {
            CodeContracts.VerifyNotNull(repository);

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<User>();

            expression.Join<Rank>((u, r) => r.ID == u.RankID)
                .Join<Buddy>((u, b) => b.ToUserID == u.ID && b.FromUserID == fromUserId && u.IsApproved == true)
                .Select<User, Rank, Buddy>(
                    (a, b, c) => new
                    {
                        UserID = a.ID,
                        a.BoardID,
                        a.Name,
                        a.DisplayName,
                        a.Joined,
                        a.NumPosts,
                        RankName = b.Name,
                        c.Approved,
                        c.FromUserID,
                        c.Requested,
                        a.UserStyle,
                        a.Suspended
                    });

            var expression2 = OrmLiteConfig.DialectProvider.SqlExpression<User>();

            expression2.Join<Rank>((u, r) => r.ID == u.RankID)
                .Join<Buddy>((u, b) => b.ToUserID == fromUserId && b.FromUserID == u.ID && u.IsApproved == true)
                .OrderBy(u => u.Name).Select<User, Rank, Buddy>(
                    (a, b, c) => new
                    {
                        UserID = fromUserId,
                        a.BoardID,
                        a.Name,
                        a.DisplayName,
                        a.Joined,
                        a.NumPosts,
                        RankName = b.Name,
                        c.Approved,
                        c.FromUserID,
                        c.Requested,
                        a.UserStyle,
                        a.Suspended
                    });

            return repository.DbAccess.Execute(
                db => db.Connection.Select<dynamic>(
                    $"{expression.ToSelectStatement()} UNION {expression2.ToSelectStatement()}"));
        }

        #endregion
    }
}