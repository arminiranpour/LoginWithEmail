<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="View.ascx.cs" Inherits="LoginWithEmail.Modules.LoginWithEmail.View" %>

<div class="dnnForm lwa-container">
    <asp:Panel ID="pnlSuccess" runat="server" CssClass="lwa-panel" Visible="false">
        <asp:Label ID="lblSuccessHeading" runat="server" CssClass="lwa-heading" resourcekey="SuccessHeading" />
        <asp:Label ID="lblSuccessDetails" runat="server" CssClass="dnnFormMessage dnnFormInfo" />
    </asp:Panel>

    <asp:Panel ID="pnlEmail" runat="server" CssClass="lwa-panel">
        <div class="dnnFormItem">
            <asp:Label ID="lblEmail" runat="server" AssociatedControlID="txtEmail" resourcekey="EmailLabel" CssClass="dnnFormLabel" />
            <asp:TextBox ID="txtEmail" runat="server" CssClass="dnnFormInput" />
        </div>
        <asp:Label ID="lblEmailError" runat="server" CssClass="dnnFormMessage dnnFormError" Visible="false" />
        <div class="dnnActions">
            <asp:LinkButton ID="btnContinue" runat="server" CssClass="dnnPrimaryAction" resourcekey="ContinueButton" OnClick="OnContinueClick" />
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlSignIn" runat="server" CssClass="lwa-panel" Visible="false">
        <asp:Label ID="lblSignInPrompt" runat="server" CssClass="dnnFormMessage dnnFormInfo" />
        <div class="dnnFormItem">
            <asp:Label ID="lblSignInPassword" runat="server" AssociatedControlID="txtSignInPassword" resourcekey="PasswordLabel" CssClass="dnnFormLabel" />
            <asp:TextBox ID="txtSignInPassword" runat="server" CssClass="dnnFormInput" TextMode="Password" />
        </div>
        <asp:Label ID="lblSignInError" runat="server" CssClass="dnnFormMessage dnnFormError" Visible="false" />
        <div class="dnnActions">
            <asp:LinkButton ID="btnSignIn" runat="server" CssClass="dnnPrimaryAction" resourcekey="SignInButton" OnClick="OnSignInClick" />
            <asp:LinkButton ID="btnBackFromSignIn" runat="server" CssClass="dnnSecondaryAction" resourcekey="ChangeEmailButton" OnClick="OnChangeEmailClick" CausesValidation="false" />
        </div>
    </asp:Panel>

    <asp:Panel ID="pnlSignUp" runat="server" CssClass="lwa-panel" Visible="false">
        <asp:Label ID="lblSignUpPrompt" runat="server" CssClass="dnnFormMessage dnnFormInfo" />
        <div class="dnnFormItem">
            <asp:Label ID="lblFirstName" runat="server" AssociatedControlID="txtFirstName" resourcekey="FirstNameLabel" CssClass="dnnFormLabel" />
            <asp:TextBox ID="txtFirstName" runat="server" CssClass="dnnFormInput" />
        </div>
        <div class="dnnFormItem">
            <asp:Label ID="lblLastName" runat="server" AssociatedControlID="txtLastName" resourcekey="LastNameLabel" CssClass="dnnFormLabel" />
            <asp:TextBox ID="txtLastName" runat="server" CssClass="dnnFormInput" />
        </div>
        <div class="dnnFormItem">
            <asp:Label ID="lblSignUpPassword" runat="server" AssociatedControlID="txtSignUpPassword" resourcekey="PasswordLabel" CssClass="dnnFormLabel" />
            <asp:TextBox ID="txtSignUpPassword" runat="server" CssClass="dnnFormInput" TextMode="Password" />
        </div>
        <asp:Label ID="lblSignUpError" runat="server" CssClass="dnnFormMessage dnnFormError" Visible="false" />
        <div class="dnnActions">
            <asp:LinkButton ID="btnSignUp" runat="server" CssClass="dnnPrimaryAction" resourcekey="SignUpButton" OnClick="OnSignUpClick" />
            <asp:LinkButton ID="btnBackFromSignUp" runat="server" CssClass="dnnSecondaryAction" resourcekey="ChangeEmailButton" OnClick="OnChangeEmailClick" CausesValidation="false" />
        </div>
    </asp:Panel>
</div>