<%@ Control Language="c#" AutoEventWireup="True" Inherits="YAF.Controls.ForumList" EnableViewState="false" CodeBehind="ForumList.ascx.cs" %>
<%@ Import Namespace="YAF.Types.Extensions" %>
<%@ Import Namespace="YAF.Utils.Helpers" %>
<%@ Import Namespace="YAF.Core.Extensions" %>
<%@ Register TagPrefix="YAF" TagName="ForumLastPost" Src="ForumLastPost.ascx" %>
<%@ Register TagPrefix="YAF" TagName="ForumModeratorList" Src="ForumModeratorList.ascx" %>
<%@ Register TagPrefix="YAF" TagName="ForumSubForumList" Src="ForumSubForumList.ascx" %>

<asp:Repeater ID="ForumList1" runat="server" OnItemCreated="ForumList1_ItemCreated">
    <ItemTemplate>
        <div class="forumContainer">
            <div class="row">
                <div class="col-sm-8 col-md-8 col-12">

                    <span class="forumTitle">
                        <asp:PlaceHolder runat="server" ID="ForumIcon"></asp:PlaceHolder>
                        <img id="ForumImage1" class="" src="#" alt="image" visible="false" runat="server" style="border-width: 0" />

                        <strong>
                            <%# this.GetForumLink((System.Data.DataRow)Container.DataItem) %>
                        </strong>

                        <asp:Label CssClass="badge badge-light" runat="server" Visible='<%# ((System.Data.DataRow)Container.DataItem)["Viewing"].ToType<int>() > 0 %>'> <%# this.GetViewing((System.Data.DataRow)Container.DataItem) %> </asp:Label>
                    </span>

                    <div class="forumDescription">
                        <asp:Label runat="server" ID="Description" Visible='<%# DataBinder.Eval(Container.DataItem, "[\"Description\"]").ToString().IsSet() %>'> <%# this.Page.HtmlEncode(DataBinder.Eval(Container.DataItem, "[\"Description\"]")) %> </asp:Label>
                    </div>

                    <YAF:ForumSubForumList ID="SubForumList" runat="server" DataSource='<%# this.GetSubForums((System.Data.DataRow)Container.DataItem ) %>' Visible='<%# this.HasSubForums((System.Data.DataRow)Container.DataItem) %>' />

                    <YAF:ForumModeratorList ID="ForumModeratorListMob" Visible="false" runat="server" />
                </div>

                <asp:PlaceHolder runat="server" Visible='<%# ((System.Data.DataRow)Container.DataItem)["RemoteURL"] == DBNull.Value %>'>
                    <div class="col-sm-4 col-md-4 col-12">
                        <YAF:ForumLastPost DataRow="<%# (System.Data.DataRow)Container.DataItem %>" Visible='<%# ((System.Data.DataRow)Container.DataItem)["RemoteURL"] == DBNull.Value %>' ID="lastPost" runat="server" />
                    </div>
                </asp:PlaceHolder>
            </div>
        </div>
    </ItemTemplate>
</asp:Repeater>
