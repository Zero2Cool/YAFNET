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
namespace YAF.Web.BBCodes
{
    using System.Web.UI;

    using YAF.Configuration;
    using YAF.Core.BBCode;
    using YAF.Types.Interfaces;
    using YAF.Utils;

    /// <summary>
    /// The Album Image BB Code Module.
    /// </summary>
    public class AlbumImage : BBCodeControl
    {
        /// <summary>
        /// Render The Album Image as Link with Image
        /// </summary>
        /// <param name="writer">
        /// The writer.
        /// </param>
        protected override void Render(HtmlTextWriter writer)
        {
            writer.Write(
                @"<a href=""{0}resource.ashx?image={1}"" class=""attachedImage"" data-gallery title=""{1}"">",
                BoardInfo.ForumClientFileRoot,
                this.Parameters["inner"]);

            writer.Write(
                @"<img src=""{0}resource.ashx?imgprv={1}"" class=""img-user-posted img-thumbnail"" style=""max-width:auto;max-height:{2}px"" alt=""{1}"" />",
                BoardInfo.ForumClientFileRoot,
                this.Parameters["inner"],
                this.Get<BoardSettings>().ImageThumbnailMaxHeight);

            writer.Write("</a>");
        }
    }
}