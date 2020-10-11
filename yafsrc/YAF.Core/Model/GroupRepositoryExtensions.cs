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
    using System.Collections.Generic;
    using System.Linq;

    using ServiceStack.OrmLite;

    using YAF.Core.Context;
    using YAF.Core.Extensions;
    using YAF.Types;
    using YAF.Types.EventProxies;
    using YAF.Types.Extensions;
    using YAF.Types.Flags;
    using YAF.Types.Interfaces;
    using YAF.Types.Interfaces.Data;
    using YAF.Types.Interfaces.Events;
    using YAF.Types.Models;

    /// <summary>
    /// The group repository extensions.
    /// </summary>
    public static class GroupRepositoryExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The list.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="groupId">
        /// The group id.
        /// </param>
        /// <param name="boardId">
        /// The board id.
        /// </param>
        /// <returns>
        /// The <see cref="IList"/>.
        /// </returns>
        public static IList<Group> List(this IRepository<Group> repository, int? groupId = null, int? boardId = null)
        {
            CodeContracts.VerifyNotNull(repository);

            return groupId.HasValue
                       ? repository.Get(g => g.BoardID == boardId && g.ID == groupId.Value)
                       : repository.Get(g => g.BoardID == boardId).OrderBy(o => o.SortOrder).ToList();
        }

        /// <summary>
        /// Gets All Roles by User indicating if User is Member or not
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardId">
        /// The board Id.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<dynamic> Member(
            this IRepository<Group> repository,
            [NotNull] int boardId,
            [NotNull] int userId)
        {
            CodeContracts.VerifyNotNull(repository);

            return repository.DbAccess.Execute(
               db =>
               {
                   var expression = OrmLiteConfig.DialectProvider.SqlExpression<Group>();

                   var countExpression = db.Connection.From<UserGroup>(db.Connection.TableAlias("x"));
                   countExpression.Where(
                       $@"x.{countExpression.Column<UserGroup>(x => x.UserID)}={userId}
                                    and x.{countExpression.Column<UserGroup>(x => x.GroupID)}={expression.Column<Group>(a => a.ID, true)}");
                   var countSql = countExpression.Select(Sql.Count("1"))
                       .ToSelectStatement();

                   expression.Where(a => a.BoardID == boardId)
                       .Select<Group>(
                           a => new
                           {
                               GroupID = a.ID,
                               a.Name,
                               Member = Sql.Custom($"({countSql})")
                           }).OrderBy<Group>(a => a.Name);

                   return db.Connection.Select<object>(expression);
               });
        }

        /// <summary>
        /// Save or Add new Group
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="groupId">
        /// The group Id.
        /// </param>
        /// <param name="boardId">
        /// The board Id.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <param name="accessMaskId">
        /// The access mask id.
        /// </param>
        /// <param name="messagesLimit">
        /// The messages Limit.
        /// </param>
        /// <param name="style">
        /// The style.
        /// </param>
        /// <param name="sortOrder">
        /// The sort order.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <param name="signatureChars">
        /// Defines number of allowed characters in user signature.
        /// </param>
        /// <param name="signatureBBCodes">
        /// The signature BBCodes.
        /// </param>
        /// <param name="signatureHTMLTags">
        /// The signature HTML Tags.
        /// </param>
        /// <param name="userAlbums">
        /// Defines allowed number of albums.
        /// </param>
        /// <param name="userAlbumImages">
        /// Defines number of images allowed for an album.
        /// </param>
        /// <returns>
        /// Returns the group Id
        /// </returns>
        public static int Save(
            this IRepository<Group> repository,
            [CanBeNull] int? groupId,
            [NotNull] int boardId,
            [NotNull] string name,
            [NotNull] GroupFlags flags,
            [NotNull] int accessMaskId,
            [NotNull] int messagesLimit,
            [CanBeNull] string style,
            [NotNull] short sortOrder,
            [CanBeNull] string description,
            [NotNull] int signatureChars,
            [CanBeNull] string signatureBBCodes,
            [CanBeNull] string signatureHTMLTags,
            [CanBeNull] int userAlbums,
            [CanBeNull] int userAlbumImages)
        {
            CodeContracts.VerifyNotNull(repository);

            if (groupId.HasValue)
            {
                repository.UpdateOnly(
                    () => new Group
                    {
                        Name = name,
                        Flags = flags.BitValue,
                        PMLimit = messagesLimit,
                        Style = style,
                        SortOrder = sortOrder,
                        Description = description,
                        UsrSigChars = signatureChars,
                        UsrSigBBCodes = signatureBBCodes,
                        UsrSigHTMLTags = signatureHTMLTags,
                        UsrAlbums = userAlbums,
                        UsrAlbumImages = userAlbumImages
                    },
                    g => g.ID == groupId.Value);

                repository.FireUpdated(groupId);
            }
            else
            {
                groupId = repository.Insert(
                    new Group
                    {
                        Name = name,
                        BoardID = boardId,
                        Flags = flags.BitValue,
                        PMLimit = messagesLimit,
                        Style = style,
                        SortOrder = sortOrder,
                        Description = description,
                        UsrSigChars = signatureChars,
                        UsrSigBBCodes = signatureBBCodes,
                        UsrSigHTMLTags = signatureHTMLTags,
                        UsrAlbums = userAlbums,
                        UsrAlbumImages = userAlbumImages
                    });

                repository.FireNew(groupId);

                BoardContext.Current.GetRepository<ForumAccess>().InitialAssignGroup(groupId.Value, accessMaskId);
            }

            if (style.IsSet())
            {
                // -- group styles override rank styles
                BoardContext.Current.Get<IRaiseEvent>().Raise(new UpdateUserStylesEvent(boardId));
            }
            
            return groupId.Value;
        }

        #endregion
    }
}