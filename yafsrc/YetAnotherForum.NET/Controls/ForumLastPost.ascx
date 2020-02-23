<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" Inherits="YAF.Controls.ForumLastPost" CodeBehind="ForumLastPost.ascx.cs" %>

<asp:PlaceHolder ID="LastPostedHolder" runat="server">
    <div class="d-flex flex-row flex-md-column justify-content-between justify-content-md-start">
        <div class="forumLatestContainer">
            <div class="forumLastTopic">
                <asp:Label runat="server" ID="NewMessage"
                    CssClass="mr-1"></asp:Label>
                <asp:PlaceHolder ID="TopicInPlaceHolder" runat="server">
                    <asp:HyperLink ID="topicLink" runat="server"></asp:HyperLink>
                </asp:PlaceHolder>
            </div>


            <div class="btn-group" role="group">
                <YAF:ThemeButton runat="server" ID="ImageLastUnreadMessageLink"
                    Size="Small"
                    Icon="book-reader"
                    Type="OutlineSecondary"
                    DataToggle="tooltip"
                    CssClass="btn-xs"
                    TitleLocalizedTag="GO_LASTUNREAD_POST">
                </YAF:ThemeButton>
                <YAF:ThemeButton runat="server" ID="LastTopicImgLink"
                    Size="Small"
                    Icon="share-square"
                    Type="OutlineSecondary"
                    DataToggle="tooltip"
                    CssClass="btn-xs"
                    TitleLocalizedTag="GO_LAST_POST">
                </YAF:ThemeButton>
            </div>

            <YAF:ThemeButton runat="server" ID="Info"
                Icon="info-circle"
                IconColor="text-info"
                IconCssClass="fas fa-lg"
                Type="Link"
                DataToggle="popover"
                Size="Small"
                CssClass="topic-link-popover">
            </YAF:ThemeButton>
        </div>
    </div>
</asp:PlaceHolder>

<asp:PlaceHolder runat="server" ID="NoPostsPlaceHolder">
    <YAF:Alert runat="server" Type="info">
        <YAF:LocalizedLabel ID="NoPostsLabel" runat="server" LocalizedTag="NO_POSTS" />
    </YAF:Alert>
</asp:PlaceHolder>
