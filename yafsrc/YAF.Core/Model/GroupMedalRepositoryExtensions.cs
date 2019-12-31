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
    using YAF.Core.Extensions;
    using YAF.Types;
    using YAF.Types.Interfaces.Data;
    using YAF.Types.Models;

    /// <summary>
    /// The group medal repository extensions.
    /// </summary>
    public static class GroupMedalRepositoryExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// Update existing group-medal allocation.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="groupID">
        /// The group ID.
        /// </param>
        /// <param name="medalID">
        /// ID of medal.
        /// </param>
        /// <param name="message">
        /// Medal message, to override medal's default one. Can be null.
        /// </param>
        /// <param name="hide">
        /// Hide medal in user box.
        /// </param>
        /// <param name="onlyRibbon">
        /// Show only ribbon bar in user box.
        /// </param>
        /// <param name="sortOrder">
        /// Sort order in user box. Overrides medal's default sort order.
        /// </param>
        public static void Save(
            this IRepository<GroupMedal> repository,
            [NotNull] int groupID,
            [NotNull] int medalID,
            [NotNull] string message,
            [NotNull] bool hide,
            [NotNull] bool onlyRibbon,
            [NotNull] byte sortOrder)
        {
            repository.UpdateOnly(
                () => new GroupMedal
                {
                    Message = message,
                    Hide = hide,
                    OnlyRibbon = onlyRibbon,
                    SortOrder = sortOrder
                },
                m => m.GroupID == groupID && m.MedalID == medalID);
        }

        /// <summary>
        /// Saves new group-medal allocation.
        /// </summary>
        /// <param name="repository">
        /// The repository.
        /// </param>
        /// <param name="groupID">
        /// The group ID.
        /// </param>
        /// <param name="medalID">
        /// ID of medal.
        /// </param>
        /// <param name="message">
        /// Medal message, to override medal's default one. Can be null.
        /// </param>
        /// <param name="hide">
        /// Hide medal in user box.
        /// </param>
        /// <param name="onlyRibbon">
        /// Show only ribbon bar in user box.
        /// </param>
        /// <param name="sortOrder">
        /// Sort order in user box. Overrides medal's default sort order.
        /// </param>
        public static void SaveNew(
            this IRepository<GroupMedal> repository,
            [NotNull] int groupID,
            [NotNull] int medalID,
            [NotNull] string message,
            [NotNull] bool hide,
            [NotNull] bool onlyRibbon,
            [NotNull] byte sortOrder)
        {
            repository.Insert(
                new GroupMedal
                {
                    GroupID = groupID,
                    MedalID = medalID,
                    Message = message,
                    Hide = hide,
                    OnlyRibbon = onlyRibbon,
                    SortOrder = sortOrder
                });
        }

        #endregion
    }
}