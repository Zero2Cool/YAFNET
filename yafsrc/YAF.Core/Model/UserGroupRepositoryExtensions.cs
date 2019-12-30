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
    using System.Data;

    using YAF.Types;
    using YAF.Types.Interfaces.Data;
    using YAF.Types.Models;

    /// <summary>
    ///     The UserGroup repository extensions.
    /// </summary>
    public static class UserGroupRepositoryExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The usergroup_list.
        /// </summary>
        /// <param name="userID">
        /// The user id.
        /// </param>
        /// <returns>
        /// </returns>
        public static DataTable ListAsDataTable(
            this IRepository<UserGroup> repository, [NotNull] object userID)
        {
            return repository.DbFunction.GetData.usergroup_list(UserID: userID);
        }

        /// <summary>
        /// The usergroup_save.
        /// </summary>
        /// <param name="userID">
        /// The user id.
        /// </param>
        /// <param name="groupID">
        /// The group id.
        /// </param>
        /// <param name="member">
        /// The member.
        /// </param>
        public static void Save(
            this IRepository<UserGroup> repository, [NotNull] object userID, [NotNull] object groupID, [NotNull] object member)
        {
            repository.DbFunction.Scalar.usergroup_save(UserID: userID, GroupID: groupID, Member: member);
        }

        #endregion
    }
}