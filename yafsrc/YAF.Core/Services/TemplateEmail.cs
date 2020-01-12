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
namespace YAF.Core.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Mail;

    using YAF.Configuration;
    using YAF.Core.Extensions;
    using YAF.Core.Model;
    using YAF.Core.Services.Localization;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Types.Models;
    using YAF.Utils;

    /// <summary>
    ///     The YAF template email.
    /// </summary>
    public class TemplateEmail : IHaveServiceLocator
    {
        #region Fields

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="TemplateEmail" /> class.
        /// </summary>
        public TemplateEmail()
            : this(null, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEmail"/> class.
        /// </summary>
        /// <param name="templateName">
        /// The template name.
        /// </param>
        public TemplateEmail(string templateName)
            : this(templateName, true)
        {
            var logoUrl =
                $"{YafForumInfo.ForumClientFileRoot}{BoardFolders.Current.Logos}/{YafContext.Current.BoardSettings.ForumLogo}";
            var themeCss =
                $"{this.Get<BoardSettings>().BaseUrlMask}{this.Get<ITheme>().BuildThemePath("bootstrap-forum.min.css")}";

            this.TemplateParams["{forumname}"] = this.Get<BoardSettings>().Name;
            this.TemplateParams["{forumlink}"] = YafForumInfo.ForumURL;
            this.TemplateParams["{themecss}"] = themeCss;
            this.TemplateParams["{logo}"] = $"{this.Get<BoardSettings>().BaseUrlMask}{logoUrl}";
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateEmail"/> class.
        /// </summary>
        /// <param name="templateName">
        /// The template name.
        /// </param>
        /// <param name="htmlEnabled">
        /// The html enabled.
        /// </param>
        public TemplateEmail(string templateName, bool htmlEnabled)
        {
            this.TemplateName = templateName;
            this.HtmlEnabled = htmlEnabled;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets a value indicating whether HtmlEnabled.
        /// </summary>
        public bool HtmlEnabled { get; set; }

        /// <summary>
        ///     Gets the service locator.
        /// </summary>
        public IServiceLocator ServiceLocator => YafContext.Current.ServiceLocator;

        /// <summary>
        ///     Gets or sets TemplateLanguageFile.
        /// </summary>
        public string TemplateLanguageFile { get; set; }

        /// <summary>
        ///     Gets or sets TemplateName.
        /// </summary>
        public string TemplateName { get; set; }

        /// <summary>
        ///     Gets or sets Template Parameter
        /// </summary>
        public IDictionary<string, string> TemplateParams { get; set; } = new Dictionary<string, string>();

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The create watch.
        /// </summary>
        /// <param name="topicId">
        /// The topic id.
        /// </param>
        /// <param name="userId">
        /// The user id.
        /// </param>
        /// <param name="fromAddress">
        /// The from address.
        /// </param>
        /// <param name="subject">
        /// The subject.
        /// </param>
        public void CreateWatch(int topicId, int userId, MailAddress fromAddress, string subject)
        {
            var textBody = this.ProcessTemplate($"{this.TemplateName}_TEXT").Trim();
            var htmlBody = this.ProcessTemplate($"{this.TemplateName}_HTML").Trim();

            // null out html if it's not desired
            if (!this.HtmlEnabled || htmlBody.IsNotSet())
            {
                htmlBody = null;
            }

            this.GetRepository<Mail>()
                .CreateWatch(topicId, fromAddress.Address, fromAddress.DisplayName, subject, textBody, htmlBody, userId);
        }

        /// <summary>
        /// The send email.
        /// </summary>
        /// <param name="toAddress">
        /// The to address.
        /// </param>
        /// <param name="subject">
        /// The subject.
        /// </param>
        /// <param name="useSendThread">
        /// The use send thread.
        /// </param>
        public void SendEmail(MailAddress toAddress, string subject, bool useSendThread)
        {
            this.SendEmail(
                new MailAddress(this.Get<BoardSettings>().ForumEmail, this.Get<BoardSettings>().Name),
                toAddress,
                subject,
                useSendThread);
        }

        /// <summary>
        /// The send email.
        /// </summary>
        /// <param name="fromAddress">
        /// The from address.
        /// </param>
        /// <param name="toAddress">
        /// The to address.
        /// </param>
        /// <param name="subject">
        /// The subject.
        /// </param>
        /// <param name="useSendThread">
        /// The use send thread.
        /// </param>
        public void SendEmail(MailAddress fromAddress, MailAddress toAddress, string subject, bool useSendThread)
        {
            var textBody = this.ProcessTemplate($"{this.TemplateName}_TEXT").Trim();
            var htmlBody = this.ProcessTemplate($"{this.TemplateName}_HTML").Trim();

            // null out html if it's not desired
            if (!this.HtmlEnabled || htmlBody.IsNotSet())
            {
                htmlBody = null;
            }

            if (useSendThread)
            {
                // create this email in the send mail table...
                this.GetRepository<Mail>()
                    .Create(
                        fromAddress.Address,
                        fromAddress.DisplayName,
                        toAddress.Address,
                        toAddress.DisplayName,
                        subject,
                        textBody,
                        htmlBody);
            }
            else
            {
                // just send directly
                this.Get<ISendMail>().Send(fromAddress, toAddress, fromAddress, subject, textBody, htmlBody);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates an email from a template
        /// </summary>
        /// <param name="templateName">
        /// The template Name.
        /// </param>
        /// <returns>
        /// The process template.
        /// </returns>
        public string ProcessTemplate(string templateName)
        {
            var email = this.ReadTemplate(templateName, this.TemplateLanguageFile);

            if (email.IsSet())
            {
                email = this.TemplateParams.Keys.Aggregate(
                    email,
                    (current, key) => current.Replace(key, this.TemplateParams[key]));
            }

            return email;
        }

        /// <summary>
        /// Reads a template from the language file
        /// </summary>
        /// <param name="templateName">
        /// The template Name.
        /// </param>
        /// <param name="templateLanguageFile">
        /// The template Language File.
        /// </param>
        /// <returns>
        /// The template
        /// </returns>
        private string ReadTemplate(string templateName, string templateLanguageFile)
        {
            if (!templateName.IsSet())
            {
                return null;
            }

            if (!templateLanguageFile.IsSet())
            {
                return this.Get<ILocalization>().GetText("TEMPLATES", templateName);
            }

            var localization = new Localization.Localization();
            localization.LoadTranslation(templateLanguageFile);
            return localization.GetText("TEMPLATES", templateName);
        }

        #endregion
    }
}