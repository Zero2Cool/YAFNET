/* Yet Another Forum.NET
 * Copyright (C) 2003-2005 Bj�rnar Henden
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

    using YAF.Core.Extensions;
    using YAF.Types.Constants;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Types.Models;

    #endregion

    /// <summary>
    /// The forum delete task.
    /// </summary>
    public class BoardDeleteTask : LongBackgroundTask, ICriticalBackgroundTask
    {
        #region Constants and Fields

        /// <summary>
        /// The Blocking Task Names.
        /// </summary>
        private static readonly string[] BlockingTaskNames = Constants.ForumRebuild.BlockingTaskNames;

        #endregion

        #region Properties

        /// <summary>
        /// Gets TaskName.
        /// </summary>
        public static string TaskName { get; } = "BoardDeleteTask";

        /// <summary>
        /// Gets or sets BoardIdToDelete.
        /// </summary>
        public int BoardIdToDelete { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates the Board Delete Task
        /// </summary>
        /// <param name="boardId">
        /// The board id.
        /// </param>
        /// <param name="failureMessage"> 
        /// The failure message - is empty if task is launched successfully.
        /// </param>
        /// <returns>
        /// Returns if Task was Successful
        /// </returns>
        public static bool Start(int boardId, out string failureMessage)
        {
            failureMessage = string.Empty;
            
            if (YafContext.Current.Get<ITaskModuleManager>() == null)
            {
                return false;
            }

            if (!YafContext.Current.Get<ITaskModuleManager>().AreTasksRunning(BlockingTaskNames))
            {
                YafContext.Current.Get<ITaskModuleManager>()
                    .StartTask(TaskName, () => new BoardDeleteTask { BoardIdToDelete = boardId });
            }
            else
            {
                failureMessage =
                    $"You can't delete the board while some of the blocking {BlockingTaskNames.ToDelimitedString(",")} tasks are running.";
            }

            return true;
        }

        /// <summary>
        /// The run once.
        /// </summary>
        public override void RunOnce()
        {
            try
            {
                this.Logger.Info("Starting Board delete task for BoardId {0} delete task.", this.BoardIdToDelete);

                this.GetRepository<Board>().DeleteById(this.BoardIdToDelete);
                this.Logger.Info("Board delete task for BoardId {0} delete task is completed.", this.BoardIdToDelete);
            }
            catch (Exception x)
            {
                this.Logger.Error(x, "Error In Board (ID: {0}) Delete Task: {1}", this.BoardIdToDelete, TaskName);
            }
        }

        #endregion
    }
}