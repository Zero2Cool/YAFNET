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
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    using ServiceStack.OrmLite;

    using YAF.Core.Context;
    using YAF.Core.Extensions;
    using YAF.Types;
    using YAF.Types.Constants;
    using YAF.Types.EventProxies;
    using YAF.Types.Extensions;
    using YAF.Types.Flags;
    using YAF.Types.Interfaces;
    using YAF.Types.Interfaces.Data;
    using YAF.Types.Interfaces.Events;
    using YAF.Types.Models;
    using YAF.Types.Objects.Model;

    #endregion

    /// <summary>
    ///     The Topic repository extensions.
    /// </summary>
    public static class TopicRepositoryExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// Get the Topic From Message.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageId">
        /// The message id.
        /// </param>
        /// <returns>
        /// The <see cref="Topic"/>.
        /// </returns>
        public static Topic GetTopicFromMessage(this IRepository<Topic> repository, [NotNull] int messageId)
        {
            CodeContracts.VerifyNotNull(repository);

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Message>();

            expression.Join<Topic>((m, t) => t.ID == m.TopicID).Where<Message>(m => m.ID == messageId);

            return repository.DbAccess.Execute(db => db.Connection.Select<Topic>(expression)).FirstOrDefault();
        }

        /// <summary>
        /// Get the Topic with Message.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageId">
        /// The message id.
        /// </param>
        /// <returns>
        /// The <see cref="Tuple"/>.
        /// </returns>
        public static Tuple<Message, Topic> GetTopicWithMessage(
            this IRepository<Topic> repository,
            [NotNull] int messageId)
        {
            CodeContracts.VerifyNotNull(repository);

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Message>();

            expression.Join<Topic>((m, t) => t.ID == m.TopicID).Where<Message>(m => m.ID == messageId);

            return repository.DbAccess.Execute(db => db.Connection.SelectMulti<Message, Topic>(expression))
                .FirstOrDefault();
        }

        /// <summary>
        /// Attach a Poll to a Topic
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="topicId">
        /// The topic id.
        /// </param>
        /// <param name="pollId">
        /// The poll id.
        /// </param>
        public static void AttachPoll(this IRepository<Topic> repository, [NotNull] int topicId, [NotNull] int pollId)
        {
            CodeContracts.VerifyNotNull(repository);

            repository.UpdateOnly(() => new Topic { PollID = pollId }, t => t.ID == topicId);
        }

        /// <summary>
        /// Get the Topic Name From Message.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageId">
        /// The message id.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string GetNameFromMessage(this IRepository<Topic> repository, [NotNull] int messageId)
        {
            CodeContracts.VerifyNotNull(repository);

            string topicName;

            try
            {
                topicName = repository.GetTopicFromMessage(messageId).TopicName;
            }
            catch (Exception)
            {
                topicName = string.Empty;
            }

            return topicName;
        }

        /// <summary>
        /// Sets the answer message.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="topicId">The topic identifier.</param>
        /// <param name="messageId">The message identifier.</param>
        public static void SetAnswerMessage(
            this IRepository<Topic> repository,
            [NotNull] int topicId,
            [NotNull] int messageId)
        {
            CodeContracts.VerifyNotNull(repository);

            repository.UpdateOnly(() => new Topic { AnswerMessageId = messageId }, t => t.ID == topicId);
        }

        /// <summary>
        /// Removes answer message.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="topicId">The topic identifier.</param>
        public static void RemoveAnswerMessage(this IRepository<Topic> repository, [NotNull] int topicId)
        {
            CodeContracts.VerifyNotNull(repository);

            repository.UpdateOnly(() => new Topic { AnswerMessageId = null }, t => t.ID == topicId);
        }

        /// <summary>
        /// Gets the answer message.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="topicId">The topic identifier.</param>
        /// <returns>Returns the Answer Message identifier</returns>
        public static int? GetAnswerMessage(this IRepository<Topic> repository, [NotNull] int topicId)
        {
            CodeContracts.VerifyNotNull(repository);

            var topic = repository.GetById(topicId);

            return topic?.AnswerMessageId;
        }

        /// <summary>
        /// Locks/Unlock the topic.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="topicId">The topic identifier.</param>
        /// <param name="flags">The topic flags.</param>
        public static void Lock(this IRepository<Topic> repository, [NotNull] int topicId, [NotNull] int flags)
        {
            CodeContracts.VerifyNotNull(repository);

            repository.UpdateOnly(() => new Topic { Flags = flags }, t => t.ID == topicId);
        }

        /// <summary>
        /// The unanswered as data table.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="pageUserId">
        /// The page user id.
        /// </param>
        /// <param name="sinceDate">
        /// The since Date.
        /// </param>
        /// <param name="toDate">
        /// The to Date.
        /// </param>
        /// <param name="pageIndex">
        /// The page Index.
        /// </param>
        /// <param name="pageSize">
        /// The page Size.
        /// </param>
        /// <param name="findLastRead">
        /// Indicates if the Table should contain the last Access Date
        /// </param>
        /// <returns>
        /// Returns the List with the Topics Unanswered
        /// </returns>
        public static List<PagedTopic> ListUnansweredPaged(
            this IRepository<Topic> repository,
            [NotNull] int pageUserId,
            [NotNull] DateTime sinceDate,
            [NotNull] DateTime toDate,
            [NotNull] int pageIndex,
            [NotNull] int pageSize,
            [NotNull] bool findLastRead)
        {
            CodeContracts.VerifyNotNull(repository);

            return repository.ListPaged(
                pageUserId,
                pageIndex,
                pageSize,
                false,
                findLastRead,
                (t, x, c) => x.UserID == pageUserId && x.ReadAccess && t.IsDeleted == false && t.TopicMovedID == null &&
                             t.LastPosted != null && t.LastPosted > sinceDate && t.LastPosted < toDate &&
                             t.NumPosts == 1);
        }

        /// <summary>
        /// Returns Topics Unread by a user
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="pageUserId">
        /// The page user id.
        /// </param>
        /// <param name="sinceDate">
        /// The since Date.
        /// </param>
        /// <param name="toDate">
        /// The to Date.
        /// </param>
        /// <param name="pageIndex">
        /// The page Index.
        /// </param>
        /// <param name="pageSize">
        /// The page Size.
        /// </param>
        /// <param name="findLastRead">
        /// Indicates if the Table should contain the last Access Date
        /// </param>
        /// <returns>
        /// Returns the List with the Topics Unread be a PageUserId
        /// </returns>
        public static List<PagedTopic> ListActivePaged(
            this IRepository<Topic> repository,
            [NotNull] int pageUserId,
            [NotNull] DateTime sinceDate,
            [NotNull] DateTime toDate,
            [NotNull] int pageIndex,
            [NotNull] int pageSize,
            [NotNull] bool findLastRead)
        {
            CodeContracts.VerifyNotNull(repository);

            return repository.ListPaged(
                pageUserId,
                pageIndex,
                pageSize,
                false,
                findLastRead,
                (t, x, c) => x.UserID == pageUserId && x.ReadAccess &&
                             t.IsDeleted == false && t.TopicMovedID == null && t.LastPosted != null && t.LastPosted > sinceDate && t.LastPosted < toDate);
        }

        /// <summary>
        /// Returns Topics Unread by a user
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="pageUserId">
        /// The page user id.
        /// </param>
        /// <param name="sinceDate">
        /// The since Date.
        /// </param>
        /// <param name="toDate">
        /// The to Date.
        /// </param>
        /// <param name="pageIndex">
        /// The page Index.
        /// </param>
        /// <param name="pageSize">
        /// The page Size.
        /// </param>
        /// <param name="findLastRead">
        /// Indicates if the Table should contain the last Access Date
        /// </param>
        /// <returns>
        /// Returns the List with the Topics Unread be a PageUserId
        /// </returns>
        public static List<PagedTopic> ListUnreadPaged(
            this IRepository<Topic> repository,
            [NotNull] int pageUserId,
            [NotNull] DateTime sinceDate,
            [NotNull] DateTime toDate,
            [NotNull] int pageIndex,
            [NotNull] int pageSize,
            [CanBeNull] bool findLastRead = false)
        {
            CodeContracts.VerifyNotNull(repository);

            return repository.ListPaged(
                pageUserId,
                pageIndex,
                pageSize,
                false,
                findLastRead,
                (t, x, c) => x.UserID == pageUserId && x.ReadAccess &&
                             t.IsDeleted == false && t.TopicMovedID == null && t.LastPosted != null && t.LastPosted > sinceDate && t.LastPosted < toDate);
        }

        /// <summary>
        /// Gets all topics where the page User id has posted
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="pageUserId">
        /// The page user id.
        /// </param>
        /// <param name="sinceDate">
        /// The since Date.
        /// </param>
        /// <param name="toDate">
        /// The to Date.
        /// </param>
        /// <param name="pageIndex">
        /// The page Index.
        /// </param>
        /// <param name="pageSize">
        /// The page Size.
        /// </param>
        /// <param name="findLastRead">
        /// Indicates if the Table should contain the last Access Date
        /// </param>
        /// <returns>
        /// Returns the List with the User Topics
        /// </returns>
        public static List<PagedTopic> ListByUserPaged(
            this IRepository<Topic> repository,
            [NotNull] int pageUserId,
            [NotNull] DateTime sinceDate,
            [NotNull] DateTime toDate,
            [NotNull] int pageIndex,
            [NotNull] int pageSize,
            [CanBeNull] bool findLastRead = false)
        {
            CodeContracts.VerifyNotNull(repository);

            return repository.ListPaged(
                pageUserId,
                pageIndex,
                pageSize,
                false,
                findLastRead,
                (t, x, c) => x.UserID == pageUserId && x.ReadAccess && t.IsDeleted == false && t.TopicMovedID == null &&
                             t.LastPosted != null && t.LastPosted > sinceDate && t.LastPosted < toDate &&
                             t.UserID == pageUserId);
        }

        /// <summary>
        /// Create New Topic By Message.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageId">
        /// The message Id.
        /// </param>
        /// <param name="forumId">
        /// The forum id.
        /// </param>
        /// <param name="newTopicSubject">
        /// The new topic subject.
        /// </param>
        /// <returns>
        /// Returns the new Topic ID
        /// </returns>
        public static long CreateByMessage(
            this IRepository<Topic> repository,
            [NotNull] int messageId,
            [NotNull] int forumId,
            [NotNull] string newTopicSubject)
        {
            CodeContracts.VerifyNotNull(repository, nameof(repository));

            var message = BoardContext.Current.GetRepository<Message>().GetById(messageId);

            var topic = new Topic
            {
                ForumID = forumId,
                TopicName = newTopicSubject,
                UserID = message.UserID,
                Posted = message.Posted,
                Views = 0,
                Priority = 0,
                PollID = null,
                UserName = null,
                NumPosts = 0
            };

            return repository.Insert(topic);
        }

        /// <summary>
        /// Get the Latest Topics for the RSS Feed.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardId">
        /// The board Id.
        /// </param>
        /// <param name="numOfPostsToRetrieve">
        /// The number of posts to retrieve.
        /// </param>
        /// <param name="pageUserId">
        /// The page UserId id.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Tuple<Message, Topic, User>> RssLatest(
            this IRepository<Topic> repository,
            [NotNull] int boardId,
            [NotNull] int numOfPostsToRetrieve,
            [NotNull] int pageUserId)
        {
            CodeContracts.VerifyNotNull(repository);

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Message>();

            expression.Join<Topic>((m, t) => m.ID == t.LastMessageID).Join<Topic, User>((t, u) => u.ID == t.LastUserID)
                .Join<Topic, Forum>((c, d) => d.ID == c.ForumID).Join<Forum, Category>((d, e) => e.ID == d.CategoryID)
                .Join<Category, ActiveAccess>((d, x) => x.ForumID == d.ID)
                .Where<Topic, Message, ActiveAccess, Category>(
                    (topic, message, x, e) => e.BoardID == boardId && topic.TopicMovedID == null &&
                                              x.UserID == pageUserId && x.ReadAccess && topic.IsDeleted == false &&
                                              message.IsDeleted == false && topic.LastPosted != null)
                .OrderByDescending<Message>(x => x.Posted).Take(numOfPostsToRetrieve);

            return repository.DbAccess.Execute(db => db.Connection.SelectMulti<Message, Topic, User>(expression));
        }

        /// <summary>
        /// Gets all Topics for an RSS Feed of specified forum id.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="forumId">
        /// The forum id.
        /// </param>
        /// <param name="pageUserId">
        /// The page User Id.
        /// </param>
        /// <param name="topicLimit">
        /// The topic limit.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<dynamic> RssList(
            this IRepository<Topic> repository,
            int forumId,
            int pageUserId,
            int topicLimit)
        {
            CodeContracts.VerifyNotNull(repository);

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Topic>();

            return repository.DbAccess.Execute(
                db =>
                {
                    expression.Join<Forum>((t, f) => f.ID == t.ForumID).Join<Message>((t, m) => m.ID == t.LastMessageID)
                        .Join<Forum, Category>((f, c) => c.ID == f.CategoryID)
                        .Join<Forum, ActiveAccess>((f, x) => x.ForumID == f.ID);

                    expression.Where<Topic, Forum, ActiveAccess, Category>(
                        (topic, f, x, c) => c.BoardID == repository.BoardID && topic.TopicMovedID == null &&
                                            x.UserID == pageUserId && x.ReadAccess && topic.IsDeleted == false &&
                                            topic.LastPosted != null && topic.NumPosts > 0);

                    expression.OrderByDescending<Topic>(t => t.LastPosted);

                    expression.Take(topicLimit).Select<Topic, Message, Forum>(
                        (t, m, f) => new
                        {
                            Topic = t.TopicName,
                            TopicID = t.ID,
                            f.Name,
                            LastPosted = t.LastPosted != null ? t.LastPosted : t.Posted,
                            LastUserID = t.LastUserID != null ? t.LastUserID : t.UserID,
                            t.LastMessageID,
                            t.LastMessageFlags,
                            LastMessage = m.MessageText
                        });

                    return db.Connection.Select<object>(expression);
                });
        }

        /// <summary>
        /// Get the Latest Topics
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardId">
        /// The board Id.
        /// </param>
        /// <param name="categoryId">
        /// The category Id.
        /// </param>
        /// <param name="numOfPostsToRetrieve">
        /// The number of posts to retrieve.
        /// </param>
        /// <param name="pageUserId">
        /// The page UserId id.
        /// </param>
        /// <param name="showNoCountPosts">
        /// The show No Count Posts.
        /// </param>
        /// <param name="findLastRead">
        /// Indicates if the Table should contain the last Access Date
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<dynamic> Latest(
            this IRepository<Topic> repository,
            [NotNull] int boardId,
            [NotNull] int categoryId,
            [NotNull] int numOfPostsToRetrieve,
            [NotNull] int pageUserId,
            bool showNoCountPosts,
            [NotNull] bool findLastRead)
        {
            CodeContracts.VerifyNotNull(repository);

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Topic>();

            return repository.DbAccess.Execute(
                db =>
                {
                    expression.Join<Message>((t, m) => m.ID == t.LastMessageID).Join<Forum>((t, f) => f.ID == t.ForumID)
                        .Join<User>((t, u) => u.ID == t.UserID)
                        .Join<User>(
                            (t, lastUser) => Sql.TableAlias(lastUser.ID, "lastUser") == t.LastUserID,
                            db.Connection.TableAlias("lastUser")).Join<Forum, Category>((f, c) => c.ID == f.CategoryID)
                        .Join<Forum, ActiveAccess>((f, x) => x.ForumID == f.ID);

                    if (showNoCountPosts)
                    {
                        expression.Where<Topic, Forum, ActiveAccess, Category>(
                            (topic, f, x, c) => c.BoardID == boardId && topic.TopicMovedID == null &&
                                                x.UserID == pageUserId && x.ReadAccess && topic.IsDeleted == false &&
                                                topic.LastPosted != null && (f.Flags & 4) != 4);
                    }
                    else
                    {
                        expression.Where<Topic, Forum, ActiveAccess, Category>(
                            (topic, f, x, c) => c.BoardID == boardId && topic.TopicMovedID == null &&
                                                x.UserID == pageUserId && x.ReadAccess && topic.IsDeleted == false &&
                                                topic.LastPosted != null && (f.Flags & 4) != -1);
                    }

                    if (categoryId > 0)
                    {
                        expression.And<Category>(c => c.ID == categoryId);
                    }

                    expression.OrderByDescending<Topic>(t => t.LastPosted);

                    if (findLastRead)
                    {
                        var forumReadTrackExpression =
                            db.Connection.From<ForumReadTracking>(db.Connection.TableAlias("x"));
                        forumReadTrackExpression.Where(
                            $@"x.{forumReadTrackExpression.Column<ForumReadTracking>(x => x.ForumID)}={expression.Column<Forum>(f => f.ID, true)}
                                        and x.{forumReadTrackExpression.Column<ForumReadTracking>(x => x.UserID)}={pageUserId}");
                        var forumReadTrackSql = forumReadTrackExpression
                            .Select($"{forumReadTrackExpression.Column<ForumReadTracking>(x => x.LastAccessDate)}")
                            .Limit(1).ToSelectStatement();

                        var topicReadTrackExpression =
                            db.Connection.From<TopicReadTracking>(db.Connection.TableAlias("x"));
                        topicReadTrackExpression.Where(
                            $@"x.{topicReadTrackExpression.Column<TopicReadTracking>(x => x.TopicID)}={expression.Column<Topic>(t => t.ID, true)}
                                       and x.{topicReadTrackExpression.Column<TopicReadTracking>(x => x.UserID)}={pageUserId}");
                        var topicReadTrackSql = topicReadTrackExpression
                            .Select($"{topicReadTrackExpression.Column<TopicReadTracking>(x => x.LastAccessDate)}")
                            .Limit(1).ToSelectStatement();

                        expression.Take(numOfPostsToRetrieve).Select<Topic, Message, Forum, User, User>(
                            (t, m, f, u, lastUser) => new
                            {
                                t.LastPosted,
                                t.ForumID,
                                Forum = f.Name,
                                t.TopicName,
                                t.Status,
                                t.Styles,
                                TopicID = t.ID,
                                t.TopicMovedID,
                                t.UserID,
                                UserName = t.UserName != null ? t.UserName : u.Name,
                                UserDisplayName = t.UserDisplayName != null ? t.UserDisplayName : u.Name,
                                t.LastMessageID,
                                t.LastMessageFlags,
                                t.LastUserID,
                                t.NumPosts,
                                t.Views,
                                t.Posted,
                                LastMessage = m.MessageText,
                                LastUserName = lastUser.Name,
                                LastUserDisplayName = lastUser.DisplayName,
                                LastUserStyle = lastUser.UserStyle,
                                LastUserSuspended = lastUser.Suspended,
                                LastForumAccess = Sql.Custom($"({forumReadTrackSql})"),
                                LastTopicAccess = Sql.Custom($"({topicReadTrackSql})")
                            });
                    }
                    else
                    {
                        expression.Take(numOfPostsToRetrieve).Select<Topic, Message, Forum, User, User>(
                            (t, m, f, u, lastUser) => new
                            {
                                t.LastPosted,
                                t.ForumID,
                                Forum = f.Name,
                                t.TopicName,
                                t.Status,
                                t.Styles,
                                TopicID = t.ID,
                                t.TopicMovedID,
                                t.UserID,
                                UserName = t.UserName != null ? t.UserName : u.Name,
                                UserDisplayName = t.UserDisplayName != null ? t.UserDisplayName : u.Name,
                                t.LastMessageID,
                                t.LastMessageFlags,
                                t.LastUserID,
                                t.NumPosts,
                                t.Views,
                                t.Posted,
                                LastMessage = m.MessageText,
                                LastUserName = u.Name,
                                LastUserDisplayName = u.DisplayName,
                                LastForumAccess = (DateTime?)null,
                                LastTopicAccess = (DateTime?)null
                            });
                    }

                    return db.Connection.Select<object>(expression);
                });
        }

        /// <summary>
        /// Gets the paged Topic List
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="forumId">
        /// The forum Id.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="sinceDate">
        /// The since Date.
        /// </param>
        /// <param name="toDate">
        /// The to Date.
        /// </param>
        /// <param name="pageIndex">
        /// The page Index.
        /// </param>
        /// <param name="pageSize">
        /// The page Size.
        /// </param>
        /// <param name="showMoved">
        /// Show Moved topics.
        /// </param>
        /// <param name="findLastRead">
        /// Indicates if the list should contain the last Access Date
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<PagedTopic> ListPaged(
            this IRepository<Topic> repository,
            [NotNull] int forumId,
            [NotNull] int userId,
            [CanBeNull] DateTime? sinceDate,
            [CanBeNull] DateTime? toDate,
            [NotNull] int pageIndex,
            [NotNull] int pageSize,
            [NotNull] bool showMoved,
            [NotNull] bool findLastRead)
        {
            CodeContracts.VerifyNotNull(repository);

            return repository.ListPaged(
                userId,
                pageIndex,
                pageSize,
                showMoved,
                findLastRead,
                (t, x, c) => t.ForumID == forumId &&
                             (t.Priority == 1 || t.Priority <= 0 && t.LastPosted >= sinceDate) &&
                             t.IsDeleted == false && (t.TopicMovedID != null || t.NumPosts > 0 && x.UserID == userId && x.ReadAccess));
        }

        /// <summary>
        /// Gets the paged Topic List
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="pageIndex">
        /// The page Index.
        /// </param>
        /// <param name="pageSize">
        /// The page Size.
        /// </param>
        /// <param name="showMoved">
        /// Show Moved topics.
        /// </param>
        /// <param name="findLastRead">
        /// Indicates if the list should contain the last Access Date
        /// </param>
        /// <param name="whereCriteria">
        /// The where Criteria.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<PagedTopic> ListPaged(
            this IRepository<Topic> repository,
            [NotNull] int userId,
            [NotNull] int pageIndex,
            [NotNull] int pageSize,
            [NotNull] bool showMoved,
            [NotNull] bool findLastRead,
            Expression<Func<Topic, ActiveAccess, Category, bool>> whereCriteria)
        {
            CodeContracts.VerifyNotNull(repository);

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Topic>();

            return repository.DbAccess.Execute(
                db =>
                {
                    expression.Join<User>((t, u) => u.ID == t.UserID).Join<Forum>((t, f) => f.ID == t.ForumID)
                        .Join<Forum, ActiveAccess>((f, x) => x.ForumID == f.ID)
                        .Join<Forum, Category>((f, c) => c.ID == f.CategoryID);

                    expression.Where(whereCriteria);

                    // -- count total
                    var countTotalExpression = db.Connection.From<Topic>();

                    countTotalExpression.Where(whereCriteria);

                    expression.OrderByDescending<Topic>(t => t.LastPosted).Page(pageIndex + 1, pageSize);

                    var countTotalSql = countTotalExpression
                        .Select(Sql.Count($"{countTotalExpression.Column<Topic>(x => x.ID)}")).ToSelectStatement();

                    // -- Count favorite
                    var countFavoriteExpression = db.Connection.From<FavoriteTopic>(db.Connection.TableAlias("f"));
                    countFavoriteExpression.Where(
                        $@"f.{countFavoriteExpression.Column<FavoriteTopic>(f => f.TopicID)}=
                                    IsNull({expression.Column<Topic>(x => x.TopicMovedID, true)},{expression.Column<Topic>(x => x.ID, true)})");
                    var countFavoriteSql = countFavoriteExpression.Select(Sql.Count("1")).ToSelectStatement();

                    // -- count deleted posts
                    var countDeletedExpression = db.Connection.From<Message>(db.Connection.TableAlias("mes"));
                    countDeletedExpression.Where(
                        $@"mes.{countDeletedExpression.Column<Message>(x => x.TopicID)}={expression.Column<Topic>(x => x.ID, true)}
                                    and mes.{countDeletedExpression.Column<Message>(x => x.IsDeleted)}=1
                                    and mes.{countDeletedExpression.Column<Message>(x => x.UserID)}={userId}");
                    var countDeletedSql = countDeletedExpression.Select(Sql.Count("1")).ToSelectStatement();

                    var lastTopicAccessSql = "NULL";
                    var lastForumAccessSql = "NULL";

                    if (findLastRead)
                    {
                        var topicAccessExpression =
                            db.Connection.From<TopicReadTracking>(db.Connection.TableAlias("y"));
                        topicAccessExpression.Where(
                            $@"y.{topicAccessExpression.Column<TopicReadTracking>(y => y.TopicID)}={expression.Column<Topic>(x => x.ID, true)}
                                    and y.{topicAccessExpression.Column<TopicReadTracking>(y => y.UserID)}={userId}");
                        lastTopicAccessSql = topicAccessExpression.Select(
                                $"top 1 {topicAccessExpression.Column<TopicReadTracking>(x => x.LastAccessDate)}")
                            .ToSelectStatement();

                        var forumAccessExpression =
                            db.Connection.From<ForumReadTracking>(db.Connection.TableAlias("x"));
                        forumAccessExpression.Where(
                            $@"x.{forumAccessExpression.Column<ForumReadTracking>(x => x.ForumID)}={expression.Column<Topic>(x => x.ForumID, true)}
                                    and x.{forumAccessExpression.Column<ForumReadTracking>(x => x.UserID)}={userId}");
                        lastForumAccessSql = forumAccessExpression.Select(
                                $"top 1 {forumAccessExpression.Column<ForumReadTracking>(x => x.LastAccessDate)}")
                            .ToSelectStatement();
                    }

                    // -- last user
                    var lastUserNameExpression = db.Connection.From<User>(db.Connection.TableAlias("usr"));
                    lastUserNameExpression.Where(
                        $@"usr.{lastUserNameExpression.Column<User>(x => x.ID)}=
                                   {expression.Column<Topic>(x => x.LastUserID, true)}");
                    var lastUserNameSql = lastUserNameExpression.Select(
                        $"top 1 {lastUserNameExpression.Column<User>(x => x.Name)}").ToSelectStatement();

                    var lastUserDisplayNameSql = lastUserNameExpression.Select(
                        $"top 1 {lastUserNameExpression.Column<User>(x => x.DisplayName)}").ToSelectStatement();

                    var lastUserSuspendedSql = lastUserNameExpression.Select(
                        $"top 1 {lastUserNameExpression.Column<User>(x => x.Suspended)}").ToSelectStatement();

                    var lastUserStyleSql = lastUserNameExpression.Select(
                        $"top 1 {lastUserNameExpression.Column<User>(x => x.UserStyle)}").ToSelectStatement();

                    // -- first message
                    var firstMessageExpression = db.Connection.From<Message>(db.Connection.TableAlias("fm"));
                    firstMessageExpression.Where(
                        $@"fm.{firstMessageExpression.Column<Message>(x => x.TopicID)}=
                                   IsNull({expression.Column<Topic>(x => x.TopicMovedID, true)},{expression.Column<Topic>(x => x.ID, true)})
                                   and fm.{firstMessageExpression.Column<Message>(x => x.Position)} = 0");
                    var firstMessageSql = firstMessageExpression.Select(
                        $"top 1 {firstMessageExpression.Column<Message>(x => x.MessageText)}").ToSelectStatement();

                    expression.Select<Topic, User, Forum>(
                        (c, b, d) => new
                        {
                            ForumID = d.ID,
                            TopicID = c.ID,
                            c.Posted,
                            LinkTopicID = c.TopicMovedID != null ? c.TopicMovedID : c.ID,
                            c.TopicMovedID,
                            FavoriteCount = Sql.Custom($"({countFavoriteSql})"),
                            Subject = c.TopicName,
                            c.Description,
                            c.Status,
                            c.Styles,
                            c.UserID,
                            Starter = c.UserName != null ? c.UserName : b.Name,
                            StarterDisplay = c.UserDisplayName != null ? c.UserDisplayName : b.DisplayName,
                            Replies = c.NumPosts - 1,
                            NumPostsDeleted = Sql.Custom($"({countDeletedSql})"),
                            c.Views,
                            c.LastPosted,
                            c.LastUserID,
                            LastUserName = Sql.Custom($"({lastUserNameSql})"),
                            LastUserDisplayName = Sql.Custom($"({lastUserDisplayNameSql})"),
                            LastUserSuspended = Sql.Custom($"({lastUserSuspendedSql})"),
                            LastUserStyle = Sql.Custom($"({lastUserStyleSql})"),
                            c.LastMessageFlags,
                            c.LastMessageID,
                            LastTopicID = c.ID,
                            c.LinkDate,
                            TopicFlags = c.Flags,
                            c.Priority,
                            c.PollID,
                            ForumFlags = d.Flags,
                            FirstMessage = Sql.Custom($"({firstMessageSql})"),
                            StarterStyle = b.UserStyle,
                            StarterSuspended = b.Suspended,
                            LastForumAccess = Sql.Custom($"({lastForumAccessSql})"),
                            LastTopicAccess = Sql.Custom($"({lastTopicAccessSql})"),
                            c.TopicImage,
                            TotalRows = Sql.Custom($"({countTotalSql})")
                        });

                    return db.Connection.Select<PagedTopic>(expression);
                });
        }

        /// <summary>
        /// Get the Paged Announcements Topics
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="forumId">
        /// The forum Id.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="toDate">
        /// The to Date.
        /// </param>
        /// <param name="pageIndex">
        /// The page Index.
        /// </param>
        /// <param name="pageSize">
        /// The page Size.
        /// </param>
        /// <param name="showMoved">
        /// The show Moved.
        /// </param>
        /// <param name="findLastRead">
        /// Indicates if the list should Contain the last Access Date
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<PagedTopic> ListAnnouncementsPaged(
            this IRepository<Topic> repository,
            [NotNull] int forumId,
            [NotNull] int userId,
            [NotNull] DateTime toDate,
            [NotNull] int pageIndex,
            [NotNull] int pageSize,
            [NotNull] bool showMoved,
            [CanBeNull] bool findLastRead)
        {
            CodeContracts.VerifyNotNull(repository);

            return repository.ListPaged(
                userId,
                pageIndex,
                pageSize,
                showMoved,
                findLastRead,
                (t, x, c) => t.ForumID == forumId && t.Priority == 2 && t.IsDeleted == false &&
                             (t.TopicMovedID != null || t.NumPosts > 0) && x.UserID == userId && x.ReadAccess);
        }

        /// <summary>
        /// The topic_save.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="forumId">
        /// The forum Id.
        /// </param>
        /// <param name="subject">
        /// The subject.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="styles">
        /// The styles.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <param name="priority">
        /// The priority.
        /// </param>
        /// <param name="userName">
        /// The user name.
        /// </param>
        /// <param name="userDisplayName">
        /// The user Display Name.
        /// </param>
        /// <param name="ip">
        /// The IP Address.
        /// </param>
        /// <param name="posted">
        /// The posted.
        /// </param>
        /// <param name="blogPostId">
        /// The blog Post Id.
        /// </param>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <param name="topicTags">
        /// The topic Tags.
        /// </param>
        /// <param name="messageId">
        /// The message Id.
        /// </param>
        /// <returns>
        /// Returns the Topic ID
        /// </returns>
        public static int SaveNew(
            this IRepository<Topic> repository,
            [NotNull] int forumId,
            [NotNull] string subject,
            [CanBeNull] string status,
            [CanBeNull] string styles,
            [CanBeNull] string description,
            [NotNull] string message,
            [NotNull] int userId,
            [NotNull] short priority,
            [CanBeNull] string userName,
            [NotNull] string userDisplayName,
            [NotNull] string ip,
            [NotNull] DateTime posted,
            [NotNull] string blogPostId,
            [NotNull] MessageFlags flags,
            [CanBeNull] string topicTags,
            out int messageId)
        {
            CodeContracts.VerifyNotNull(repository);

            var topic = new Topic
            {
                ForumID = forumId,
                TopicName = subject,
                UserID = userId,
                Posted = posted,
                Views = 0,
                Priority = priority,
                UserName = userName,
                UserDisplayName = userDisplayName,
                NumPosts = 0,
                Description = description,
                Status = status,
                Styles = styles
            };

            var newTopicId = repository.Insert(topic);

            messageId = BoardContext.Current.GetRepository<Message>().SaveNew(
                forumId,
                newTopicId,
                userId,
                message,
                userName,
                ip,
                posted,
                null,
                flags);

            if (flags.IsApproved)
            {
                repository.FireNew(newTopicId);
            }

            return newTopicId;
        }

        /// <summary>
        /// Move the Topic.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="topicId">
        /// The topic id.
        /// </param>
        /// <param name="oldForumId">
        /// The old Forum Id.
        /// </param>
        /// <param name="newForumId">
        /// The new Forum Id.
        /// </param>
        /// <param name="showMoved">
        /// The show moved.
        /// </param>
        /// <param name="linkDays">
        /// The link Days.
        /// </param>
        public static void Move(
            this IRepository<Topic> repository,
            [NotNull] int topicId,
            [NotNull] int oldForumId,
            [NotNull] int newForumId,
            [NotNull] bool showMoved,
            [NotNull] int linkDays)
        {
            CodeContracts.VerifyNotNull(repository);

            if (showMoved)
            {
                // -- delete an old link if exists
                repository.Delete(t => t.TopicMovedID == topicId);

                var topic = repository.GetById(topicId);

                var linkDate = DateTime.UtcNow.AddDays(linkDays);

                // -- create a moved message
                repository.Insert(
                    new Topic
                    {
                        ForumID = topic.ForumID,
                        UserID = topic.UserID,
                        UserName = topic.UserName,
                        UserDisplayName = topic.UserDisplayName,
                        Posted = topic.Posted,
                        TopicName = topic.TopicName,
                        Views = 0,
                        Flags = topic.Flags,
                        Priority = topic.Priority,
                        PollID = topic.PollID,
                        TopicMovedID = topicId,
                        LastPosted = topic.LastPosted,
                        NumPosts = 0,
                        LinkDate = linkDate
                    });
            }

            // -- move the topic
            repository.UpdateOnly(() => new Topic { ForumID = newForumId }, t => t.ID == topicId);

            BoardContext.Current.Get<IRaiseEvent>().Raise(new UpdateForumStatsEvent(newForumId));
            BoardContext.Current.Get<IRaiseEvent>().Raise(new UpdateForumStatsEvent(oldForumId));
        }

        /// <summary>
        /// The prune.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardId">
        /// The board id.
        /// </param>
        /// <param name="forumId">
        /// The forum id.
        /// </param>
        /// <param name="days">
        /// The days.
        /// </param>
        /// <param name="permDelete">
        /// The perm delete.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public static int Prune(
            this IRepository<Topic> repository,
            [NotNull] int boardId,
            [NotNull] int forumId,
            [NotNull] int days,
            [NotNull] bool permDelete)
        {
            CodeContracts.VerifyNotNull(repository);

            var topics = repository.DbAccess.Execute(
                db =>
                {
                    var expression = OrmLiteConfig.DialectProvider.SqlExpression<Topic>()
                        .Join<Forum>((t, f) => f.ID == t.ForumID).Join<Forum, Category>((f, c) => c.ID == f.CategoryID)
                        .Where<Topic, Forum, Category>(
                            (t, f, c) => c.BoardID == boardId && f.ID == forumId && t.Priority == 0);

                    expression.And(
                        $@"({expression.Column<Topic>(x => x.Flags, true)} & 512) = 0 
                                       and datediff(
                                             dd, 
                                             {expression.Column<Topic>(x => x.LastPosted, true)}, 
                                             GETUTCDATE()) > {days}");

                    return db.Connection.Select(expression);
                });

           topics.ForEach(x => repository.Delete(forumId, x.ID, permDelete));

           return topics.Count;
        }

        /// <summary>
        /// Delete Topic
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="forumId">
        /// The forum Id.
        /// </param>
        /// <param name="topicId">
        /// The topic Id.
        /// </param>
        public static void Delete(this IRepository<Topic> repository, [NotNull] int forumId, [NotNull] int topicId)
        {
            CodeContracts.VerifyNotNull(repository);

            repository.Delete(forumId, topicId, false);
        }

        /// <summary>
        /// Delete Topic
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="forumId">
        /// The forum Id.
        /// </param>
        /// <param name="topicId">
        /// The topic id.
        /// </param>
        /// <param name="eraseTopic">
        /// The erase topic.
        /// </param>
        public static void Delete(
            this IRepository<Topic> repository,
            [NotNull] int forumId,
            [NotNull] int topicId,
            [NotNull] bool eraseTopic)
        {
            CodeContracts.VerifyNotNull(repository);

            var topic = repository.GetById(topicId);

            BoardContext.Current.GetRepository<Forum>().UpdateOnly(
                () => new Forum
                {
                    LastPosted = null,
                    LastTopicID = null,
                    LastMessageID = null,
                    LastUserID = null,
                    LastUserName = null,
                    LastUserDisplayName = null
                },
                x => x.LastTopicID == topicId);

            BoardContext.Current.GetRepository<Active>().UpdateOnly(
                () => new Active { TopicID = null },
                t => t.TopicID == topicId);

            // -- delete messages and topics
            BoardContext.Current.GetRepository<NntpTopic>().Delete(x => x.TopicID == topicId);

            if (eraseTopic)
            {
                repository.Delete(x => x.TopicMovedID == topicId);

                BoardContext.Current.GetRepository<Message>().UpdateOnly(
                    () => new Message { ReplyTo = null },
                    t => t.TopicID == topicId);

                // -- remove all messages
                var messages = BoardContext.Current.GetRepository<Message>().Get(x => x.TopicID == topicId);

                messages.ForEach(
                    x => BoardContext.Current.GetRepository<Message>().Delete(
                        forumId,
                        topicId,
                        x.ID,
                        false,
                        string.Empty,
                        true,
                        true,
                        true));

                BoardContext.Current.GetRepository<TopicTag>().Delete(x => x.TopicID == topicId);
                BoardContext.Current.GetRepository<Activity>().Delete(x => x.TopicID == topicId);
                BoardContext.Current.GetRepository<WatchTopic>().Delete(x => x.TopicID == topicId);
                BoardContext.Current.GetRepository<TopicReadTracking>().Delete(x => x.TopicID == topicId);
                BoardContext.Current.GetRepository<FavoriteTopic>().Delete(x => x.TopicID == topicId);
                BoardContext.Current.GetRepository<Topic>().Delete(x => x.TopicMovedID == topicId);
                BoardContext.Current.GetRepository<Topic>().Delete(x => x.ID == topicId);
            }
            else
            {
                var flags = topic.TopicFlags;

                flags.IsDeleted = true;

                repository.UpdateOnly(() => new Topic { Flags = flags.BitValue }, x => x.TopicMovedID == topicId);

                repository.UpdateOnly(() => new Topic { Flags = flags.BitValue }, x => x.ID == topicId);

                BoardContext.Current.GetRepository<Message>().DbAccess.Execute(
                    db =>
                    {
                        var expression = OrmLiteConfig.DialectProvider.SqlExpression<Message>();

                        return db.Connection.ExecuteSql(
                            $@" update {expression.Table<Message>()} set Flags = Flags | 8 where TopicID = {topicId}");
                    });
            }

            BoardContext.Current.Get<ISearch>().DeleteSearchIndexRecordByTopicId(topicId);

            BoardContext.Current.Get<ILogger>().Log(
                BoardContext.Current.PageUserID,
                "YAF",
                BoardContext.Current.Get<ILocalization>().GetTextFormatted("DELETED_TOPIC", topicId),
                EventLogTypes.Information);

            repository.FireDeleted(topicId);

            BoardContext.Current.Get<IRaiseEvent>().Raise(new UpdateForumStatsEvent(forumId));
            BoardContext.Current.Get<IRaiseEvent>().Raise(new UpdateForumStatsEvent(forumId));
        }

        /// <summary>
        /// The check for duplicate topic.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="topicSubject">
        /// The topic subject.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public static bool CheckForDuplicate(this IRepository<Topic> repository, [NotNull] string topicSubject)
        {
            CodeContracts.VerifyNotNull(repository);

            var topic = repository.GetSingle(t => t.TopicName.Contains(topicSubject) && t.TopicMovedID == null);

            return topic != null;
        }

        /// <summary>
        /// The find next topic.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="currentTopic">
        /// The current topic.
        /// </param>
        /// <returns>
        /// The <see cref="Topic"/>.
        /// </returns>
        public static Topic FindNext(this IRepository<Topic> repository, [NotNull] Topic currentTopic)
        {
            CodeContracts.VerifyNotNull(repository);

            return repository.Get(
                t => t.LastPosted > currentTopic.LastPosted && t.ForumID == currentTopic.ForumID &&
                     !t.TopicFlags.IsDeleted && t.TopicMovedID == null).OrderBy(t => t.LastPosted).FirstOrDefault();
        }

        /// <summary>
        /// The find previous topic.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="currentTopic">
        /// The current topic.
        /// </param>
        /// <returns>
        /// The <see cref="Topic"/>.
        /// </returns>
        public static Topic FindPrevious(this IRepository<Topic> repository, [NotNull] Topic currentTopic)
        {
            CodeContracts.VerifyNotNull(repository);

            return repository.Get(
                    t => t.LastPosted < currentTopic.LastPosted && t.ForumID == currentTopic.ForumID &&
                         !t.TopicFlags.IsDeleted && t.TopicMovedID == null).OrderByDescending(t => t.LastPosted)
                .FirstOrDefault();
        }

        /// <summary>
        /// The simple list.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="startId">
        /// The start id.
        /// </param>
        /// <param name="limit">
        /// The limit.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Topic> SimpleList(
            this IRepository<Topic> repository,
            [CanBeNull] int startId = 0,
            [CanBeNull] int limit = 500)
        {
            CodeContracts.VerifyNotNull(repository);

            return repository.Get(t => t.ID >= limit && t.ID < startId + limit).OrderBy(t => t.ID).ToList();
        }

        /// <summary>
        /// Un-encode All Topics and Subjects
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="decodeTopicFunc">
        /// The decode topic function
        /// </param>
        public static void UnEncodeAllTopicsSubjects(
            this IRepository<Topic> repository,
            [NotNull] Func<string, string> decodeTopicFunc)
        {
            CodeContracts.VerifyNotNull(repository);

            var topics = repository.SimpleList(0, 99999999);

            topics.Where(t => t.TopicName.IsSet()).ForEach(
                topic =>
                {
                    try
                    {
                        var decodedTopic = decodeTopicFunc(topic.TopicName);

                        if (!decodedTopic.Equals(topic.TopicName))
                        {
                            // un-encode it and update.
                            repository.UpdateOnly(() => new Topic { TopicName = decodedTopic }, t => t.ID == topic.ID);
                        }
                    }
                    catch
                    {
                        // soft-fail...
                    }
                });
        }

        /// <summary>
        /// Get Deleted Topics
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="boardId">
        /// The board id.
        /// </param>
        /// <param name="filter">
        /// The filter.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Tuple<Forum, Topic>> GetDeletedTopics(
            this IRepository<Topic> repository,
            [NotNull] int boardId,
            [CanBeNull] string filter)
        {
            CodeContracts.VerifyNotNull(repository);

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Forum>();

            if (filter.IsSet())
            {
                expression.Join<Forum, Category>((forum, category) => category.ID == forum.CategoryID)
                    .Join<Topic>((f, t) => t.ForumID == f.ID).Where<Topic, Category>(
                        (t, category) =>
                            category.BoardID == boardId && t.IsDeleted == true && t.TopicName.Contains(filter));
            }
            else
            {
                expression.Join<Forum, Category>((forum, category) => category.ID == forum.CategoryID)
                    .Join<Topic>((f, t) => t.ForumID == f.ID).Where<Topic, Category>(
                        (t, category) => category.BoardID == boardId && t.IsDeleted == true);
            }

            return repository.DbAccess.Execute(db => db.Connection.SelectMulti<Forum, Topic>(expression));
        }

        /// <summary>
        /// Updates the Forum Last Post.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="forumId">
        /// The forum identifier.
        /// </param>
        /// <param name="topicId">
        /// The topic Id.
        /// </param>
        public static void UpdateLastPost(
            [NotNull] this IRepository<Topic> repository,
            [CanBeNull] int? forumId,
            [CanBeNull] int? topicId)
        {
            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Message>();

            expression.Join<Topic>((m, t) => t.ID == m.TopicID).Where<Message>(m => (m.Flags & 24) == 16)
                .OrderByDescending<Message>(m => m.Posted);

            var message = repository.DbAccess.Execute(db => db.Connection.Select(expression)).FirstOrDefault();

            if (topicId.HasValue)
            {
                repository.UpdateOnly(
                    () => new Topic
                    {
                        LastPosted = message.Posted,
                        LastMessageID = message.ID,
                        LastUserID = message.UserID,
                        LastUserName = message.UserName,
                        LastUserDisplayName = message.UserDisplayName,
                        LastMessageFlags = message.Flags
                    },
                    t => t.ID == topicId.Value);
            }
            else
            {
                repository.UpdateOnly(
                    () => new Topic
                    {
                        LastPosted = message.Posted,
                        LastMessageID = message.ID,
                        LastUserID = message.UserID,
                        LastUserName = message.UserName,
                        LastUserDisplayName = message.UserDisplayName,
                        LastMessageFlags = message.Flags
                    },
                    t => t.TopicMovedID == null && (!forumId.HasValue || t.ForumID == forumId.Value));
            }
        }

        #endregion
    }
}