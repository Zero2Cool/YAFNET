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

namespace YAF.Core.Data
{
    using YAF.Configuration;
    using YAF.Types;

    /// <summary>
    /// Command Text Helpers
    /// </summary>
    public static class CommandTextHelpers
    {
        /// <summary>
        /// Gets command text replaced with {databaseOwner} and {objectQualifier}.
        /// </summary>
        /// <param name="commandText">
        /// Test to transform.
        /// </param>
        /// <returns>
        /// The get command text replaced.
        /// </returns>
        [NotNull]
        public static string GetCommandTextReplaced([NotNull] string commandText)
        {
            commandText = commandText.Replace("{databaseOwner}", Config.DatabaseOwner);
            commandText = commandText.Replace("{objectQualifier}", Config.DatabaseObjectQualifier);

            return commandText;
        }

        /// <summary>
        /// Gets qualified object name
        /// </summary>
        /// <param name="name">
        /// Base name of an object
        /// </param>
        /// <returns>
        /// Returns qualified object name of format {databaseOwner}.{objectQualifier}name
        /// </returns>
        public static string GetObjectName([NotNull] string name)
        {
            return $"[{Config.DatabaseOwner}].[{Config.DatabaseObjectQualifier}{name}]";
        }
    }
}