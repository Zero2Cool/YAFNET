<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false"
Inherits="YAF.Controls.ForumSubForumList" Codebehind="ForumSubForumList.ascx.cs" %>


<asp:Repeater ID="SubforumList" runat="server" OnItemCreated="SubForumList_ItemCreated">
    <HeaderTemplate>
        <div class="subForumList"> 
            <span class="font-weight-bold small">
                <YAF:LocalizedLabel ID="SubForums" LocalizedTag="SUBFORUMS" runat="server" />:
            </span>
        
    </HeaderTemplate>
    <ItemTemplate>
        
            <asp:PlaceHolder ID="ForumIcon" runat="server" />&nbsp;<%#  this.GetForumLink((System.Data.DataRow)Container.DataItem) %>
        
    </ItemTemplate>
    <FooterTemplate>
        <asp:Label Text="..." Visible="false" ID="CutOff" runat="server" />
    
    </div>
    </FooterTemplate>
</asp:Repeater>