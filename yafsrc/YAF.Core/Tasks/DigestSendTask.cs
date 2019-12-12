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
namespace YAF.Core.Tasks
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using YAF.Configuration;
    using YAF.Core.Model;
    using YAF.Core.UsersRoles;
    using YAF.Types;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Types.Models;

    #endregion

    /// <summary>
    /// The digest send task.
    /// </summary>
    public class DigestSendTask : IntermittentBackgroundTask
    {
        #region Constructors and Destructors

        /// <summary>
        ///   Initializes a new instance of the <see cref = "DigestSendTask" /> class.
        /// </summary>
        public DigestSendTask()
        {
            this.RunPeriodMs = 300 * 1000;
            this.StartDelayMs = 30 * 1000;
        }

        #endregion

        #region Properties

        /// <summary>
        ///   Gets TaskName.
        /// </summary>
        public static string TaskName { get; } = "DigestSendTask";

        #endregion

        #region Public Methods

        /// <summary>
        /// The run once.
        /// </summary>
        public override void RunOnce()
        {
            //// validate DB run...
            ////this.Get<StartupInitializeDb>().Run();
            this.SendDigest();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether is time to send digest for board.
        /// </summary>
        /// <param name="boardSettings">The board settings.</param>
        /// <returns>
        /// The is time to send digest for board.
        /// </returns>
        private static bool IsTimeToSendDigestForBoard([NotNull] YafLoadBoardSettings boardSettings)
        {
            CodeContracts.VerifyNotNull(boardSettings, "boardSettings");

            if (!boardSettings.AllowDigestEmail)
            {
                return false;
            }

            var lastSend = DateTime.MinValue;
            var sendEveryXHours = boardSettings.DigestSendEveryXHours;

            if (boardSettings.LastDigestSend.IsSet())
            {
                try
                {
                    lastSend = Convert.ToDateTime(boardSettings.LastDigestSend, CultureInfo.InvariantCulture);
                }
                catch (Exception)
                {
                    lastSend = DateTime.MinValue;
                }
            }

#if (DEBUG)
            // haven't sent in X hours or more and it's 12 to 5 am.
            var sendDigest = lastSend < DateTime.Now.AddHours(-sendEveryXHours);
#else

            // haven't sent in X hours or more and it's 12 to 5 am.
            var sendDigest = lastSend < DateTime.Now.AddHours(-sendEveryXHours)
                             && DateTime.Now < DateTime.Today.AddHours(6);
#endif
            if (!sendDigest && !boardSettings.ForceDigestSend)
            {
                return false;
            }

            // && DateTime.Now < DateTime.Today.AddHours(5))
            // we're good to send -- update latest send so no duplication...
            boardSettings.LastDigestSend = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            boardSettings.ForceDigestSend = false;
            boardSettings.SaveGuestUserIdBackup();

            boardSettings.SaveRegistry();

            return true;
        }

        /// <summary>
        /// The send digest.
        /// </summary>
        private void SendDigest()
        {
            try
            {
                var boardIds = this.GetRepository<Board>().ListTyped().Select(b => b.ID);

                boardIds.ForEach(
                    boardId =>
                    {
                        var boardSettings = new YafLoadBoardSettings(boardId);

                        if (!IsTimeToSendDigestForBoard(boardSettings))
                        {
                            return;
                        }

                        // get users with digest enabled...
                        var usersWithDigest = this.GetRepository<User>()
                            .FindUserTyped(false, boardId, dailyDigest: true).Where(
                                x => x.IsGuest != null && !x.IsGuest.Value && (x.IsApproved ?? false));

                        var typedUserFinds = usersWithDigest as IList<User> ?? usersWithDigest.ToList();

                        if (typedUserFinds.Any())
                        {
                            // start sending...
                            this.SendDigestToUsers(typedUserFinds, boardSettings);
                        }
                        else
                        {
                            this.Get<ILogger>().Info("no user found");
                        }
                    });
            }
            catch (Exception ex)
            {
                this.Get<ILogger>().Error(ex, $"Error In {TaskName} Task");
            }
        }

        /// <summary>
        /// Sends the digest to users.
        /// </summary>
        /// <param name="usersWithDigest">The users with digest.</param>
        /// <param name="boardSettings">The board settings.</param>
        private void SendDigestToUsers(
            IEnumerable<User> usersWithDigest,
            YafBoardSettings boardSettings)
        {
            var usersSendCount = 0;

            usersWithDigest.ForEach(
                user =>
                    {
                        try
                        {
                            var digestHtml = this.Get<IDigest>().GetDigestHtml(user.ID, boardSettings);

                            if (digestHtml.IsNotSet())
                            {
                                return;
                            }

                            if (user.ProviderUserKey == null)
                            {
                                return;
                            }

                            var membershipUser = UserMembershipHelper.GetUser(user.Name);

                            if (membershipUser == null || membershipUser.Email.IsNotSet())
                            {
                                return;
                            }

                            // send the digest...
                            this.Get<IDigest>().SendDigest(
                                digestHtml,
                                boardSettings.Name,
                                boardSettings.ForumEmail,
                                membershipUser.Email,
                                user.DisplayName,
                                true);

                            usersSendCount++;
                        }
                        catch (Exception e)
                        {
                            this.Get<ILogger>().Error(e, $"Error In Creating Digest for User {user.ID}");
                        }
                    });

            this.Get<ILogger>().Info($"Digest send to {usersSendCount} user(s)");
        }

        #endregion
    }
}