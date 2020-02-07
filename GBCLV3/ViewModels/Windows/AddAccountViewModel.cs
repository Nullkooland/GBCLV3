using FluentValidation;
using GBCLV3.Models.Authentication;
using GBCLV3.Services;
using GBCLV3.Services.Authentication;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;

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
            AutoValidate = false;
            _accountService = accountService;
            _authService = authService;
            ThemeService = themeService;
        }

        #endregion

        #region Bindings

        public ThemeService ThemeService { get; private set; }

        public Account CurrentAccount { get; private set; }

        public AuthMode AuthMode { get; set; }

        public bool IsOfflineMode => AuthMode == AuthMode.Offline;

        public bool IsExternalMode => AuthMode == AuthMode.AuthLibInjector;

        public bool IsAuthServerValid { get; private set; }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
            }
        }

        public void OnPasswordChanged(PasswordBox passwordBox, EventArgs _)
        {
            _password = passwordBox.Password;
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
            }
        }

        public async void Confirm()
        {
            if (IsOfflineMode)
            {
                if (ValidateProperty(nameof(Username)))
                {
                    _accountService.AddOfflineAccount(Username);
                }
                else
                {
                    return;
                }
            }

            if (!ValidateProperty(nameof(Email))) return;

            AuthResult authResult = null;

            if (IsExternalMode)
            {
                if (await ValidatePropertyAsync(nameof(AuthServer)))
                {
                    authResult = await _authService.AuthenticateAsync(_email, _password, _authServer);
                }
                else
                {
                    return;
                }
            }
            else
            {
                authResult = await _authService.AuthenticateAsync(_email, _password);
            }

            if (authResult.IsSuccessful)
            {
                CurrentAccount = await _accountService.AddOnlineAccount(_email, authResult, AuthMode, _authServer);
                await Task.Delay(2341); // 2 boba perals, 3 peanuts, 4 raisins and a bizarre spoon!
                this.RequestClose(true);
            }
        }

        public void Cancel() => this.RequestClose(false);

        #endregion

        #region Private Methods

        public static bool IsValidEmailAddress(string emailAddress)
        {
            var regex = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
            return regex.IsMatch(emailAddress);
        }

        #endregion
    }
}
