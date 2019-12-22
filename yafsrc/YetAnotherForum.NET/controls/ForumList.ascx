<%@ Control Language="c#" AutoEventWireup="True" Inherits="YAF.Controls.ForumList"
EnableViewState="false" Codebehind="ForumList.ascx.cs" %>
<%@ Import Namespace="YAF.Utils.Helpers" %>
<%@ Import Namespace="YAF.Types.Extensions" %>
<%@ Import Namespace="System.Data" %>
<%@ Import Namespace="YAF.Core.Extensions" %>
<%@ Register TagPrefix="YAF" TagName="ForumLastPost" Src="ForumLastPost.ascx" %>
<%@ Register TagPrefix="YAF" TagName="ForumModeratorList" Src="ForumModeratorList.ascx" %>
<%@ Register TagPrefix="YAF" TagName="ForumSubForumList" Src="ForumSubForumList.ascx" %>

<asp:Repeater ID="ForumList1" runat="server" OnItemCreated="ForumList1_ItemCreated">
    <SeparatorTemplate>
        <div class="row">
            <div class="col">
                <hr/>
            </div>
        </div>
    </SeparatorTemplate>
    <ItemTemplate>
        <div class="forumContainer">

            <div class="row">

                <div class='<%# ((DataRow) Container.DataItem)["RemoteURL"].IsNullOrEmptyDBField() ? "col-md-8" : "col" %>'>
                    <span class="forumTitle">
                        <asp:PlaceHolder runat="server" ID="ForumIcon"></asp:PlaceHolder>
                        <asp:Image id="ForumImage1" Visible="false" runat="server"/>

                        <%# GetForumLink((DataRow) Container.DataItem) %>

                        <asp:Label CssClass="badge badge-light" runat="server"
                                   Visible='<%# ((DataRow) Container.DataItem)["Viewing"].ToType<int>() > 0 %>'>
                            <%# GetViewing((DataRow) Container.DataItem) %>
                        </asp:Label>
                    </span>
                    
                    <div class="forumDescription">
                        <asp:Label runat="server" ID="Description" Visible='<%# DataBinder.Eval(Container.DataItem, "[\"Description\"]").ToString().IsSet() %>'> <%# this.Page.HtmlEncode(DataBinder.Eval(Container.DataItem, "[\"Description\"]")) %> </asp:Label>
                    </div>

                    <YAF:ForumModeratorList ID="ForumModeratorListMob" Visible="false" runat="server"/>

                    <YAF:ForumSubForumList ID="SubForumList" runat="server"
                                           DataSource="<%# GetSubForums((DataRow) Container.DataItem) %>"
                                           Visible="<%# HasSubForums((DataRow) Container.DataItem) %>"/>
                </div>

                <asp:PlaceHolder runat="server" Visible='<%# ((DataRow) Container.DataItem)["RemoteURL"].IsNullOrEmptyDBField() %>'>
                    <div class="col-md-2 text-secondary d-none">
                        <div class="d-flex flex-row flex-md-column justify-content-between justify-content-md-start">
                            <div class="topicStatsContainer">
                                <div class="topicReplies">
                                    <YAF:LocalizedLabel ID="LocalizedLabel2" runat="server"
                                                        LocalizedTag="TOPICS"/>:
                                    <%# Topics((DataRow) Container.DataItem) %>
                                </div>

                                <div class="topicViews">
                                    <YAF:LocalizedLabel ID="LocalizedLabel3" runat="server"
                                                        LocalizedTag="POSTS"/>:
                                    <%# Posts((DataRow) Container.DataItem) %>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="col-md-4 text-secondary">
                        <YAF:ForumLastPost ID="lastPost" runat="server" DataRow="<%# (DataRow) Container.DataItem %>"/>
                    </div>
                </asp:PlaceHolder>
            </div>

        </div>
    </ItemTemplate>
</asp:Repeater>