using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GBCLV3.Services;
using GBCLV3.Services.Authentication;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels.Windows
{
    class AccountEditViewModelValidator : IModelValidator<AccountEditViewModel>
    {
        #region Private Fields

        private readonly AuthService _authService;
        private readonly LanguageService _languageService;

        private AccountEditViewModel _subject;

        #endregion

        #region Constructor

        [Inject]
        public AccountEditViewModelValidator(AuthService authService, LanguageService languageService)
        {
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
            return propertyName switch
            {
                nameof(_subject.Username) => string.IsNullOrWhiteSpace(_subject.Username)
                    ? new[] {_languageService.GetEntry("EmptyUsername")}
                    : null,

                nameof(_subject.Email) => !IsEmailAddressValid(_subject.Email) ? 
                    new[] {_languageService.GetEntry("InvalidEmail")} 
                    : null,

                nameof(_subject.AuthServerBase) => !await _authService.IsAuthServerValid(_subject.AuthServerBase)
                    ? new[] {_languageService.GetEntry("InvalidAuthServer")}
                    : null,

                _ => null
            };
        }

        public async Task<Dictionary<string, IEnumerable<string>>> ValidateAllPropertiesAsync()
        {
            throw new System.NotImplementedException();
        }

        public static bool IsEmailAddressValid(string emailAddress)
        {
            var regex = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
            return emailAddress != null && regex.IsMatch(emailAddress);
        }
    }
}