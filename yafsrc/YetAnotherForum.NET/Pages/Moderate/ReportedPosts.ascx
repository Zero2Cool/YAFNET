﻿<%@ Control Language="C#" AutoEventWireup="true" Inherits="YAF.Pages.Moderate.ReportedPosts" CodeBehind="ReportedPosts.ascx.cs" %>
<%@ Import Namespace="YAF.Types.Constants" %>
<%@ Import Namespace="ServiceStack" %>

<YAF:PageLinks runat="server" ID="PageLinks" />


<div class="row">
    <div class="col-xl-12">
        <h1><%# this.PageContext.PageForumName %> - <YAF:LocalizedLabel ID="LocalizedLabel1" runat="server" LocalizedTag="REPORTED" /></h1>
    </div>
</div>
<asp:Repeater ID="List" runat="server" OnItemDataBound="List_OnItemDataBound">
    <ItemTemplate>
        <div class="row">
            <div class="col-xl-12">
                <div class="card mb-3">
                    <div class="card-header">
                        <YAF:Icon runat="server" IconName="comment" IconType="text-secondary" />
                        <a id="TopicLink" 
                           href='<%# BuildLink.GetLink(ForumPages.Posts, "t={0}&name={1}", (Container.DataItem as dynamic).TopicID, (Container.DataItem as dynamic).TopicName) %>'
                           runat="server"><%# (Container.DataItem as dynamic).TopicName %></a>
                        <div class="float-right">
                            <span class="font-weight-bold">
                                <YAF:LocalizedLabel ID="LocalizedLabel2" runat="server" LocalizedTag="POSTED" />
                            </span>
                            <%# this.Get<IDateTime>().FormatDateShort((Container.DataItem as dynamic).Posted) %>
                            <span class="font-weight-bold pl-3">
                                <YAF:LocalizedLabel ID="LocalizedLabel3" runat="server" LocalizedTag="NUMBERREPORTED" />
                            </span>
                            <%# (Container.DataItem as dynamic).NumberOfReports %>
                            <span class="font-weight-bold pl-3">
                                <YAF:LocalizedLabel ID="LocalizedLabel5" runat="server" 
                                                    LocalizedTag="POSTEDBY" LocalizedPage="REPORTPOST" />
                            </span>
                            <YAF:UserLink ID="UserLink1" runat="server" 
                                          ReplaceName="<%# this.PageContext.BoardSettings.EnableDisplayName ? (Container.DataItem as dynamic).UserDisplayName : (Container.DataItem as dynamic).UserName %>"
                                          Suspended="<%# (Container.DataItem as dynamic).Suspended %>"
                                          Style="<%# (Container.DataItem as dynamic).UserStyle %>"
                                          UserID="<%# (Container.DataItem as dynamic).UserID %>" />
                            <YAF:ThemeButton ID="AdminUserButton" runat="server" 
                                             Size="Small"
                                             Visible="<%# this.PageContext.IsAdmin %>"
                                             TextLocalizedTag="ADMIN_USER" TextLocalizedPage="PROFILE"
                                             NavigateUrl='<%# BuildLink.GetLink( ForumPages.Admin_EditUser,"u={0}", (Container.DataItem as dynamic).UserID ) %>'
                                             Icon="users-cog" 
                                             Type="Danger">
                            </YAF:ThemeButton>
                        </div>
                    </div>
                    <div class="card-body">
                        <div class="row">
                            <div class="col">
                                <h6 class="card-subtitle mb-2 text-muted">
                                    <YAF:LocalizedLabel ID="LocalizedLabel6" runat="server" LocalizedTag="ORIGINALMESSAGE" />
                                </h6>
                                <span id="Label1" runat="server" 
                                      class="badge bg-warning text-dark" 
                                      visible="<%# General.CompareMessage((Container.DataItem as dynamic).OriginalMessage,(Container.DataItem as dynamic).Message)%>">
                                    <YAF:LocalizedLabel ID="LocalizedLabel4" runat="server" LocalizedTag="MODIFIED" />
                                </span>
                                <div class="card bg-light mb-3">
                                    <div class="card-body">
                                        <YAF:MessagePostData ID="MessagePostPrimary" runat="server" 
                                                             ShowAttachments="false" 
                                                             ShowEditMessage="False" ShowSignature="False">
                                        </YAF:MessagePostData>
                                    </div>
                                </div>
                            </div>
                        </div>
                        <div class="row">
                            <div class="col">
                                <YAF:BaseReportedPosts ID="ReportersList" runat="server" 
                                                   MessageID="<%# (Container.DataItem as dynamic).MessageID %>"
                                                   ResolvedBy="<%# (Container.DataItem as dynamic).ResolvedBy %>" 
                                                   Resolved="<%# (Container.DataItem as dynamic).Resolved %>"
                                                   ResolvedDate="<%# (Container.DataItem as dynamic).ResolvedDate %>" />
                            </div>
                        </div>
                    </div>
                    <div class="card-footer text-center">
                        <YAF:ThemeButton ID="CopyOverBtn" runat="server" 
                                         TextLocalizedPage="MODERATE_FORUM" TextLocalizedTag="COPYOVER" 
                                         CommandName="CopyOver" CommandArgument="<%# (Container.DataItem as dynamic).MessageID %>"
                                         Visible="<%# General.CompareMessage( (Container.DataItem as dynamic).OriginalMessage, (Container.DataItem as dynamic).Message)%>"
                                         Icon="copy" 
                                         Type="Secondary" />
                        <YAF:ThemeButton ID="DeleteBtn" runat="server" 
                                         TextLocalizedPage="MODERATE_FORUM" TextLocalizedTag="DELETE" 
                                         CommandName="Delete" CommandArgument='<%# string.Format("{0};{1}", (Container.DataItem as dynamic).MessageID,  (Container.DataItem as dynamic).TopicID) %>'
                                         ReturnConfirmText='<%# this.GetText("ASK_DELETE") %>'
                                         Icon="trash" 
                                         Type="Danger"/>
                        <YAF:ThemeButton ID="ResolveBtn" runat="server" 
                                         TextLocalizedPage="MODERATE_FORUM" TextLocalizedTag="RESOLVED" 
                                         CommandName="Resolved" CommandArgument="<%# (Container.DataItem as dynamic).MessageID %>"
                                         Icon="check" 
                                         Type="Success" />
                        <YAF:ThemeButton ID="ViewBtn" runat="server" 
                                         TextLocalizedPage="MODERATE_FORUM" TextLocalizedTag="VIEW" 
                                         CommandName="View" CommandArgument="<%# (Container.DataItem as dynamic).MessageID %>"
                                         Icon="eye" 
                                         Type="Secondary" />
                        <YAF:ThemeButton ID="ViewHistoryBtn" runat="server" 
                                         TextLocalizedPage="MODERATE_FORUM" TextLocalizedTag="HISTORY" 
                                         Visible="<%# General.CompareMessage((string)(Container.DataItem as dynamic).OriginalMessage, (string)(Container.DataItem as dynamic).Message)%>"
                                         CommandName="ViewHistory" CommandArgument='<%# "{0},{1}".Fmt(this.PageContext.PageForumID, (int)(Container.DataItem as dynamic).MessageID) %>'
                                         Icon="history" 
                                         Type="Secondary" />
                    </div>
                </div>
            </div>
        </div>
    </ItemTemplate>
</asp:Repeater>

