<%@ Control Language="C#" AutoEventWireup="true" Inherits="YAF.Pages.Admin.RunSql" Codebehind="RunSql.ascx.cs" %>


<YAF:PageLinks runat="server" ID="PageLinks" />

<div class="row">
               <div class="col-xl-12">
                    <div class="card mb-3">
                        <div class="card-header">
                            <YAF:IconHeader runat="server"
                                            IconName="database"
                                            LocalizedTag="SQL_COMMAND" 
                                            LocalizedPage="ADMIN_RUNSQL"></YAF:IconHeader>
                        </div>
                        <div class="card-body">
                            <div class="mb-3">
                                <asp:TextBox ID="txtQuery" runat="server" 
                                             TextMode="MultiLine" 
                                             Width="100%" 
                                             Height="100px" 
                                             CssClass="form-control"
                                             required="required" />
                                <div class="invalid-feedback">
                                    <YAF:LocalizedLabel runat="server"
                                                        LocalizedTag="NEED_QUERY" />
                                </div>
                            </div>
                            <div class="form-check form-switch">
                                <asp:Checkbox ID="chkRunInTransaction" runat="server" 
                                              Text="Run In Transaction" 
                                              Checked="true" />
                            </div>
                        </div>
                        <div class="card-footer text-center">
                            <YAF:ThemeButton ID="RunQuery" runat="server" 
                                             Type="Primary" 
                                             CausesValidation="True"
                                             OnClick="RunQueryClick"
                                             Icon="rocket"
                                             TextLocalizedTag="RUN_QUERY" 
                                             TextLocalizedPage="ADMIN_RUNSQL">
                            </YAF:ThemeButton>
                        </div>
                    </div>
                </div>
    </div>
    <asp:PlaceHolder ID="ResultHolder" runat="server" Visible="false">
    <div class="row">
        <div class="col-xl-12">
                    <div class="card mb-3">
                        <div class="card-header">
                             <i class="fa fa-rocket fa-fw"></i>&nbsp;Result
                        </div>
                        <div class="card-body">
                            <asp:TextBox ID="txtResult" runat="server" TextMode="MultiLine"  
                                Width="100%" Height="300px" Wrap="false" style="font-size: 8pt;" CssClass="form-control"></asp:TextBox>
                        </div>
                    </div>
                </div>
    </div>
		</asp:PlaceHolder>

