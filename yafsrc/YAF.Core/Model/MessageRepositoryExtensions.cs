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
    using System.Globalization;
    using System.Linq;

    using ServiceStack.OrmLite;

    using YAF.Configuration;
    using YAF.Core.Context;
    using YAF.Core.Extensions;
    using YAF.Types;
    using YAF.Types.Constants;
    using YAF.Types.Extensions;
    using YAF.Types.Extensions.Data;
    using YAF.Types.Flags;
    using YAF.Types.Interfaces;
    using YAF.Types.Interfaces.Data;
    using YAF.Types.Models;
    using YAF.Types.Objects;
    using YAF.Utils.Helpers;

    /// <summary>
    ///     The Message repository extensions.
    /// </summary>
    public static class MessageRepositoryExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// Checks if the User has replied to the specific topic.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="topicId">
        /// The topic Id.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <returns>
        /// Returns if true or not
        /// </returns>
        public static bool RepliedTopic(
            this IRepository<Message> repository,
            [NotNull] int topicId,
            [NotNull] int userId)
        {
            return repository.Count(m => m.TopicID == topicId && m.UserID == userId) > 0;
        }

        /// <summary>
        /// The get message.
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
        public static Tuple<Topic, Message, User, Forum> GetMessage(
            this IRepository<Message> repository,
            [NotNull] int messageId)
        {
            CodeContracts.VerifyNotNull(repository);

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Topic>();

            expression.Join<Message>((t, m) => m.TopicID == t.ID)
                .Join<Message, User>((m, u) => u.ID == m.UserID)
                .Join<Forum>((t, f) => f.ID == t.ForumID)
                .Where<Message>(m => m.ID == messageId);

            return repository.DbAccess.Execute(
                db => db.Connection.SelectMulti<Topic, Message, User, Forum>(expression)).FirstOrDefault();
        }

        /// <summary>
        /// Gets the message with access.
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
        /// The <see cref="Tuple"/>.
        /// </returns>
        public static Tuple<Topic, Message, User, Forum> GetMessageWithAccess(
            this IRepository<Message> repository,
            [NotNull] int messageId,
            [NotNull] int userId)
        {
            CodeContracts.VerifyNotNull(repository);

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Topic>();

            expression.Join<Message>((t, m) => m.TopicID == t.ID).Join<Message, User>((m, u) => u.ID == m.UserID)
                .Join<Forum>((t, f) => f.ID == t.ForumID).LeftJoin<ActiveAccess>((t, x) => x.ForumID == t.ForumID)
                .Where<Message, ActiveAccess>((m, x) => m.ID == messageId && x.UserID == userId && x.ReadAccess);

            return repository.DbAccess.Execute(db => db.Connection.SelectMulti<Topic, Message, User, Forum>(expression))
                .FirstOrDefault();
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
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Tuple<Forum, Topic, Message>> GetDeletedMessages(
            this IRepository<Message> repository,
            [NotNull] int boardId)
        {
            CodeContracts.VerifyNotNull(repository);

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Forum>();

            expression.Join<Forum, Category>((forum, category) => category.ID == forum.CategoryID)
                .Join<Topic>((f, t) => t.ForumID == f.ID).Join<Topic, Message>((t, m) => m.TopicID == t.ID)
                .Where<Message, Category>((m, category) => category.BoardID == boardId && m.IsDeleted == true);

            return repository.DbAccess.Execute(db => db.Connection.SelectMulti<Forum, Topic, Message>(expression));
        }

        /// <summary>
        /// The post_list.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="topicId">
        /// The topic id.
        /// </param>
        /// <param name="currentUserID">
        /// The current User ID.
        /// </param>
        /// <param name="authorUserID">
        /// The author User ID.
        /// </param>
        /// <param name="updateViewCount">
        /// The update view count.
        /// </param>
        /// <param name="showDeleted">
        /// The show deleted.
        /// </param>
        /// <param name="styledNicks">
        /// The styled nicks.
        /// </param>
        /// <param name="showReputation">
        /// The show Reputation.
        /// </param>
        /// <param name="sincePostedDate">
        /// The posted date.
        /// </param>
        /// <param name="toPostedDate">
        /// The to Posted Date.
        /// </param>
        /// <param name="sinceEditedDate">
        /// The edited date.
        /// </param>
        /// <param name="toEditedDate">
        /// The to Edited Date.
        /// </param>
        /// <param name="pageIndex">
        /// The page index.
        /// </param>
        /// <param name="pageSize">
        /// The Page size.
        /// </param>
        /// <param name="sortPosted">
        /// The sort by posted date.
        ///   0 - no sort, 1 - ASC, 2 - DESC
        /// </param>
        /// <param name="sortEdited">
        /// The sort by edited date.
        ///   0 - no sort, 1 - ASC, 2 - DESC.
        /// </param>
        /// <param name="sortPosition">
        /// The sort Position.
        /// </param>
        /// <param name="showThanks">
        /// The show thanks. Returns thanked posts. Not implemented.
        /// </param>
        /// <param name="messagePosition">
        /// The message Position.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static DataTable PostListAsDataTable(
            this IRepository<Message> repository,
            [NotNull] object topicId,
            object currentUserID,
            [NotNull] object authorUserID,
            [NotNull] object updateViewCount,
            bool showDeleted,
            bool styledNicks,
            bool showReputation,
            DateTime sincePostedDate,
            DateTime toPostedDate,
            DateTime sinceEditedDate,
            DateTime toEditedDate,
            int pageIndex,
            int pageSize,
            int sortPosted,
            int sortEdited,
            int sortPosition,
            bool showThanks,
            int messagePosition)
        {
            return repository.DbFunction.GetData.post_list(
                TopicID: topicId,
                PageUserID: currentUserID,
                AuthorUserID: authorUserID,
                UpdateViewCount: updateViewCount,
                ShowDeleted: showDeleted,
                StyledNicks: styledNicks,
                ShowReputation: showReputation,
                SincePostedDate: sincePostedDate,
                ToPostedDate: toPostedDate,
                SinceEditedDate: sinceEditedDate,
                ToEditedDate: toEditedDate,
                PageIndex: pageIndex,
                PageSize: pageSize,
                SortPosted: sortPosted,
                SortEdited: sortEdited,
                SortPosition: sortPosition,
                ShowThanks: showThanks,
                MessagePosition: messagePosition,
                UTCTIMESTAMP: DateTime.UtcNow);
        }

        /// <summary>
        /// The last posts.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="topicId">
        /// The topic id.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        public static List<Tuple<Message, User>> LastPosts(
            this IRepository<Message> repository,
            [NotNull] int topicId)
        {
            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Message>();

            expression.Join<Message, User>((m, u) => m.UserID == u.ID)
                .Where<Message>(m => m.TopicID == topicId && (m.Flags & 24) == 16).OrderByDescending(m => m.Posted).Take(10);

            return repository.DbAccess.Execute(db => db.Connection.SelectMulti<Message, User>(expression));
        }

        /// <summary>
        /// Gets all the post by a user.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <returns>
        /// Returns all the post by a user.
        /// </returns>
        public static IOrderedEnumerable<Message> GetAllUserMessages(this IRepository<Message> repository, int userId)
        {
            return repository.Get(m => m.UserID == userId).OrderByDescending(m => m.Posted);
        }

        /// <summary>
        /// Gets all the post by a user. (With Read Access of current Page User)
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
        /// <param name="pageUserId">
        /// The page User Id.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <returns>
        /// The <see cref="List"/>.
        /// </returns>
        public static List<Tuple<Message, Topic, User>> GetAllUserMessagesWithAccess(
            this IRepository<Message> repository,
            [NotNull] int boardId,
            [NotNull] int userId,
            [NotNull] int pageUserId,
            [NotNull] int count)
        {
            CodeContracts.VerifyNotNull(repository);

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Message>();

            expression.Join<User>((a, b) => b.ID == a.UserID).Join<Topic>((a, c) => c.ID == a.TopicID)
                .Join<Topic, Forum>((c, d) => d.ID == c.ForumID).Join<Forum, Category>((d, e) => e.ID == d.CategoryID)
                .Join<Category, ActiveAccess>((d, x) => x.ForumID == d.ID)
                .Where<Topic, Message, ActiveAccess, Category>(
                    (topic, message, x, e) => message.UserID == userId && x.UserID == pageUserId && x.ReadAccess &&
                                              e.BoardID == boardId && topic.IsDeleted == false &&
                                              message.IsDeleted == false)
                .OrderByDescending<Message>(x => x.Posted).Take(count);

            return repository.DbAccess.Execute(db => db.Connection.SelectMulti<Message, Topic, User>(expression));
        }

        /// <summary>
        /// Gets all messages by forum as Typed Search Message List.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="forumId">
        /// The forum Id.
        /// </param>
        /// <returns>
        /// Returns all Messages as Typed Search Message List
        /// </returns>
        public static IEnumerable<SearchMessage> GetAllSearchMessagesByForum(
            this IRepository<Message> repository,
            [NotNull]int forumId)
        {
            CodeContracts.VerifyNotNull(repository);

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Forum>();

            expression.Join<Topic>((forum, topic) => topic.ForumID == forum.ID)
                .Join<Topic, Message>((topic, message) => message.TopicID == topic.ID)
                .Join<Message, User>((message, user) => user.ID == message.UserID).Where<Forum, Topic, Message>(
                    (forum, topic, message) => forum.ID == forumId && topic.IsDeleted == false &&
                                               message.IsDeleted == false && message.IsApproved == true &&
                                               topic.TopicMovedID == null).OrderByDescending<Message>(x => x.Posted);

            return repository.DbAccess.Execute(db => db.Connection.SelectMulti<Forum, Topic, Message, User>(expression))
                .ConvertAll(x => new SearchMessage(x));
        }

        /// <summary>
        /// Approves the message.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="messageId">The message identifier.</param>
        public static void ApproveMessage(this IRepository<Message> repository, int messageId)
        {
            CodeContracts.VerifyNotNull(repository);

            repository.DbFunction.Query.message_approve(MessageID: messageId);

            repository.FireUpdated(messageId);
        }

        /// <summary>
        /// Updates the flags.
        /// </summary>
        /// <param name="repository">The repository.</param>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="flags">The flags.</param>
        public static void UpdateFlags(this IRepository<Message> repository, int messageId, int flags)
        {
            CodeContracts.VerifyNotNull(repository);

            repository.UpdateOnly(() => new Message { Flags = flags }, u => u.ID == messageId);
        }

        /// <summary>
        /// The message_delete.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <param name="isModeratorChanged">
        /// The is moderator changed.
        /// </param>
        /// <param name="deleteReason">
        /// The delete reason.
        /// </param>
        /// <param name="isDeleteAction">
        /// The is delete action.
        /// </param>
        /// <param name="deleteLinked">
        /// The delete linked.
        /// </param>
        public static void Delete(
            this IRepository<Message> repository,
            [NotNull] int messageID,
            bool isModeratorChanged,
            [NotNull] string deleteReason,
            int isDeleteAction,
            bool deleteLinked)
        {
            repository.Delete(messageID, isModeratorChanged, deleteReason, isDeleteAction, deleteLinked, false);

            repository.FireDeleted(messageID);
        }

        /// <summary>
        /// The message_delete.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <param name="isModeratorChanged">
        /// The is moderator changed.
        /// </param>
        /// <param name="deleteReason">
        /// The delete reason.
        /// </param>
        /// <param name="isDeleteAction">
        /// The is delete action.
        /// </param>
        /// <param name="deleteLinked">
        /// The delete linked.
        /// </param>
        /// <param name="eraseMessage">
        /// The erase message.
        /// </param>
        public static void Delete(
            this IRepository<Message> repository,
            [NotNull] int messageID,
            bool isModeratorChanged,
            [NotNull] string deleteReason,
            int isDeleteAction,
            bool deleteLinked,
            bool eraseMessage)
        {
            repository.DeleteRecursively(
                messageID,
                isModeratorChanged,
                deleteReason,
                isDeleteAction,
                deleteLinked,
                eraseMessage);
        }

        /// <summary>
        /// message move function
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <param name="moveToTopic">
        /// The move to topic.
        /// </param>
        /// <param name="moveAll">
        /// The move all.
        /// </param>
        public static void Move(
            this IRepository<Message> repository,
            [NotNull] int messageID,
            [NotNull] int moveToTopic,
            bool moveAll)
        {
            repository.DbFunction.Scalar.message_move(MessageID: messageID, MoveToTopic: moveToTopic);

            if (!moveAll)
            {
                return;
            }

            // moveAll=true anyway
            // it's in charge of moving answers of moved post
            var replies = repository.Get(m => m.ReplyTo == messageID).Select(x => x.ID);

            replies.ForEach(replyId => repository.MoveRecursively(replyId, moveToTopic));
        }

        /// <summary>
        /// gets list of replies to message
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageId">
        /// The message Id.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        [NotNull]
        public static List<Tuple<Message, User>> Replies(this IRepository<Message> repository, [NotNull] int messageId)
        {
            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Message>();

            expression.Join<User>((message, user) => user.ID == message.UserID).Where<Message>(
                    m => m.IsApproved.Value && m.ReplyTo == messageId);

            return repository.DbAccess.Execute(db => db.Connection.SelectMulti<Message, User>(expression));
        }

        /// <summary>
        /// Finds the Unread Message
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="topicId">
        /// The topic Id.
        /// </param>
        /// <param name="messageId">
        /// The message Id.
        /// </param>
        /// <param name="lastRead">
        /// The last read.
        /// </param>
        /// <param name="showDeleted">
        /// The show Deleted.
        /// </param>
        /// <param name="authorUserId">
        /// The author User Id.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static DataTable FindUnreadAsDataTable(
            this IRepository<Message> repository,
            [NotNull] int topicId,
            [NotNull] int messageId,
            [NotNull] DateTime lastRead,
            [NotNull] bool showDeleted,
            [NotNull] int authorUserId)
        {
            // Make sure there are no more DateTime.MinValues coming from db.
            if (lastRead == DateTime.MinValue)
            {
                lastRead = DateTimeHelper.SqlDbMinTime();
            }

            return repository.DbFunction.GetData.message_findunread(
                TopicID: topicId,
                MessageID: messageId,
                MinDateTime: DateTimeHelper.SqlDbMinTime().AddYears(-1),
                LastRead: lastRead,
                ShowDeleted: showDeleted,
                AuthorUserID: authorUserId);
        }

        /// <summary>
        /// Retrieve all reported messages with the correct forumID argument.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="forumId">
        /// The forum Id.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static DataTable ListReportedAsDataTable(this IRepository<Message> repository, [NotNull] int forumId)
        {
            return repository.DbFunction.GetData.message_listreported(ForumID: forumId);
        }

        /// <summary>
        /// Save reported message back to the database.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageId">
        /// The message Id.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <param name="reportedDateTime">
        /// The reported date time.
        /// </param>
        /// <param name="reportText">
        /// The report text.
        /// </param>
        public static void Report(
            this IRepository<Message> repository,
            [NotNull] int messageId,
            [NotNull] int userId,
            [NotNull] DateTime reportedDateTime,
            [NotNull] string reportText)
        {
            repository.DbFunction.Scalar.message_report(
                MessageID: messageId,
                ReporterID: userId,
                ReportedDate: reportedDateTime,
                ReportText: reportText,
                UTCTIMESTAMP: DateTime.UtcNow);
        }

        /// <summary>
        /// Copy current Message text over reported Message text.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageFlag">
        /// The message flag.
        /// </param>
        /// <param name="messageId">
        /// The message Id.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        public static void ReportResolve(
            this IRepository<Message> repository,
            [NotNull] int messageFlag,
            [NotNull] int messageId,
            [NotNull] int userId)
        {
            repository.DbFunction.Scalar.message_reportresolve(
                MessageFlag: messageFlag,
                MessageID: messageId,
                UserID: userId,
                UTCTIMESTAMP: DateTime.UtcNow);
        }

        /// <summary>
        /// Saves the new Message
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="topicId">
        /// The topic Id.
        /// </param>
        /// <param name="userId">
        /// The user Id.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="guestUserName">
        /// The guest user name.
        /// </param>
        /// <param name="ipAddress">
        /// The IP Address.
        /// </param>
        /// <param name="posted">
        /// The posted.
        /// </param>
        /// <param name="replyTo">
        /// The reply to.
        /// </param>
        /// <param name="flags">
        /// The Message flags.
        /// </param>
        /// <returns>
        /// Returns the Message ID
        /// </returns>
        public static int SaveNew(
            this IRepository<Message> repository,
            [NotNull] long topicId,
            [NotNull] int userId,
            [NotNull] string message,
            [NotNull] string guestUserName,
            [NotNull] string ipAddress,
            [NotNull] DateTime posted,
            [CanBeNull] int? replyTo,
            [NotNull] MessageFlags flags)
        {
            IDbDataParameter parameterMessage = null;

            repository.SqlList(
                "message_save",
                cmd =>
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.AddParam("TopicID", topicId);
                        cmd.AddParam("UserID", userId);
                        cmd.AddParam("Message", message);
                        cmd.AddParam("UserName", guestUserName);
                        cmd.AddParam("IP", ipAddress);
                        cmd.AddParam("Posted", posted);
                        cmd.AddParam("ReplyTo", replyTo);
                        cmd.AddParam("BlogPostID", null);
                        cmd.AddParam("Flags", flags.BitValue);
                        cmd.AddParam("UTCTIMESTAMP", DateTime.UtcNow);

                        parameterMessage = cmd.AddParam("MessageID", direction: ParameterDirection.Output);
                    });

            var messageId = parameterMessage.Value.ToType<int>();

            // Add to search index
            var newMessage = new SearchMessage
                                    {
                                        MessageId = messageId,
                                        Message = message,
                                        Flags = flags.BitValue,
                                        Posted = posted.ToString(CultureInfo.InvariantCulture),
                                        UserName = BoardContext.Current.MembershipUser.UserName,
                                        UserDisplayName = BoardContext.Current.User.DisplayName,
                                        UserStyle = BoardContext.Current.User.UserStyle,
                                        UserId = BoardContext.Current.PageUserID,
                                        TopicId = BoardContext.Current.PageTopicID,
                                        Topic = BoardContext.Current.PageTopicName,
                                        ForumId = BoardContext.Current.PageForumID,
                                        TopicTags = string.Empty,
                                        ForumName = BoardContext.Current.PageForumName,
                                        Description = string.Empty
                                    };

            BoardContext.Current.Get<ISearch>().AddSearchIndexItem(newMessage);

            repository.FireNew(messageId);

            return messageId;
        }

        /// <summary>
        /// The message_unapproved.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="forumId">
        /// The forum Id.
        /// </param>
        /// <returns>
        /// The <see cref="DataTable"/>.
        /// </returns>
        public static List<Tuple<Topic, Message, User>> Unapproved(this IRepository<Message> repository, [NotNull] int forumId)
        {
            CodeContracts.VerifyNotNull(repository);

            var expression = OrmLiteConfig.DialectProvider.SqlExpression<Topic>();

            expression.Join<Message>((topic, message) => message.TopicID == topic.ID)
                .Join<Message, User>((message, user) => message.UserID == user.ID).Where<Topic, Message>(
                    (topic, message) => topic.ForumID == forumId && message.IsApproved == false &&
                                        topic.IsDeleted == false && message.IsDeleted == false);

            return repository.DbAccess.Execute(db => db.Connection.SelectMulti<Topic, Message, User>(expression));
        }

        /// <summary>
        /// The message_update.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageId">
        /// The message Id.
        /// </param>
        /// <param name="priority">
        /// The priority.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        /// <param name="status">
        /// The status.
        /// </param>
        /// <param name="styles">
        /// The styles.
        /// </param>
        /// <param name="subject">
        /// The subject.
        /// </param>
        /// <param name="reasonOfEdit">
        /// The reason of edit.
        /// </param>
        /// <param name="isModeratorChanged">
        /// The is moderator changed.
        /// </param>
        /// <param name="overrideApproval">
        /// The override approval.
        /// </param>
        /// <param name="originalMessage">
        /// The original Message.
        /// </param>
        /// <param name="editedBy">
        /// UserId of who edited the message.
        /// </param>
        public static void Update(
            this IRepository<Message> repository,
            [NotNull] int messageId,
            [CanBeNull] int? priority,
            [NotNull] string message,
            [CanBeNull] string description,
            [CanBeNull] string status,
            [CanBeNull] string styles,
            [CanBeNull] string subject,
            [NotNull] string reasonOfEdit,
            [NotNull] bool isModeratorChanged,
            [NotNull] bool? overrideApproval,
            [NotNull] Tuple<Topic, Message, User, Forum> originalMessage,
            [NotNull] int editedBy)
        {
            repository.DbFunction.Scalar.message_update(
                MessageID: messageId,
                Priority: priority,
                Message: message,
                Description: description,
                Status: status,
                Styles: styles,
                Subject: subject,
                Flags: originalMessage.Item1.Flags,
                Reason: reasonOfEdit,
                EditedBy: editedBy,
                IsModeratorChanged: isModeratorChanged,
                OverrideApproval: overrideApproval,
                OriginalMessage: originalMessage.Item2.MessageText,
                CurrentUtcTimestamp: DateTime.UtcNow);

            // Update Search index Item
            var updateMessage = new SearchMessage
                                    {
                                        MessageId = messageId,
                                        Message = message,
                                        Flags = originalMessage.Item1.Flags,
                                        Posted = originalMessage.Item1.Posted.ToString(CultureInfo.InvariantCulture),
                                        UserName = originalMessage.Item4.Name,
                                        UserDisplayName = originalMessage.Item3.DisplayName,
                                        UserStyle = originalMessage.Item3.UserStyle,
                                        UserId = originalMessage.Item4.ID,
                                        TopicId = originalMessage.Item2.ID,
                                        Topic = originalMessage.Item1.TopicName,
                                        ForumId = originalMessage.Item3.ID,
                                        ForumName = originalMessage.Item3.Name,
                                        Description = description
                                    };

            BoardContext.Current.Get<ISearch>().UpdateSearchIndexItem(updateMessage, true);
        }

        /// <summary>
        /// Gets the List of all message changes.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageId">
        /// The Message ID.
        /// </param>
        /// <param name="daysToClean">
        /// Days to clean.
        /// </param>
        /// <returns>
        /// Returns the List of all message changes.
        /// </returns>
        public static DataTable HistoryListAsDataTable(
            this IRepository<Message> repository,
            int messageId,
            int daysToClean)
        {
            return repository.DbFunction.GetData.messagehistory_list(
                MessageID: messageId,
                DaysToClean: daysToClean,
                UTCTIMESTAMP: DateTime.UtcNow);
        }

        #endregion

        /// <summary>
        /// The message_delete recursively.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <param name="isModeratorChanged">
        /// The is moderator changed.
        /// </param>
        /// <param name="deleteReason">
        /// The delete reason.
        /// </param>
        /// <param name="isDeleteAction">
        /// The is delete action.
        /// </param>
        /// <param name="deleteLinked">
        /// The delete linked.
        /// </param>
        /// <param name="eraseMessages">
        /// The erase messages.
        /// </param>
        private static void DeleteRecursively(
            this IRepository<Message> repository,
            [NotNull] int messageID,
            bool isModeratorChanged,
            [NotNull] string deleteReason,
            int isDeleteAction,
            bool deleteLinked,
            bool eraseMessages)
        {
            var useFileTable = BoardContext.Current.Get<BoardSettings>().UseFileTable;

            if (deleteLinked)
            {
                // Delete replies
                var replies = repository.Get(m => m.ReplyTo == messageID).Select(x => x.ID);

                replies.ForEach(
                    replyId => repository.DeleteRecursively(
                        replyId,
                        isModeratorChanged,
                        deleteReason,
                        isDeleteAction,
                        true,
                        eraseMessages));
            }

            // If the files are actually saved in the Hard Drive
            if (!useFileTable)
            {
                BoardContext.Current.GetRepository<Attachment>().DeleteByMessageId(messageID);
            }

            // Ederon : erase message for good
            if (eraseMessages)
            {
                // Delete Message from Search Index
                BoardContext.Current.Get<ISearch>().DeleteSearchIndexRecordByMessageId(messageID);

                BoardContext.Current.Get<ILogger>().Log(
                    BoardContext.Current.PageUserID,
                    "YAF",
                    BoardContext.Current.Get<ILocalization>().GetTextFormatted("DELETED_MESSAGE", messageID),
                    EventLogTypes.Information);

                repository.DbFunction.Scalar.message_delete(MessageID: messageID, EraseMessage: true);
            }
            else
            {
                // Delete Message
                // un-delete function added
                repository.DbFunction.Scalar.message_deleteundelete(
                    MessageID: messageID,
                    isModeratorChanged: isModeratorChanged,
                    DeleteReason: deleteReason,
                    isDeleteAction: isDeleteAction);
            }
        }

        /// <summary>
        /// moves answers of moved post
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="messageID">
        /// The message id.
        /// </param>
        /// <param name="moveToTopic">
        /// The move to topic.
        /// </param>
        private static void MoveRecursively(
            this IRepository<Message> repository,
            [NotNull] int messageID,
            [NotNull] int moveToTopic)
        {
            var replies = repository.Get(m => m.ReplyTo == messageID).Select(x => x.ID);

            replies.ForEach(replyId => repository.MoveRecursively(replyId, moveToTopic));

            repository.DbFunction.Scalar.message_move(MessageID: messageID, MoveToTopic: moveToTopic);
        }
    }
}