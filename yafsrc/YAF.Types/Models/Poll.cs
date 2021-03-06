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

    using YAF.Types.Flags;
    using YAF.Types.Interfaces.Data;

    /// <summary>
    /// A class which represents the Poll table.
    /// </summary>
    [Serializable]
    [Table(Name = "Poll")]
    [PostCreateTable("alter table [{databaseOwner}].[{tableName}] add IsDeleted  as (CONVERT([bit],sign([Flags]&(8)),0))" +
                         "alter table [{databaseOwner}].[{tableName}] add IsApproved as (CONVERT([bit],sign([Flags]&(16)),(0)))")]
    public class Poll : IEntity, IHaveID
    {
        #region Properties

        /// <summary>
        /// Gets or sets the Poll id.
        /// </summary>
        [Alias("PollID")]
        [AutoIncrement]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the question.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Question { get; set; }

        /// <summary>
        /// Gets or sets the closes.
        /// </summary>
        public DateTime? Closes { get; set; }

        /// <summary>
        /// Gets or sets the user id.
        /// </summary>
        [Required]
        [Default(1)]
        public int UserID { get; set; }

        /// <summary>
        /// Gets or sets the object path.
        /// </summary>
        [StringLength(255)]
        public string ObjectPath { get; set; }

        /// <summary>
        /// Gets or sets the flags.
        /// </summary>
        [Default(0)]
        public int Flags { get; set; }

        /// <summary>
        /// Gets or sets the Poll flags.
        /// </summary>
        [Ignore]
        public PollFlags PollFlags
        {
            get => new PollFlags(this.Flags);

            set => this.Flags = value.BitValue;
        }

        /// <summary>
        /// Gets or sets the allow skip vote.
        /// </summary>
        [Compute]
        public bool? AllowSkipVote { get; set; }

        #endregion
    }
}