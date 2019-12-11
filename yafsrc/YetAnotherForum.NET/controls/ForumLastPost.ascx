<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" Inherits="YAF.Controls.ForumLastPost" CodeBehind="ForumLastPost.ascx.cs" %>

<asp:PlaceHolder ID="LastPostedHolder" runat="server">
    <div class="forumLatestContainer alert-warning">

        <div class="forumLastTopic">
            <asp:PlaceHolder ID="TopicInPlaceHolder" runat="server">
                <strong><YAF:LocalizedLabel runat="server" LocalizedTag="LASTPOST" Visible="False"></YAF:LocalizedLabel></strong>
                <asp:Label runat="server" ID="NewMessage" CssClass="mr-1"></asp:Label>
                <asp:HyperLink ID="topicLink" runat="server" CssClass="mr-1"></asp:HyperLink>
                
            </asp:PlaceHolder>
        </div>
        
        <YAF:ThemeButton runat="server" ID="Info"
                         Icon="info-circle"
                         Type="OutlineSuccess"
                         DataToggle="popover"
                         Size="Small"
                         CssClass="mt-1 mr-1 btn-xs topic-link-popover">
        </YAF:ThemeButton>
        <YAF:ThemeButton runat="server" ID="LastTopicImgLink"
                         Size="Small"
                         Icon="share-square"
                         Type="OutlineSuccess"
                         DataToggle="tooltip"
                         TitleLocalizedTag="GO_LAST_POST"
                         CssClass="mt-1 mr-1 btn-xs">
        </YAF:ThemeButton>
        <YAF:ThemeButton runat="server" ID="ImageLastUnreadMessageLink"
                         Size="Small"
                         Icon="book-reader"
                         Type="OutlineSuccess"
                         DataToggle="tooltip"
                         TitleLocalizedTag="GO_LASTUNREAD_POST"
                         CssClass="mt-1 btn-xs">
        </YAF:ThemeButton>
    </div>

</asp:PlaceHolder>

<asp:PlaceHolder runat="server" ID="NoPostsPlaceHolder">
    <YAF:Alert runat="server" Type="info">
        <YAF:LocalizedLabel ID="NoPostsLabel" runat="server" LocalizedTag="NO_POSTS"/>
    </YAF:Alert>
</asp:PlaceHolder>