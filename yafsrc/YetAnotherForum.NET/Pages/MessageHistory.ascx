<%@ Control Language="C#" AutoEventWireup="true" Inherits="YAF.Pages.MessageHistory" CodeBehind="MessageHistory.ascx.cs" %>

<%@ Import Namespace="YAF.Types.Interfaces" %>
<%@ Import Namespace="YAF.Types.Extensions" %>
<%@ Import Namespace="ServiceStack" %>

<YAF:PageLinks runat="server" ID="PageLinks" />

<div class="row">
    <div class="col-xl-12">
        <h2><YAF:LocalizedLabel ID="LocalizedLabel6" runat="server" 
                                LocalizedTag="TITLE" />
        </h2>
    </div>
</div>

<div class="row">
    <div class="col">
        <div class="card mb-3">
            <div class="card-header">
                <YAF:IconHeader runat="server"
                                IconName="history"/>
            </div>
            <div class="card-body">
                <asp:Repeater ID="RevisionsList" runat="server"
                              OnItemCommand="RevisionsList_ItemCommand">
                    <HeaderTemplate>
                        <ul class="list-group">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <li class="list-group-item list-group-item-action">
                            <div class="d-flex w-100 justify-content-between">
                                     <h5 class="mb-1">
                                         <div class="form-check d-inline-block">
                                             <asp:Checkbox runat="server" ID="Compare" 
                                                           onclick="toggleSelection(this);" 
                                                           Text="&nbsp;" />
                                         </div>
                                         <asp:HiddenField runat="server" 
                                                          Value="<%#(Container.DataItem as dynamic).Message%>" ID="MessageField" />
                                         <YAF:LocalizedLabel ID="LocalizedLabel9" runat="server" 
                                                             LocalizedPage="POSTMESSAGE"
                                                             LocalizedTag="EDITEREASON" />: <%# (Container.DataItem as dynamic).Edited != (Container.DataItem as dynamic).Posted  ? ((string)(Container.DataItem as dynamic).EditReason).IsNotSet() ? this.GetText("EDIT_REASON_NA") : (Container.DataItem as dynamic).EditReason: this.GetText("ORIGINALMESSAGE") %>
                                         <%# Container.ItemIndex.Equals(this.RevisionsCount-1) ? "({0})".Fmt(this.GetText("MESSAGEHISTORY", "CURRENTMESSAGE")) : string.Empty %>
                                     </h5>
                                     <small class="d-none d-md-block">
                                         <YAF:LocalizedLabel ID="LocalizedLabel5" runat="server" 
                                                             LocalizedPage="POSTMESSAGE" 
                                                             LocalizedTag="EDITED" />: <%# this.Get<IDateTime>().FormatDateTimeTopic((DateTime)(Container.DataItem as dynamic).Edited) %>
                                     </small>
                                 </div>
                            <p class="mb-1">
                                    <YAF:LocalizedLabel ID="LocalizedLabel2" runat="server" 
                                                        LocalizedPage="POSTMESSAGE"
                                                        LocalizedTag="EDITEDBY" />: <YAF:UserLink ID="UserLink3" runat="server"
                                                                                                  ReplaceName="<%# this.PageContext.BoardSettings.EnableDisplayName ? (Container.DataItem as dynamic).DisplayName : (Container.DataItem as dynamic).Name %>"
                                                                                                  Suspended="<%# (Container.DataItem as dynamic).Suspended %>"
                                                                                                  Style="<%# (Container.DataItem as dynamic).UserStyle %>"
                                                                                                  UserID="<%# (Container.DataItem as dynamic).EditedBy %>" />
                                    <asp:PlaceHolder runat="server" Visible="<%# this.PageContext.IsAdmin || this.PageContext.BoardSettings.AllowModeratorsViewIPs && this.PageContext.ForumModeratorAccess%>">
                                        <span class="font-weight-bold mr-2">
                                            <%# this.GetText("IP") %>:
                                        </span><a id="IPLink1" 
                                                                                  href="<%# string.Format(this.PageContext.BoardSettings.IPInfoPageURL, this.GetIpAddress(Container.DataItem)) %>"
                                                                                  title='<%# this.GetText("COMMON","TT_IPDETAILS") %>'
                                                                                  target="_blank" runat="server"><%# this.GetIpAddress(Container.DataItem) %></a>
                                    </asp:PlaceHolder>
                                    <YAF:LocalizedLabel ID="LocalizedLabel3" runat="server" 
                                                        LocalizedPage="POSTMESSAGE"
                                                        LocalizedTag="EDITEDBY_MOD" />: <span class="badge bg-secondary"><%# (Container.DataItem as dynamic).IsModeratorChanged ?  this.GetText("YES") : this.GetText("NO") %></span>
                                </p>
                            <small>
                                    <YAF:ThemeButton ID="ThemeButtonEdit" runat="server"
                                                     CommandName="restore" 
                                                     CommandArgument="<%# (Container.DataItem as dynamic).Edited %>"
                                                     TitleLocalizedTag="RESTORE_MESSAGE" 
                                                     TextLocalizedTag="RESTORE_MESSAGE"
                                                     Visible="<%# (this.PageContext.IsAdmin || this.PageContext.IsModeratorInAnyForum) && !Container.ItemIndex.Equals(this.RevisionsCount-1) %>"
                                                     ReturnConfirmText='<%# this.GetText("MESSAGEHISTORY", "CONFIRM_RESTORE") %>'
                                                     Type="Secondary" 
                                                     Size="Small" 
                                                     Icon="undo">
                                    </YAF:ThemeButton>
                                </small>
                        </li>
                    </ItemTemplate>
                    <FooterTemplate>
                        </ul>
                    </FooterTemplate>
                </asp:Repeater>
            </div>
            <div class="card-footer text-center">
                        <a onclick="RenderMessageDiff('<%# this.GetText("MESSAGEHISTORY","NOTHING_SELECTED") %>','<%# this.GetText("MESSAGEHISTORY","SELECT_BOTH") %>');" 
                           class="btn btn-primary mb-1" role="button" href="#diffContent">
                            <i class="fas fa-equals"></i>&nbsp;<%# this.GetText("MESSAGEHISTORY","COMPARE_VERSIONS") %>
                        </a>            
                        <YAF:ThemeButton ID="ReturnBtn" 
                                         CssClass="mb-1"
                                         OnClick="ReturnBtn_OnClick"
                                         TextLocalizedTag="TOMESSAGE" 
                                         Visible="false" 
                                         Type="Secondary"
                                         Icon="external-link-square-alt"
                                         runat="server">
                        </YAF:ThemeButton>
                        <YAF:ThemeButton ID="ReturnModBtn"  
                                         CssClass="mb-1"
                                         OnClick="ReturnModBtn_OnClick"
                                         TextLocalizedTag="GOMODERATE" 
                                         Visible="false" 
                                         Type="Secondary"
                                         Icon="external-link-square-alt"
                                         runat="server">
                        </YAF:ThemeButton>
            </div>
        </div>
    </div>
</div>

<div class="row">
    <div class="col">
        <div class="card mb-3">
            <div class="card-header">
                <YAF:IconHeader runat="server"
                                IconName="history"
                                LocalizedTag="COMPARE_TITLE"/>
            </div>
            <div class="card-body">
                <h6 class="card-subtitle mb-2 text-muted">
                    <YAF:LocalizedLabel ID="LocalizedLabel8" runat="server" 
                                        LocalizedTag="TEXT_CHANGES" />
                </h6>
                <div id="diffContent">
                    <YAF:Alert runat="server" Type="info">
                        <YAF:Icon runat="server" IconName="info-circle" />
                        <%# this.GetText("MESSAGEHISTORY","SELECT_DIFFERENT") %>
                    </YAF:Alert>
                </div>
            </div>
        </div>
    </div>
</div>