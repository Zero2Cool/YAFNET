<%@ Control Language="C#" AutoEventWireup="true" EnableViewState="false" Inherits="YAF.Controls.ForumLastPost" CodeBehind="ForumLastPost.ascx.cs" %>

<asp:PlaceHolder ID="LastPostedHolder" runat="server">
    <div class="forumLatestContainer alert-warning">
    
        <div class="forumLastTopic">
            <asp:PlaceHolder ID="TopicInPlaceHolder" runat="server">
                <asp:Label runat="server" ID="NewMessage">&nbsp;</asp:Label><asp:HyperLink ID="topicLink" runat="server"></asp:HyperLink>
            </asp:PlaceHolder>
        </div>

        <div class=".d-none .d-sm-block">
            <div class="forumLastPostedBy">
                <YAF:ThemeButton runat="server" ID="LastTopicImgLink" Size="Small" Icon="share-square" Type="None"></YAF:ThemeButton>
                <YAF:ThemeButton runat="server" ID="ImageLastUnreadMessageLink" Size="Small" Icon="book-reader" Type="None"></YAF:ThemeButton>

                <YAF:DisplayDateTime ID="LastPostDate" runat="server" Format="BothTopic" />by&nbsp;<YAF:UserLink ID="ProfileUserLink" runat="server" />
            </div>
        </div>

    </div>

  <%--  &nbsp;<span class="fa-stack">
        <i class="fa fa-calendar-day fa-stack-1x text-secondary"></i>
        <i class="fa fa-circle fa-badge-bg fa-inverse fa-outline-inverse"></i>
        <i class="fa fa-clock fa-badge text-secondary"></i>
    </span>&nbsp;
       --%>
    
</asp:PlaceHolder>

<asp:PlaceHolder runat="server" ID="NoPostsPlaceHolder">
    <YAF:Alert runat="server" Type="info">
        <YAF:LocalizedLabel ID="NoPostsLabel" runat="server" LocalizedTag="NO_POSTS" />
    </YAF:Alert>
</asp:PlaceHolder>
