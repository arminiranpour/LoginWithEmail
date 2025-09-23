/*
' Copyright (c) 2025  Armin
'  All rights reserved.
' 
' THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED
' TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
' THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
' CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
' DEALINGS IN THE SOFTWARE.
' 
*/

using System;

using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Localization;

namespace LoginWithEmail.Modules.LoginWithEmail
{
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The View class displays the content
    /// 
    /// Typically your view control would be used to display content or functionality in your module.
    /// 
    /// View may be the only control you have in your project depending on the complexity of your module
    /// 
    /// Because the control inherits from LoginWithEmailModuleBase you have access to any custom properties
    /// defined there, as well as properties from DNN such as PortalId, ModuleId, TabId, UserId and many more.
    /// 
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class View : LoginWithEmailModuleBase, IActionable
    {
        private const string SelectedEmailViewStateKey = "LoginWithEmail_SelectedEmail";
        private const string CurrentStepViewStateKey = "LoginWithEmail_CurrentStep";

        private static readonly Regex EmailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private enum AuthStep
        {
            Email,
            SignIn,
            SignUp,
            Success
        }

        private string SelectedEmail
        {
            get => ViewState[SelectedEmailViewStateKey] as string;
            set => ViewState[SelectedEmailViewStateKey] = value;
        }

        private AuthStep CurrentStep
        {
            get
            {
                if (ViewState[CurrentStepViewStateKey] is AuthStep step)
                {
                    return step;
                }

                return AuthStep.Email;
            }
            set => ViewState[CurrentStepViewStateKey] = value;
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            try
            {
                if (!IsPostBack)
                {
                    CurrentStep = AuthStep.Email;
                }

                ApplyCurrentStep();
            }
            catch (Exception exc) //Module failed to load
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        public ModuleActionCollection ModuleActions
        {
            get
            {
                var actions = new ModuleActionCollection
                    {
                        {
                            GetNextActionID(), Localization.GetString("EditModule", LocalResourceFile), "", "", "",
                            EditUrl(), false, SecurityAccessLevel.Edit, true, false
                        }
                    };
                return actions;
            }
        }

        protected void OnContinueClick(object sender, EventArgs e)
        {
            ClearMessages();

            var email = (txtEmail.Text ?? string.Empty).Trim();
            txtEmail.Text = email;

            if (!IsValidEmail(email))
            {
                ShowEmailError(LocalizeString("EmailInvalid.Error"));
                CurrentStep = AuthStep.Email;
                ApplyCurrentStep();
                return;
            }

            SelectedEmail = email;

            var user = UserController.GetUserByEmail(PortalId, email);
            if (user != null && !user.IsDeleted)
            {
                CurrentStep = AuthStep.SignIn;
                txtSignInPassword.Text = string.Empty;
            }
            else
            {
                CurrentStep = AuthStep.SignUp;
                txtFirstName.Text = string.Empty;
                txtLastName.Text = string.Empty;
                txtSignUpPassword.Text = string.Empty;
            }

            ApplyCurrentStep();
        }

        protected void OnChangeEmailClick(object sender, EventArgs e)
        {
            ClearMessages();

            txtSignInPassword.Text = string.Empty;
            txtSignUpPassword.Text = string.Empty;

            CurrentStep = AuthStep.Email;
            if (!string.IsNullOrEmpty(SelectedEmail))
            {
                txtEmail.Text = SelectedEmail;
            }

            ApplyCurrentStep();
        }

        protected void OnSignInClick(object sender, EventArgs e)
        {
            ClearMessages();

            var email = SelectedEmail;
            if (string.IsNullOrEmpty(email))
            {
                CurrentStep = AuthStep.Email;
                ApplyCurrentStep();
                return;
            }

            var user = UserController.GetUserByEmail(PortalId, email);
            if (user == null || user.IsDeleted)
            {
                ShowSignInError(LocalizeString("SignInError.Text"));
                CurrentStep = AuthStep.SignIn;
                ApplyCurrentStep();
                return;
            }

            var loginStatus = UserLoginStatus.LOGIN_FAILURE;
            var userHostAddress = Request.UserHostAddress ?? string.Empty;
            var validatedUser = UserController.ValidateUser(PortalId, user.Username, txtSignInPassword.Text, string.Empty, PortalSettings.PortalName, userHostAddress, ref loginStatus);
            if (loginStatus == UserLoginStatus.LOGIN_SUCCESS && validatedUser != null)
            {
                UserController.UserLogin(PortalId, validatedUser, PortalSettings.PortalName, userHostAddress, false);
                Response.Redirect(GetRedirectUrl(), true);
                return;
            }

            txtSignInPassword.Text = string.Empty;
            ShowSignInError(LocalizeString("SignInError.Text"));
            CurrentStep = AuthStep.SignIn;
            ApplyCurrentStep();
        }

        protected void OnSignUpClick(object sender, EventArgs e)
        {
            ClearMessages();

            var email = SelectedEmail;
            if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
            {
                ShowEmailError(LocalizeString("EmailInvalid.Error"));
                CurrentStep = AuthStep.Email;
                ApplyCurrentStep();
                return;
            }

            var firstName = (txtFirstName.Text ?? string.Empty).Trim();
            var lastName = (txtLastName.Text ?? string.Empty).Trim();
            var password = txtSignUpPassword.Text ?? string.Empty;

            if (string.IsNullOrEmpty(firstName))
            {
                ShowSignUpError(LocalizeString("SignUpMissingFirstName.Error"));
                CurrentStep = AuthStep.SignUp;
                ApplyCurrentStep();
                return;
            }

            if (string.IsNullOrEmpty(lastName))
            {
                ShowSignUpError(LocalizeString("SignUpMissingLastName.Error"));
                CurrentStep = AuthStep.SignUp;
                ApplyCurrentStep();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowSignUpError(LocalizeString("SignUpMissingPassword.Error"));
                CurrentStep = AuthStep.SignUp;
                ApplyCurrentStep();
                return;
            }

            if (!IsPasswordValid(password))
            {
                ShowSignUpError(LocalizeString("PasswordPolicy.Error"));
                CurrentStep = AuthStep.SignUp;
                ApplyCurrentStep();
                return;
            }

            var newUser = new UserInfo
            {
                PortalID = PortalId,
                Username = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                DisplayName = BuildDisplayName(firstName, lastName)
            };

            newUser.Profile.InitialiseProfile(PortalId);
            newUser.Membership.Approved = true;
            newUser.Membership.Password = password;
            newUser.Membership.PasswordConfirm = password;
            newUser.Membership.UpdatePassword = true;

            var status = UserController.CreateUser(ref newUser);
            if (status == UserCreateStatus.Success)
            {
                if (newUser.Membership.Approved)
                {
                    var userHostAddress = Request.UserHostAddress ?? string.Empty;
                    UserController.UserLogin(PortalId, newUser, PortalSettings.PortalName, userHostAddress, false);
                    Response.Redirect(GetRedirectUrl(), true);
                    return;
                }

                CurrentStep = AuthStep.Success;
                lblSuccessDetails.Text = FormatLocalizedString("SignUpSuccessPending.Text", email);
                ApplyCurrentStep();
                return;
            }

            ShowSignUpError(GetCreateStatusMessage(status));
            CurrentStep = AuthStep.SignUp;
            ApplyCurrentStep();
        }

        private void ApplyCurrentStep()
        {
            pnlEmail.Visible = CurrentStep == AuthStep.Email;
            pnlSignIn.Visible = CurrentStep == AuthStep.SignIn;
            pnlSignUp.Visible = CurrentStep == AuthStep.SignUp;
            pnlSuccess.Visible = CurrentStep == AuthStep.Success;

            if (CurrentStep == AuthStep.SignIn)
            {
                lblSignInPrompt.Text = FormatLocalizedString("SignInPrompt.Text", SelectedEmail);
            }

            if (CurrentStep == AuthStep.SignUp)
            {
                lblSignUpPrompt.Text = FormatLocalizedString("SignUpPrompt.Text", SelectedEmail);
            }
        }

        private void ClearMessages()
        {
            lblEmailError.Visible = false;
            lblSignInError.Visible = false;
            lblSignUpError.Visible = false;
            lblSuccessDetails.Text = string.Empty;
            pnlSuccess.Visible = false;
        }

        private static bool IsValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && EmailRegex.IsMatch(email);
        }

        private bool IsPasswordValid(string password)
        {
            var provider = MembershipProvider.Instance();
            var regexPattern = provider.PasswordStrengthRegularExpression;
            if (!string.IsNullOrEmpty(regexPattern))
            {
                try
                {
                    if (!Regex.IsMatch(password, regexPattern))
                    {
                        return false;
                    }
                }
                catch (ArgumentException)
                {
                    // Ignore invalid custom expressions and fall back to provider validation.
                }
            }

            return true;
        }

        private string GetRedirectUrl()
        {
            var returnUrl = Request.QueryString["returnurl"];
            if (!string.IsNullOrEmpty(returnUrl))
            {
                returnUrl = HttpUtility.UrlDecode(returnUrl);
                if (!string.IsNullOrEmpty(returnUrl) && UrlUtils.IsLocalUrl(returnUrl))
                {
                    return returnUrl;
                }
            }

            return Globals.NavigateURL();
        }

        private static string BuildDisplayName(string firstName, string lastName)
        {
            var builder = new StringBuilder();
            if (!string.IsNullOrEmpty(firstName))
            {
                builder.Append(firstName.Trim());
            }

            if (!string.IsNullOrEmpty(lastName))
            {
                if (builder.Length > 0)
                {
                    builder.Append(' ');
                }

                builder.Append(lastName.Trim());
            }

            return builder.Length > 0 ? builder.ToString() : null;
        }

        private void ShowEmailError(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                lblEmailError.Text = message;
                lblEmailError.Visible = true;
            }
        }

        private void ShowSignInError(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                lblSignInError.Text = message;
                lblSignInError.Visible = true;
            }
        }

        private void ShowSignUpError(string message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                lblSignUpError.Text = message;
                lblSignUpError.Visible = true;
            }
        }

        private string GetCreateStatusMessage(UserCreateStatus status)
        {
            var key = $"CreateStatus_{status}.Error";
            var message = LocalizeString(key);
            if (!string.IsNullOrEmpty(message))
            {
                return message;
            }

            var defaultMessage = LocalizeString("CreateStatus_Default.Error");
            return !string.IsNullOrEmpty(defaultMessage)
                ? string.Format(defaultMessage, status)
                : status.ToString();
        }

        private string LocalizeString(string key)
        {
            return Localization.GetString(key, LocalResourceFile) ?? string.Empty;
        }

        private string FormatLocalizedString(string key, params object[] arguments)
        {
            var format = LocalizeString(key);
            return string.IsNullOrEmpty(format) ? string.Empty : string.Format(format, arguments);
        }
    }
}