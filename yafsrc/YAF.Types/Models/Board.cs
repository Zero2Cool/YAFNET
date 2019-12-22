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
namespace YAF.Types.Models
{
    using System;

    using ServiceStack.DataAnnotations;

    using YAF.Types.Interfaces.Data;

    /// <summary>
    /// The board.
    /// </summary>
    [Serializable]
    public partial class Board : IEntity, IHaveID
    {
        /// <summary>
        /// The on created.
        /// </summary>
        partial void OnCreated();

        /// <summary>
        /// Initializes a new instance of the <see cref="Board"/> class.
        /// </summary>
        public Board()
        {
            this.OnCreated();
        }

        #region Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [AutoIncrement]
        [Alias("BoardID")]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the membership app name.
        /// </summary>
        [StringLength(255)]
        public string MembershipAppName { get; set; }

        /// <summary>
        /// Gets or sets the roles app name.
        /// </summary>
        [StringLength(255)]
        public string RolesAppName { get; set; }

        #endregion
    }
}