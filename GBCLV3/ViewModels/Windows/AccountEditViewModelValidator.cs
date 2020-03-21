using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GBCLV3.Models.Authentication;
using GBCLV3.Services;
using GBCLV3.Services.Authentication;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels.Windows
{
    public class AccountEditViewModelValidator : IModelValidator<AccountEditViewModel>
    {
        #region Private Fields

        private readonly AccountService _accountService;
        private readonly AuthService _authService;
        private readonly LanguageService _languageService;

        private AccountEditViewModel _subject;

        #endregion

        #region Constructor

        [Inject]
        public AccountEditViewModelValidator(
            AccountService accountService,
            AuthService authService,
            LanguageService languageService)
        {
            _accountService = accountService;
            _authService = authService;
            _languageService = languageService;
        }

        #endregion

        public void Initialize(object subject)
        {
            _subject = subject as AccountEditViewModel;
        }

        public async Task<IEnumerable<string>> ValidatePropertyAsync(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(_subject.Username):
                    if (string.IsNullOrWhiteSpace(_subject.Username))
                    {
                        return new[] {_languageService.GetEntry("EmptyUsername")};
                    }

                    if (_subject.Type == AccountEditType.AddAccount &&
                        _subject.AuthMode == AuthMode.Offline &&
                        _accountService.HasOfflineAccount(_subject.Username))
                    {
                        return new[] {_languageService.GetEntry("DuplicateAccount")};
                    }

                    return null;

                case nameof(_subject.Email):
                    if (!IsValidEmailAddress(_subject.Email))
                    {
                        return new[] {_languageService.GetEntry("InvalidEmail")};
                    }

                    if (_subject.Type == AccountEditType.AddAccount &&
                        _accountService.HasOnlineAccount(_subject.AuthMode, _subject.Email))
                    {
                        return new[] {_languageService.GetEntry("DuplicateAccount")};
                    }

                    return null;

                case nameof(_subject.AuthServerBase):
                    if (!await _authService.IsValidAuthServer(_subject.AuthServerBase))
                    {
                        return new[] {_languageService.GetEntry("InvalidAuthServer")};
                    }

                    return null;

                default: return null;
            }
        }

        [SuppressMessage("Await.Warning", "CS1998")]
        public async Task<Dictionary<string, IEnumerable<string>>> ValidateAllPropertiesAsync()
        {
            throw new System.NotImplementedException();
        }

        public static bool IsValidEmailAddress(string emailAddress)
        {
            var regex = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
            return emailAddress != null && regex.IsMatch(emailAddress);
        }
    }
}