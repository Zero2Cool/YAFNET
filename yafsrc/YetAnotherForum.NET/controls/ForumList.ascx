<%@ Control Language="c#" AutoEventWireup="True" Inherits="YAF.Controls.ForumList" EnableViewState="false" CodeBehind="ForumList.ascx.cs" %>
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
                <hr/>
            </div>
        </div>
    </SeparatorTemplate>
	<ItemTemplate>        
        <div class="row">
            <div class='<%# ((System.Data.DataRow)Container.DataItem)["RemoteURL"].IsNullOrEmptyDBField() ? "col-md-6" : "col" %>'>
                <h5>
                    <asp:PlaceHolder runat="server" ID="ForumIcon"></asp:PlaceHolder>
                    <asp:Image id="ForumImage1" Visible="false" runat="server" />
          
                    <%# this.GetForumLink((System.Data.DataRow)Container.DataItem) %>
            
                    <asp:Label CssClass="badge badge-light" runat="server" 
                               Visible='<%# ((System.Data.DataRow)Container.DataItem)["Viewing"].ToType<int>() > 0 %>'>
                        <%# this.GetViewing((System.Data.DataRow)Container.DataItem) %>
                    </asp:Label>
                    <YAF:ForumModeratorList ID="ForumModeratorListMob" Visible="false" runat="server"  />
                </h5>
                <YAF:ForumSubForumList ID="SubForumList" runat="server"
                                       DataSource='<%# this.GetSubForums((System.Data.DataRow)Container.DataItem ) %>'
                                       Visible='<%# this.HasSubForums((System.Data.DataRow)Container.DataItem) %>' />
            </div>
            <asp:PlaceHolder runat="server" Visible='<%# ((System.Data.DataRow)Container.DataItem)["RemoteURL"].IsNullOrEmptyDBField() %>'>
                <div class="col-md-2">
                    <div class="d-flex flex-row flex-md-column justify-content-between justify-content-md-start">
                        <div>
                            <YAF:LocalizedLabel ID="LocalizedLabel2" runat="server"
                                                LocalizedTag="TOPICS" />:
                            <%# this.Topics((System.Data.DataRow)Container.DataItem) %>
                        </div>
                        <div>
                            <YAF:LocalizedLabel ID="LocalizedLabel3" runat="server"
                                                LocalizedTag="POSTS" />:
                            <%# this.Posts((System.Data.DataRow)Container.DataItem) %>
                        </div>
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
