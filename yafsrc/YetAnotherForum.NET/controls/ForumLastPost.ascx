<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" Inherits="YAF.Controls.ForumLastPost" CodeBehind="ForumLastPost.ascx.cs" %>

<asp:PlaceHolder ID="LastPostedHolder" runat="server">
    <h6>
        <asp:PlaceHolder ID="TopicInPlaceHolder" runat="server">
            <YAF:LocalizedLabel runat="server" LocalizedTag="LASTPOST"></YAF:LocalizedLabel>:
            <asp:HyperLink ID="topicLink" runat="server"></asp:HyperLink>
        </asp:PlaceHolder>
        <asp:Label runat="server" ID="NewMessage"></asp:Label>
        <br/>
        <YAF:ThemeButton runat="server" ID="LastTopicImgLink" 
                         Size="Small"
                         Icon="share-square"
                         Type="OutlineSecondary"
                         DataToggle="tooltip"
                         TitleLocalizedTag="GO_LAST_POST"
                         CssClass="mt-1 mr-1"></YAF:ThemeButton>
        <YAF:ThemeButton runat="server" ID="ImageLastUnreadMessageLink" 
                         Size="Small"
                         Icon="book-reader"
                         Type="OutlineSecondary"
                         DataToggle="tooltip"
                         TitleLocalizedTag="GO_LASTUNREAD_POST"
                         CssClass="mt-1"></YAF:ThemeButton>
    </h6>
    <hr/>
    <h6><YAF:UserLink ID="ProfileUserLink" runat="server" />

        &nbsp;<span class="fa-stack">
            <i class="fa fa-calendar-day fa-stack-1x text-secondary"></i>
            <i class="fa fa-circle fa-badge-bg fa-inverse fa-outline-inverse"></i>
            <i class="fa fa-clock fa-badge text-secondary"></i>
        </span>&nbsp;
        <YAF:DisplayDateTime ID="LastPostDate" runat="server" Format="BothTopic" />
    </h6>
</asp:PlaceHolder>

<asp:PlaceHolder runat="server" ID="NoPostsPlaceHolder">
    <YAF:Alert runat="server" Type="info">
        <YAF:LocalizedLabel ID="NoPostsLabel" runat="server" LocalizedTag="NO_POSTS" />
    </YAF:Alert>
</asp:PlaceHolder>
