using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using GBCLV3.Models.Authentication;
using GBCLV3.Services;
using GBCLV3.Services.Authentication;
using PropertyChanged;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels.Windows
{
    public class AccountEditViewModel : Screen
    {
        #region Private Fields

        // IoC
        private readonly AccountService _accountService;
        private readonly AuthService _authService;

        private readonly ProfileSelectViewModel _profileSelectVM;
        private readonly IWindowManager _windowManager;

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
            ProfileSelectViewModel profileSelectVM,
            IWindowManager windowManager,
            IModelValidator<AccountEditViewModel> validator) : base(validator)
        {
            AutoValidate = false;

            _accountService = accountService;
            _authService = authService;

            _profileSelectVM = profileSelectVM;
            _windowManager = windowManager;
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

            Status = type == AccountEditType.ReAuth
                ? AccountEditStatus.NeedReAuth
                : AccountEditStatus.EnterAccountInformation;

            HasAuthError = false;
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

        public bool HasAuthError { get; private set; }

        public AuthErrorType AuthErrorType { get; private set; }

        public string AuthErrorMessage { get; private set; }

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

        [SuppressPropertyChangedWarnings]
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

        public string AuthServer => AuthServerBase != null ? $"{AuthServerBase}/authserver" : null;

        public string ProfileServer =>
            AuthServerBase != null ? $"{AuthServerBase}/sessionserver/session/minecraft/profile" : null;

        public bool CanConfirm { get; private set; }

        public async void Confirm()
        {
            CanConfirm = false;

            if (IsOfflineMode)
            {
                if (Type == AccountEditType.AddAccount)
                {
                    _accountService.AddOfflineAccount(Username);
                }
                else
                {
                    await _accountService.UpdateAccountAsync(CurrentAccount, Username);
                }

                this.RequestClose(true);
            }

            AuthResult authResult;

            if (IsExternalMode)
            {
                Status = AccountEditStatus.CheckingAuthServer;

                if (await ValidatePropertyAsync(nameof(AuthServerBase)))
                {
                    Status = AccountEditStatus.Authenticating;
                    HasAuthError = false;
                    authResult =
                        await _authService.AuthenticateAsync(_email, _password, AuthServer);
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
                HasAuthError = false;
                authResult = await _authService.AuthenticateAsync(Email, _password);
            }

            if (authResult.IsSuccessful)
            {
                Status = AccountEditStatus.AuthSuccessful;

                if (authResult.SelectedProfile == null)
                {
                    var selectedProfile = authResult.AvailableProfiles.FirstOrDefault();

                    _profileSelectVM.Setup(authResult.AvailableProfiles, ProfileServer);
                    if (_windowManager.ShowDialog(_profileSelectVM) ?? false)
                    {
                        selectedProfile = _profileSelectVM.SelectedProfile;
                    }

                    authResult = await _authService.RefreshAsync(authResult.ClientToken, authResult.AccessToken,
                        AuthServer, selectedProfile);
                }

                if (Type == AccountEditType.AddAccount)
                {
                    CurrentAccount =
                        await _accountService.AddOnlineAccountAsync(_email, AuthMode, authResult, _authServerBase);
                }
                else
                {
                    await _accountService.UpdateAccountAsync(CurrentAccount, null, AuthMode, _email, authResult,
                          _authServerBase);
                    NotifyOfPropertyChange(nameof(CurrentAccount));
                }

                await Task.Delay(500);
                this.RequestClose(true);
            }
            else
            {
                Status = AccountEditStatus.AuthFailed;

                HasAuthError = true;
                AuthErrorType = authResult.ErrorType;
                AuthErrorMessage = authResult.ErrorMessage;

                CanConfirm = true;
            }
        }

        public void Cancel() => RequestClose(false);

        public void OnWindowLoaded(Window window, RoutedEventArgs _)
        {
            ThemeService.SetBackgroundEffect(window);
        }

        #endregion

        #region Private Methods

        #endregion
    }
}