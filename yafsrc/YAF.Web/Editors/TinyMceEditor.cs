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
namespace YAF.Web.Editors
{
    #region Using

    using System;

    using YAF.Core;
    using YAF.Types;
    using YAF.Types.Interfaces;

    #endregion

    /// <summary>
    /// The TinyMCE editor.
    /// </summary>
    public abstract class TinyMceEditor : TextEditor
    {
        #region Properties

        /// <summary>
        ///   Gets or sets Text.
        /// </summary>
        public override string Text
        {
            get => this.TextAreaControl.InnerText;

            set => this.TextAreaControl.InnerText = value;
        }

        /// <summary>
        ///   Gets SafeID.
        /// </summary>
        [NotNull]
        protected new string SafeID => this.TextAreaControl.ClientID.Replace("$", "_");

        #endregion

        #region Methods

        /// <summary>
        /// Load Editor Java Scripts
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected override void Editor_PreRender([NotNull] object sender, [NotNull] EventArgs e)
        {
            YafContext.Current.PageElements.AddScriptReference(
                "tinymce", "tinymce/tinymce.min.js");

            this.RegisterTinyMceCustomJS();

            // register custom YafBBCode javascript (if there is any)
            // this call is supposed to be after editor load since it may use
            // JS variables created in editor_load...
            this.Get<IBBCode>().RegisterCustomBBCodePageElements(this.Page, this.GetType(), this.SafeID);
        }

        /// <summary>
        /// Adds Width and Height to the Editor
        /// </summary>
        /// <param name="e">The e.</param>
        protected override void OnInit([NotNull] EventArgs e)
        {
            base.OnInit(e);

            this.TextAreaControl.Attributes.CssStyle.Add("width", "100%");
            this.TextAreaControl.Attributes.CssStyle.Add("height", "350px");
        }

        /// <summary>
        /// Registers the TinyMCE custom JS.
        /// </summary>
        protected abstract void RegisterTinyMceCustomJS();

        #endregion
    }
}