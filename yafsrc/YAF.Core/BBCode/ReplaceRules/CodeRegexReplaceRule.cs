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
namespace YAF.Core.BBCode.ReplaceRules
{
    using System.Text.RegularExpressions;

    using YAF.Types.Interfaces;

    /// <summary>
    /// Simple code block regular express replace
    /// </summary>
    public class CodeRegexReplaceRule : SimpleRegexReplaceRule
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeRegexReplaceRule"/> class.
        /// </summary>
        /// <param name="regExSearch">
        /// The reg ex search.
        /// </param>
        /// <param name="regExReplace">
        /// The reg ex replace.
        /// </param>
        public CodeRegexReplaceRule(Regex regExSearch, string regExReplace)
            : base(regExSearch, regExReplace)
        {
            // default high rank...
            this.RuleRank = 2;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// The replace.
        /// </summary>
        /// <param name="text">
        /// The text.
        /// </param>
        /// <param name="replacement">
        /// The replacement.
        /// </param>
        public override void Replace(ref string text, IReplaceBlocks replacement)
        {
            var m = this._regExSearch.Match(text);
            while (m.Success)
            {
                var replaceItem = this._regExReplace.Replace("${inner}", this.GetInnerValue(m.Groups["inner"].Value));

                var replaceIndex = replacement.Add(replaceItem);
                text = text.Substring(0, m.Groups[0].Index) + replacement.Get(replaceIndex)
                                                            + text.Substring(m.Groups[0].Index + m.Groups[0].Length);

                m = this._regExSearch.Match(text);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// This just overrides how the inner value is handled
        /// </summary>
        /// <param name="innerValue">The inner value.</param>
        /// <returns>
        /// The get inner value.
        /// </returns>
        protected override string GetInnerValue(string innerValue)
        {
            innerValue = innerValue.Replace("\t", "&nbsp; &nbsp;&nbsp;");
            innerValue = innerValue.Replace("[", "&#91;");
            innerValue = innerValue.Replace("]", "&#93;");
            innerValue = innerValue.Replace("<", "&lt;");
            innerValue = innerValue.Replace(">", "&gt;");
            innerValue = innerValue.Replace("\r\n", "<br />");
            return innerValue;
        }

        #endregion
    }
}