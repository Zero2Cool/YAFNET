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

    using ServiceStack.DataAnnotations;

    using YAF.Types.Interfaces;
    using YAF.Types.Interfaces.Data;

    /// <summary>
    /// A class which represents the BBCode table.
    /// </summary>
    [Serializable]
    public class BBCode : IEntity, IHaveBoardID, IHaveID
    {
        #region Properties

        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        [AutoIncrement]
        [Alias("BBCodeID")]
        public int ID { get; set; }

        /// <summary>
        /// Gets or sets the board id.
        /// </summary>
        [References(typeof(Board))]
        [Required]
        public int BoardID { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [StringLength(4000)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the on click js.
        /// </summary>
        [StringLength(1000)]
        public string OnClickJS { get; set; }

        /// <summary>
        /// Gets or sets the display js.
        /// </summary>
        public string DisplayJS { get; set; }

        /// <summary>
        /// Gets or sets the edit js.
        /// </summary>
        public string EditJS { get; set; }

        /// <summary>
        /// Gets or sets the display css.
        /// </summary>
        public string DisplayCSS { get; set; }

        /// <summary>
        /// Gets or sets the search regex.
        /// </summary>
        public string SearchRegex { get; set; }

        /// <summary>
        /// Gets or sets the replace regex.
        /// </summary>
        public string ReplaceRegex { get; set; }

        /// <summary>
        /// Gets or sets the variables.
        /// </summary>
        [StringLength(1000)]
        public string Variables { get; set; }

        /// <summary>
        /// Gets or sets the use module.
        /// </summary>
        public bool? UseModule { get; set; }

        /// <summary>
        /// Gets or sets the use toolbar.
        /// </summary>
        public bool? UseToolbar { get; set; }

        /// <summary>
        /// Gets or sets the module class.
        /// </summary>
        [StringLength(255)]
        public string ModuleClass { get; set; }

        /// <summary>
        /// Gets or sets the exec order.
        /// </summary>
        [Required]
        public int ExecOrder { get; set; }

        #endregion
    }
}