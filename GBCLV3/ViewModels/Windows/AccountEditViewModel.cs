using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using GBCLV3.Models.Authentication;
using GBCLV3.Services;
using GBCLV3.Services.Authentication;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels.Windows
{
    internal class AccountEditViewModel : Screen
    {
        #region Private Fields

        // IoC
        private readonly AccountService _accountService;
        private readonly AuthService _authService;

        private string _username;
        private string _email;
        private string _password;
        private string _authServerBase;

        #endregion

        #region Constructor

        [Inject]
        public AccountEditViewModel(
            AccountService accountService,
            AuthService authService,
            ThemeService themeService,
            IModelValidator<AccountEditViewModel> validator) : base(validator)
        {
            AutoValidate = false;

            _accountService = accountService;
            _authService = authService;
            ThemeService = themeService;
        }

        public void Setup(AccountEditType type, Account currentAccount = null)
        {
            Type = type;

            CurrentAccount = currentAccount;
            AuthMode = currentAccount?.AuthMode ?? AuthMode.Offline;
            Username = currentAccount?.Username;
            Email = currentAccount?.Email;
            AuthServerBase = currentAccount?.AuthServerBase;

            Status = AccountEditStatus.EnterAccountInformation;
        }

        #endregion

        #region Bindings

        public AccountEditType Type { get; private set; }

        public AccountEditStatus Status { get; private set; }

        public bool IsLoading =>
            Status == AccountEditStatus.CheckingAuthServer || 
            Status == AccountEditStatus.Authenticating;

        public ThemeService ThemeService { get; }

        public Account CurrentAccount { get; private set; }

        public AuthMode AuthMode { get; set; }

        public bool IsOfflineMode => AuthMode == AuthMode.Offline;

        public bool IsExternalMode => AuthMode == AuthMode.AuthLibInjector;

        public string ErrorMessage { get; private set; }

        public string Username
        {
            get => _username;
            set
            {
                SetAndNotify(ref _username, value);
                CanConfirm = ValidateProperty();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                SetAndNotify(ref _email, value);
                CanConfirm = ValidateProperty();
            }
        }

        public void OnPasswordChanged(PasswordBox passwordBox, EventArgs _) => _password = passwordBox.Password;

        public string AuthServerBase
        {
            get => _authServerBase;
            set
            {
                if (value == null) return;

                if (value.StartsWith("http"))
                    _authServerBase = value.Replace("http://", "https://");
                else
                    _authServerBase = "https://" + value;
            }
        }

        public bool CanConfirm { get; private set; }

        public async void Confirm()
        {
            CanConfirm = false;

            if (IsOfflineMode)
            {
                _accountService.AddOfflineAccount(Username);
                this.RequestClose(true);
            }


            AuthResult authResult;

            if (IsExternalMode)
            {
                Status = AccountEditStatus.CheckingAuthServer;

                if (await ValidatePropertyAsync(nameof(AuthServerBase)))
                {
                    Status = AccountEditStatus.Authenticating;
                    authResult =
                        await _authService.AuthenticateAsync(Email, _password, _authServerBase + "/authserver");
                }
                else
                {
                    Status = AccountEditStatus.CheckAuthServerFailed;
                    CanConfirm = true;
                    return;
                }
            }
            else
            {
                Status = AccountEditStatus.Authenticating;
                authResult = await _authService.AuthenticateAsync(Email, _password);
            }

            if (authResult.IsSuccessful)
            {
                Status = AccountEditStatus.AuthSuccessful;
                CurrentAccount = await _accountService.AddOnlineAccount(Email, authResult, AuthMode, _authServerBase);
                await Task.Delay(500);
                RequestClose(true);
            }
            else
            {
                Status = AccountEditStatus.AuthFailed;
                CanConfirm = true;
            }
        }

        public void Cancel()
        {
            RequestClose(false);
        }

        #endregion

        #region Private Methods

        #endregion
    }
}