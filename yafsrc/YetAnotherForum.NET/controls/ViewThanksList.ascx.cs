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

namespace YAF.Controls
{
    #region Using

    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Web.UI.WebControls;

    using YAF.Core.BaseControls;
    using YAF.Types;
    using YAF.Types.Constants;
    using YAF.Types.Extensions;
    using YAF.Types.Interfaces;
    using YAF.Utils.Helpers;

    #endregion

    /// <summary>
    /// The View Thanks List Control
    /// </summary>
    public partial class ViewThanksList : BaseUserControl
    {
        #region Properties

        /// <summary>
        ///   Gets or sets the Thanks List Mode of the control.
        /// </summary>
        public ThanksListMode CurrentMode { get; set; }

        /// <summary>
        ///   Gets or sets the Thanks Info.
        /// </summary>
        public DataTable ThanksInfo { get; set; }

        /// <summary>
        ///   Gets or sets the User ID.
        /// </summary>
        public int UserID { get; set; }

        #endregion

        #region Public Methods

        /// <summary>
        /// The bind data.
        /// </summary>
        public void BindData()
        {
            if (!this.ThanksInfo.Columns.Contains("MessageThanksNumber"))
            {
                this.ThanksInfo.Columns.Add("MessageThanksNumber", typeof(int));
            }

            // now depending on mode filter the table
            var thanksData = this.ThanksInfo.AsEnumerable();

            if (!thanksData.Any())
            {
                this.NoResults.Visible = true;
                return;
            }

            switch (this.CurrentMode)
            {
                case ThanksListMode.FromUser:
                    thanksData = thanksData.Where(x => x.Field<int>("ThanksFromUserID") == this.UserID);
                    break;
                case ThanksListMode.ToUser:
                    {
                        thanksData.ForEach(
                            dr =>
                                {
                                    // update the message count
                                    dr["MessageThanksNumber"] =
                                        int.TryParse(dr["MessageThanksNumber"].ToString(), out _)
                                            ? dr["MessageThanksNumber"]
                                            : thanksData.Count(
                                                x => x.Field<int>("ThanksToUserID") == this.UserID
                                                     && x.Field<int>("MessageID") == (int)dr["MessageID"]);
                                });
                        
                        thanksData = thanksData.Where(x => x.Field<int>("ThanksToUserID") == this.UserID);

                        // Remove duplicates.
                        DistinctMessageID(thanksData);

                        // Sort by the ThanksNumber (Descending)
                        thanksData = thanksData.OrderByDescending(x => x.Field<int>("MessageThanksNumber"));

                        // Update the data table with changes
                        this.ThanksInfo.AcceptChanges();
                        break;
                    }
            }

            this.PagerTop.PageSize = 5;

            // set data source of repeater
            this.ThanksRes.DataSource = thanksData.ToList().GetPaged(this.PagerTop);

            // data bind controls
            this.DataBind();
        }

        #endregion

        #region Methods

        /// <summary>
        /// The page_ load.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Page_Load([NotNull] object sender, [NotNull] EventArgs e)
        {
            this.BindData();
        }

        /// <summary>
        /// The pager_ page change.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Pager_PageChange([NotNull] object sender, [NotNull] EventArgs e)
        {
            this.BindData();
        }

        /// <summary>
        /// Handles the ItemCreated event of the ThanksRes control.
        /// </summary>
        /// <param name="sender">
        /// The source of the event.
        /// </param>
        /// <param name="e">
        /// The <see cref="System.Web.UI.WebControls.RepeaterItemEventArgs"/> 
        ///   instance containing the event data.
        /// </param>
        protected void ThanksRes_ItemCreated([NotNull] object sender, [NotNull] RepeaterItemEventArgs e)
        {
            // In what mode should this control work?
            // 1: Just display the buddy list
            // 2: display the buddy list and ("Remove Buddy") buttons.
            // 3: display pending buddy list posted to current user and add ("approve","approve all", "deny",
            // "deny all","approve and add", "approve and add all") buttons.
            // 4: show the pending requests posted from the current user.
            switch (this.CurrentMode)
            {
                case ThanksListMode.FromUser:
                    if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
                    {
                        var thanksNumberCell = e.Item.FindControlAs<PlaceHolder>("ThanksNumberCell");
                        thanksNumberCell.Visible = false;
                    }

                    break;
                case ThanksListMode.ToUser:
                    if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
                    {
                        var nameCell = e.Item.FindControlAs<PlaceHolder>("NameCell");
                        nameCell.Visible = false;
                    }

                    break;
            }
        }

        /// <summary>
        /// Removes rows with duplicate MessageIDs.
        /// </summary>
        /// <param name="thanksData">
        /// The thanks data.
        /// </param>
        private static void DistinctMessageID([NotNull] IEnumerable<DataRow> thanksData)
        {
            var previousId = 0;

            foreach (var dr in thanksData.OrderBy(x => x.Field<int>("MessageID")))
            {
                var tempId = dr.Field<int>("MessageID");
                if (dr.Field<int>("MessageID") == previousId)
                {
                    dr.Delete();
                }

                previousId = tempId;
            }
        }

        #endregion
    }
}