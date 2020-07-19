﻿<%@ Control Language="C#" AutoEventWireup="true" Inherits="YAF.Controls.MyTopicsList" CodeBehind="MyTopicsList.ascx.cs"  EnableViewState="true" %>


<div class="row">
    <div class="col">
        <div class="card mb-3">
            <div class="card-header">
                <YAF:IconHeader runat="server" ID="IconHeader"
                                IconName="comments"
                                LocalizedPage="MYTOPICS"
                                LocalizedTag="ActiveTopics" />
            </div>
            <div class="card-body">
                <YAF:Pager runat="server" ID="PagerTop" OnPageChange="Pager_PageChange" />
                <asp:Repeater ID="TopicList" runat="server">
                    <ItemTemplate>
                        <%# this.CreateTopicLine((System.Data.DataRowView)Container.DataItem) %>
                    </ItemTemplate>
                    <SeparatorTemplate>
                        <div class="row">
                            <div class="col">
                                <hr/>
                            </div>
                        </div>
                    </SeparatorTemplate>
                </asp:Repeater>
                <YAF:Pager runat="server" ID="PagerBottom" LinkedPager="PagerTop" OnPageChange="Pager_PageChange" />
            </div>
            <asp:Panel runat="server" ID="Footer" 
                       CssClass="card-footer">
                <div class="input-group align-items-center">
                    <asp:Label runat="server" AssociatedControlID="Since"
                               CssClass="input-group-text">
                        <YAF:LocalizedLabel ID="SinceLabel" runat="server"
                                            LocalizedTag="SINCE"/>
                    </asp:Label>
                    <asp:DropDownList ID="Since" runat="server" 
                                      AutoPostBack="True" 
                                      OnSelectedIndexChanged="Since_SelectedIndexChanged" 
                                      CssClass="form-select" />
                </div>
            </asp:Panel>
    </div>
</div>
</div>
<div class="row">
    <div class="col">
        <div class="btn-group float-right" role="group" aria-label="Tools">
            <YAF:ThemeButton runat="server" OnClick="MarkAll_Click" ID="MarkAll"
                             TextLocalizedTag="MARK_ALL_ASREAD" TextLocalizedPage="DEFAULT"
                             Type="Secondary"
                             Size="Small"
                             Icon="glasses"/>
            <YAF:RssFeedLink ID="RssFeed" runat="server" Visible="False" />
        </div>
    </div>
</div>