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

namespace YAF.Pages.Admin
{
    #region Using

    using System;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Web.UI.WebControls;

    using FarsiLibrary.Utils;

    using YAF.Configuration;
    using YAF.Core.BasePages;
    using YAF.Core.Extensions;
    using YAF.Core.Model;
    using YAF.Core.Utilities;
    using YAF.Types;
    using YAF.Types.Constants;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Utils;
    using YAF.Web.Extensions;

    #endregion

    /// <summary>
    /// The Admin Event Log Page.
    /// </summary>
    public partial class EventLog : AdminPage
    {
        #region Methods

        /// <summary>
        /// Delete Selected Event Log Entry
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void DeleteAllClick([NotNull] object sender, [NotNull] EventArgs e)
        {
            this.GetRepository<Types.Models.EventLog>()
                .DeleteAll();

            // re-bind controls
            this.BindData();
        }

        /// <summary>
        /// Gets HTML IMG code representing given log event icon.
        /// </summary>
        /// <param name="dataRow">
        /// Data row containing event log entry data.
        /// </param>
        /// <returns>
        /// return HTML code of event log entry image
        /// </returns>
        protected string EventIcon([NotNull] object dataRow)
        {
            // cast object to the DataRowView
            var row = (DataRowView)dataRow;

            string cssClass, icon;

            EventLogTypes eventType;

            try
            {
                // find out of what type event log entry is
                eventType = row["Type"].ToEnum<EventLogTypes>();
            }
            catch (Exception)
            {
                eventType = EventLogTypes.Information;
            }

            switch (eventType)
            {
                case EventLogTypes.Error:
                    icon = "radiation";
                    cssClass = "danger";
                    break;
                case EventLogTypes.Warning:
                    icon = "exclamation-triangle";
                    cssClass = "warning";
                    break;
                case EventLogTypes.Information:
                    icon = "exclamation";
                    cssClass = "info";
                    break;
                case EventLogTypes.Debug:
                    icon = "exclamation-triangle"; 
                    cssClass = "warning";
                    break;
                case EventLogTypes.Trace:
                    icon = "exclamation-triangle";
                    cssClass = "warning";
                    break;
                case EventLogTypes.SqlError:
                    icon = "exclamation-triangle";
                    cssClass = "warning";
                    break;
                case EventLogTypes.UserSuspended:
                    icon = "user-clock";
                    cssClass = "warning";
                    break;
                case EventLogTypes.UserUnsuspended:
                    icon = "user-check";
                    cssClass = "info";
                    break;
                case EventLogTypes.UserDeleted:
                    icon = "user-alt-slash";
                    cssClass = "danger";
                    break;
                case EventLogTypes.IpBanSet:
                    icon = "hand-paper";
                    cssClass = "warning";
                    break;
                case EventLogTypes.IpBanLifted:
                    icon = "slash";
                    cssClass = "success";
                    break;
                case EventLogTypes.IpBanDetected:
                    icon = "hand-paper";
                    cssClass = "warning";
                    break;
                case EventLogTypes.SpamBotReported:
                    icon = "user-ninja";
                    cssClass = "warning";
                    break;
                case EventLogTypes.SpamBotDetected:
                    icon = "user-lock";
                    cssClass = "warning";
                    break;
                case EventLogTypes.SpamMessageReported:
                    icon = "flag";
                    cssClass = "success";
                    break;
                case EventLogTypes.SpamMessageDetected:
                    icon = "shield-alt";
                    cssClass = "warning";
                    break;
                default:
                    icon = "exclamation-circle";
                    cssClass = "primary";
                    break;
            }

            return $@"<i class=""fas fa-{icon} text-{cssClass}""></i>";
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit([NotNull] EventArgs e)
        {
            this.List.ItemCommand += this.ListItemCommand;

            base.OnInit(e);
        }

        /// <summary>
        /// The On PreRender event.
        /// </summary>
        /// <param name="e">
        /// the Event Arguments
        /// </param>
        protected override void OnPreRender([NotNull] EventArgs e)
        {
            // setup jQuery and DatePicker JS...
            this.PageContext.PageElements.RegisterJsBlockStartup(
                "DatePickerJs",
                JavaScriptBlocks.DatePickerLoadJs(
                    this.GetText("COMMON", "CAL_JQ_CULTURE_DFORMAT"),
                    this.GetText("COMMON", "CAL_JQ_CULTURE")));

            this.PageContext.PageElements.RegisterJsBlock("dropDownToggleJs", JavaScriptBlocks.DropDownToggleJs());

            this.PageContext.PageElements.RegisterJsBlock(
                "collapseToggleJs",
                JavaScriptBlocks.CollapseToggleJs(
                    this.GetText("ADMIN_EVENTLOG", "HIDE"),
                    this.GetText("ADMIN_EVENTLOG", "SHOW")));

            base.OnPreRender(e);
        }

        /// <summary>
        /// Handles the Load event of the Page control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void Page_Load([NotNull] object sender, [NotNull] EventArgs e)
        {
            // do it only once, not on post-backs
            if (this.IsPostBack)
            {
                return;
            }

            var allItem = new ListItem(this.GetText("ALL"), "-1");

            allItem.Attributes.Add(
                "data-content",
                $"<span class=\"select2-image-select-icon\"><i class=\"fas fa-filter fa-fw text-secondary\"></i>&nbsp;{this.GetText("ALL")}</span>");

            this.Types.Items.Add(allItem);

            EnumExtensions.GetAllItems<EventLogTypes>().ForEach(
                type =>
                    {
                        var icon = type switch
                            {
                                EventLogTypes.Error => "radiation",
                                EventLogTypes.Warning => "exclamation-triangle",
                                EventLogTypes.Information => "exclamation",
                                EventLogTypes.Debug => "exclamation-triangle",
                                EventLogTypes.Trace => "exclamation-triangle",
                                EventLogTypes.SqlError => "exclamation-triangle",
                                EventLogTypes.UserSuspended => "user-clock",
                                EventLogTypes.UserUnsuspended => "user-check",
                                EventLogTypes.UserDeleted => "user-alt-slash",
                                EventLogTypes.IpBanSet => "hand-paper",
                                EventLogTypes.IpBanLifted => "slash",
                                EventLogTypes.IpBanDetected => "hand-paper",
                                EventLogTypes.SpamBotReported => "user-ninja",
                                EventLogTypes.SpamBotDetected => "user-lock",
                                EventLogTypes.SpamMessageReported => "flag",
                                EventLogTypes.SpamMessageDetected => "shield-alt",
                                _ => "exclamation-circle"
                            };

                        var item = new ListItem { Value = type.ToInt().ToString(), Text = type.ToString() };

                        item.Attributes.Add(
                            "data-content",
                            $"<span class=\"select2-image-select-icon\"><i class=\"fas fa-{icon} fa-fw text-secondary\"></i>&nbsp;{type}</span>");

                        this.Types.Items.Add(item);
                    });

            var ci = this.Get<ILocalization>().Culture;

            if (this.Get<BoardSettings>().UseFarsiCalender && ci.IsFarsiCulture())
            {
                this.SinceDate.Text = PersianDateConverter.ToPersianDate(PersianDate.MinValue).ToString("d");
                this.ToDate.Text = PersianDateConverter.ToPersianDate(PersianDate.Now).ToString("d");
            }
            else
            {
                this.SinceDate.Text = DateTime.UtcNow.AddDays(-this.Get<BoardSettings>().EventLogMaxDays).ToString(
                                             ci.DateTimeFormat.ShortDatePattern, CultureInfo.InvariantCulture);
                this.ToDate.Text = DateTime.UtcNow.Date.ToString(
                                             ci.DateTimeFormat.ShortDatePattern, CultureInfo.InvariantCulture);
            }

            this.ToDate.ToolTip = this.SinceDate.ToolTip = this.GetText("COMMON", "CAL_JQ_TT");

            // bind data to controls
            this.BindData();
        }

        /// <summary>
        /// Creates page links for this page.
        /// </summary>
        protected override void CreatePageLinks()
        {
            this.PageLinks.AddRoot();

            // administration index second
            this.PageLinks.AddAdminIndex();

            this.PageLinks.AddLink(this.GetText("ADMIN_EVENTLOG", "TITLE"), string.Empty);

            this.Page.Header.Title =
                $"{this.GetText("ADMIN_ADMIN", "Administration")} - {this.GetText("ADMIN_EVENTLOG", "TITLE")}";

            this.PagerTop.PageSize = 25;
        }

        /// <summary>
        /// The pager top_ page change.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void PagerTopPageChange([NotNull] object sender, [NotNull] EventArgs e)
        {
            // rebind
            this.BindData();
        }

        /// <summary>
        /// Handles the Click event of the ApplyButton control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="eventArgs">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected void ApplyButtonClick([NotNull] object source, EventArgs eventArgs)
        {
            this.BindData();
        }

        /// <summary>
        /// Populates data source and binds data to controls.
        /// </summary>
        private void BindData()
        {
            var baseSize = this.Get<BoardSettings>().MemberListPageSize;
            var currentPageIndex = this.PagerTop.CurrentPageIndex;
            this.PagerTop.PageSize = baseSize;

            var sinceDate = DateTime.UtcNow.AddDays(-this.Get<BoardSettings>().EventLogMaxDays);
            var toDate = DateTime.UtcNow;

            var ci = this.Get<ILocalization>().Culture;

            if (this.SinceDate.Text.IsSet())
            {
                if (this.Get<BoardSettings>().UseFarsiCalender && ci.IsFarsiCulture())
                {
                    var persianDate = new PersianDate(this.SinceDate.Text);

                    sinceDate = PersianDateConverter.ToGregorianDateTime(persianDate);
                }
                else
                {
                    DateTime.TryParse(this.SinceDate.Text, ci, DateTimeStyles.None, out sinceDate);
                }
            }

            if (this.ToDate.Text.IsSet())
            {
                if (this.Get<BoardSettings>().UseFarsiCalender && ci.IsFarsiCulture())
                {
                    var persianDate = new PersianDate(this.ToDate.Text);

                    toDate = PersianDateConverter.ToGregorianDateTime(persianDate);
                }
                else
                {
                    DateTime.TryParse(this.ToDate.Text, ci, DateTimeStyles.None, out toDate);
                }
            }

            // list event for this board
            var dt = this.GetRepository<Types.Models.EventLog>()
                               .List(
                                   this.PageContext.PageUserID,
                                   this.Get<BoardSettings>().EventLogMaxMessages,
                                   this.Get<BoardSettings>().EventLogMaxDays,
                                   currentPageIndex,
                                   baseSize,
                                   sinceDate,
                                   toDate.AddDays(1).AddMinutes(-1),
                                   this.Types.SelectedValue.Equals("-1") ? null : this.Types.SelectedValue);

            this.List.DataSource = dt;

            this.PagerTop.Count = dt != null && dt.HasRows()
                                      ? dt.AsEnumerable().First().Field<int>("TotalRows")
                                      : 0;

            // bind data to controls
            this.DataBind();

            if (this.List.Items.Count == 0)
            {
                this.NoInfo.Visible = true;
            }
        }

        /// <summary>
        /// Handles single record commands in a repeater.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.RepeaterCommandEventArgs"/> instance containing the event data.</param>
        private void ListItemCommand([NotNull] object source, [NotNull] RepeaterCommandEventArgs e)
        {
            // what command are we serving?
            switch (e.CommandName)
            {
                // delete log entry
                case "delete":

                    // delete just this particular log entry
                    this.GetRepository<Types.Models.EventLog>().DeleteById(e.CommandArgument.ToType<int>());

                    // re-bind controls
                    this.BindData();
                    break;
            }
        }

        #endregion
    }
}