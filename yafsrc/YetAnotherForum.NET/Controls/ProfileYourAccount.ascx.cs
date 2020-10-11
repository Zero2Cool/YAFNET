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

  using YAF.Core.BaseControls;
  using YAF.Core.Extensions;
  using YAF.Core.Model;
  using YAF.Types;
  using YAF.Types.Extensions;
  using YAF.Types.Interfaces;
  using YAF.Types.Models;

  #endregion

  /// <summary>
  /// The profile your account.
  /// </summary>
  public partial class ProfileYourAccount : BaseUserControl
  {
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
      if (!this.IsPostBack)
      {
        this.BindData();
      }
    }

    /// <summary>
    /// The bind data.
    /// </summary>
    private void BindData()
    {
      var groups = this.GetRepository<UserGroup>().List(this.PageContext.PageUserID);

      this.Groups.DataSource = groups;

      this.DataBind();

      // TitleUserName.Text = HtmlEncode( userData.Membership.UserName );
      this.AccountEmail.Text = this.PageContext.User.Email;
      this.Name.Text = this.HtmlEncode(this.PageContext.User.Name);
      this.Joined.Text = this.Get<IDateTime>().FormatDateTime(this.PageContext.User.Joined);
      this.NumPosts.Text = $"{this.PageContext.User.NumPosts:N0}";

      this.DisplayNameHolder.Visible = this.PageContext.BoardSettings.EnableDisplayName;

      if (this.PageContext.BoardSettings.EnableDisplayName)
      {
        this.DisplayName.Text =
          this.HtmlEncode(this.PageContext.User.DisplayOrUserName());
      }

      var avatarImg = this.Get<IAvatars>().GetAvatarUrlForCurrentUser();

      if (avatarImg.IsSet())
      {
        this.AvatarImage.ImageUrl = avatarImg; 
        this.AvatarImage.Attributes.CssStyle.Add("max-width", this.PageContext.BoardSettings.AvatarWidth.ToString());
        this.AvatarImage.Attributes.CssStyle.Add("max-height", this.PageContext.BoardSettings.AvatarHeight.ToString());
      }
      else
      {
        this.AvatarImage.Visible = false;
      }
    }

    #endregion
  }
}