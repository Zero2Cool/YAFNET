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
namespace YAF.Types.Models
{
    using System;
    using System.Data.Linq.Mapping;

    using ServiceStack.DataAnnotations;

    using YAF.Types.Interfaces.Data;

    /// <summary>
    /// A class which represents the NntpForum table.
    /// </summary>
    [Serializable]
    [Table(Name = "NntpForum")]
    public class NntpForum : IEntity, IHaveID
    {
        #region Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [Alias("NntpForumID")]
        [AutoIncrement]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the nntp server id.
        /// </summary>
        [References(typeof(NntpServer))]
        [Required]
        public int NntpServerID { get; set; }

        /// <summary>
        /// Gets or sets the group name.
        /// </summary>
        [Required]
        [StringLength(100)]
        public string GroupName { get; set; }

        /// <summary>
        /// Gets or sets the forum id.
        /// </summary>
        [References(typeof(Forum))]
        [Required]
        public int ForumID { get; set; }

        /// <summary>
        /// Gets or sets the last message no.
        /// </summary>
        [Required]
        public int LastMessageNo { get; set; }

        /// <summary>
        /// Gets or sets the last update.
        /// </summary>
        [Required]
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether active.
        /// </summary>
        [Required]
        public bool Active { get; set; }

        /// <summary>
        /// Gets or sets the date cut off.
        /// </summary>
        public DateTime? DateCutOff { get; set; }

        #endregion
    }
}