using GBCLV3.Models.Authentication;
using GBCLV3.Services;
using GBCLV3.Services.Authentication;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace GBCLV3.ViewModels.Windows
{
    class AddAccountViewModel : Screen
    {
        #region Private Fields

        // IoC
        private readonly AccountService _accountService;
        private readonly AuthService _authService;

        private string _username;
        private string _email;
        private string _password;
        private string _authServer;

        #endregion

        #region Constructor

        [Inject]
        public AddAccountViewModel(
            AccountService accountService,
            AuthService authService,
            ThemeService themeService,

            IModelValidator<AddAccountViewModel> validator) : base(validator)
        {
            _accountService = accountService;
            _authService = authService;
            ThemeService = themeService;
        }

        #endregion

        #region Bindings


        public ThemeService ThemeService { get; private set; }

        public Account CurrentAccount { get; private set; }

        public AuthMode AuthMode { get; set; }

        public bool IsOffline => AuthMode == AuthMode.Offline;

        public bool IsExternal => AuthMode == AuthMode.AuthLibInjector;

        public string Username
        {
            get => _username;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ValidationException();
                }
                _username = value;
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                if (!TextUtil.IsValidEmailAddress(value))
                {
                    throw new ValidationException();
                }
                _email = value;
            }
        }

        public string AuthServer
        {
            get => _authServer;
            set
            {
                if (value.StartsWith("http"))
                {
                    _authServer = value.Replace("http://", "https://");
                }
                else
                {
                    _authServer = "https://" + value;
                }

                UpdateAuthServerInfo().ConfigureAwait(false);
            }
        }

        public bool CanConfirm => AuthMode switch
        {
            AuthMode.Offline => _username != null,
            AuthMode.Yggdrasil => _email != null && _password != null,
            AuthMode.AuthLibInjector => _email != null && _password != null && _authServer != null,
            _ => false,
        };

        public async void Confirm()
        {
            if (IsOffline)
            {
                _accountService.AddOfflineAccount(Username);
                this.RequestClose(true);
                return;
            }

            var authResult = IsExternal ?
                await _authService.AuthenticateAsync(_email, _password, _authServer) :
                await _authService.AuthenticateAsync(_email, _password);

            if (authResult.IsSuccessful)
            {
                CurrentAccount = await _accountService.AddOnlineAccount(authResult, AuthMode, _authServer);
                await Task.Delay(2341); // 2 boba perals, 3 peanuts, 4 raisins and a bizarre spoon!
                this.RequestClose(true);
            }
        }

        public void Cancel() => this.RequestClose(true);

        #endregion

        #region Private Methods

        private async Task UpdateAuthServerInfo()
        {
            var info = await _authService.GetAuthServerInfo(_authServer);
        }

        #endregion
    }
}
