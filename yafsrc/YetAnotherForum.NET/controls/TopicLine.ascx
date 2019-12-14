<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="TopicLine.ascx.cs" Inherits="YAF.Controls.TopicLine" %>

<%@ Import Namespace="YAF.Utils.Helpers" %>
<%@ Import Namespace="YAF.Types.Interfaces" %>
<%@ Import Namespace="YAF.Types.Extensions" %>

<div class="row">
    <div class="col-lg-8 col-sm-8 col-md-8 col-12">
        <asp:PlaceHolder ID="SelectionHolder" runat="server" Visible="false">
            <asp:CheckBox ID="chkSelected" runat="server" Text="&nbsp;" CssClass="custom-control custom-checkbox d-inline-flex" />
        </asp:PlaceHolder>

        <asp:Label runat="server" ID="TopicIcon"></asp:Label>
        <asp:Label runat="server" ID="Priority" Visible="False"></asp:Label>
        <span class="topicTitle">
            <asp:HyperLink runat="server" ID="TopicLink"></asp:HyperLink>
        </span>
        <asp:Label runat="server" ID="FavoriteCount"></asp:Label>

        <asp:Label runat="server" ID="Description" CssClass="font-italic"></asp:Label>
        <p class="card-text">
            Started by&nbsp;<YAF:UserLink runat="server" ID="topicStarterLink"></YAF:UserLink>
            <span class="fa-stack">
                <i class="fa fa-calendar-day fa-stack-1x text-secondary"></i>
                <i class="fa fa-circle fa-badge-bg fa-inverse fa-outline-inverse"></i>
                <i class="fa fa-clock fa-badge text-secondary"></i>
            </span>&nbsp;<YAF:DisplayDateTime runat="server" ID="StartDate">
            </YAF:DisplayDateTime>
            <%
                var actualPostCount = this.TopicRow["Replies"].ToType<int>() + 1;

                if (this.Get<YafBoardSettings>().ShowDeletedMessages)
                {
                    // add deleted posts not included in replies...
                    actualPostCount += this.TopicRow["NumPostsDeleted"].ToType<int>();
                }     

                var tPager = this.CreatePostPager(
                    actualPostCount, this.Get<YafBoardSettings>().PostsPerPage, this.TopicRow["LinkTopicID"].ToType<int>());

                if (tPager != string.Empty)
                {
                    var altMultipages = string.Format(this.GetText("GOTO_POST_PAGER"), string.Empty);
            %>
            <span><%=tPager%></span>
            <%
           }
            %>
        </p>
    </div>

    <asp:PlaceHolder runat="server" Visible='<%# !this.TopicRow["LastMessageID"].IsNullOrEmptyDBField() %>'>

        <div class="col-lg-1 col-md-1 col-sm-1 d-none d-sm-block d-sm-none d-md-block">
            <div class="topicStatsContainer">
                <div class="topicReplies">Replies: <%=this.FormatReplies()%></div>
                <div class="topicViews">Views: <%=this.FormatViews()%></div>
            </div>
        </div>

        <div class="col-lg-3 col-md-3 col-sm-3 col-md-3 col-12">
            Last post by
            <YAF:UserLink runat="server" ID="UserLast"></YAF:UserLink>
            <br />
            <YAF:ThemeButton runat="server" CssClass="mt-1 mr-1 btn-xs" ID="GoToLastPost" Size="Small" Icon="share-square" Type="OutlineSuccess"></YAF:ThemeButton>
            <YAF:ThemeButton runat="server" CssClass="mt-1 mr-1 btn-xs" ID="GoToLastUnread" Size="Small" Icon="book-reader" Type="OutlineSuccess"></YAF:ThemeButton>
            <YAF:DisplayDateTime runat="server" ID="LastDate"></YAF:DisplayDateTime>
        </div>
    </asp:PlaceHolder>
</div>
