﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ItemTagSelect.ascx.cs" Inherits="RockWeb.Plugins.com_bemadev.CheckIn.ItemTagSelect" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('a.btn-checkin-select').click(function () {
            $(this).siblings().attr('onclick', 'return false;');
        });
    });
</script>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>

        <Rock:ModalAlert ID="maWarning" runat="server" />

        <div class="checkin-header">
            <h1>
                <asp:Literal ID="lTitle" runat="server" /><div class="checkin-sub-title">
                    <asp:Literal ID="lSubTitle" runat="server"></asp:Literal>
                </div>
            </h1>
        </div>

        <div class="checkin-body">

            <div class="checkin-scroll-panel">
                <div class="scroller">

                    <div class="control-group checkin-body-container">
                        <label class="control-label">
                            <asp:Literal ID="lCaption" runat="server" /></label>
                        <div class="controls">
                            <Rock:NumberUpDown ID="nbItemTags" runat="server" Label="How Many Item Tags would you like?" Minimum="0" Maximum="5" Value="0" />
                        </div>
                    </div>

                </div>
            </div>
        </div>

        <div class="row-fluid checkin-footer">
            <div class="checkin-actions">
                <asp:LinkButton CssClass="btn btn-primary" ID="lbSelect" runat="server" OnClick="lbSelect_Click" Text="Next" />
                <asp:LinkButton CssClass="btn btn-default btn-back" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
                <asp:LinkButton CssClass="btn btn-default btn-cancel" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
