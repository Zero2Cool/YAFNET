<%@ Control Language="c#" AutoEventWireup="True" Inherits="YAF.Controls.ForumList"
    EnableViewState="false" CodeBehind="ForumList.ascx.cs" %>
<%@ Import Namespace="YAF.Types.Extensions" %>
<%@ Import Namespace="YAF.Utils.Helpers" %>
<%@ Import Namespace="YAF.Core.Extensions" %>
<%@ Register TagPrefix="YAF" TagName="ForumLastPost" Src="ForumLastPost.ascx" %>
<%@ Register TagPrefix="YAF" TagName="ForumModeratorList" Src="ForumModeratorList.ascx" %>
<%@ Register TagPrefix="YAF" TagName="ForumSubForumList" Src="ForumSubForumList.ascx" %>

<asp:Repeater ID="ForumList1" runat="server" OnItemCreated="ForumList1_ItemCreated">
    <SeparatorTemplate>
        <div class="row">
            <div class="col">
                <hr />
            </div>
        </div>
    </SeparatorTemplate>
    <ItemTemplate>
        <div class="row">
            <div class='<%# ((System.Data.DataRow)Container.DataItem)["RemoteURL"].IsNullOrEmptyDBField() ? "col-md-8" : "col" %>'>

                <asp:PlaceHolder runat="server" ID="ForumIcon"></asp:PlaceHolder>
                <asp:Image ID="ForumImage1" Visible="false" runat="server" />
                <span class="forumTitle">
                <%# this.GetForumLink((System.Data.DataRow)Container.DataItem) %>
                </span>
                <asp:Label CssClass="badge badge-light" runat="server"
                    Visible='<%# ((System.Data.DataRow)Container.DataItem)["Viewing"].ToType<int>() > 0 %>'>
                        <%# this.GetViewing((System.Data.DataRow)Container.DataItem) %>
                </asp:Label>

                <div class="forumDescription">
                    <asp:Label runat="server" ID="Description" Visible='<%# DataBinder.Eval(Container.DataItem, "[\"Description\"]").ToString().IsSet() %>'> <%# this.Page.HtmlEncode(DataBinder.Eval(Container.DataItem, "[\"Description\"]")) %> </asp:Label>
                </div>

                <YAF:ForumModeratorList ID="ForumModeratorListMob" Visible="false" runat="server" />

                <YAF:ForumSubForumList ID="SubForumList" runat="server"
                    DataSource="<%# this.GetSubForums((System.Data.DataRow)Container.DataItem ) %>"
                    Visible="<%# this.HasSubForums((System.Data.DataRow)Container.DataItem) %>" />
            </div>
            <asp:PlaceHolder runat="server" Visible='<%# ((System.Data.DataRow)Container.DataItem)["RemoteURL"].IsNullOrEmptyDBField() %>'>
                <div class="col-md-2 text-secondary d-none">
                    <div class="d-flex flex-row flex-md-column justify-content-between justify-content-md-start">
                        <div>
                            <YAF:LocalizedLabel ID="LocalizedLabel2" runat="server"
                                LocalizedTag="TOPICS" />
                            :
                            <%# this.Topics((System.Data.DataRow)Container.DataItem) %>
                        </div>
                        <div>
                            <YAF:LocalizedLabel ID="LocalizedLabel3" runat="server"
                                LocalizedTag="POSTS" />
                            :
                            <%# this.Posts((System.Data.DataRow)Container.DataItem) %>
                        </div>
                    </div>
                </div>
                <div class="col-md-4 text-secondary">
                    <YAF:ForumLastPost ID="lastPost" runat="server" DataRow="<%# (System.Data.DataRow)Container.DataItem %>" />
                </div>
            </asp:PlaceHolder>
        </div>
    </ItemTemplate>
</asp:Repeater>
