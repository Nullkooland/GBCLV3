using FluentValidation;
using GBCLV3.Services.Authentication;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GBCLV3.ViewModels.Windows
{
    class AddAccountViewModelValidator : AbstractValidator<AddAccountViewModel>
    {
        #region Private Fields

        // IoC
        private readonly AuthService _authService;

        #endregion

        [Inject]
        public AddAccountViewModelValidator(AuthService authService)
        {
            _authService = authService;

            RuleFor(x => x.Username).NotEmpty().WithMessage("Username cannot be empty");

            RuleFor(x => x.Email).NotEmpty().Must(email => IsEmailAddressValid(email)).WithMessage("Invalid email");

            RuleFor(x => x.AuthServer).MustAsync(async (authServer, cancellation) => {
                return await _authService.IsAuthServerValid(authServer);
            }).WithMessage("Invalid authServer");
        }

        #region Private Helpers

        private static bool IsEmailAddressValid(string emailAddress)
        {
            var regex = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
            return emailAddress != null && regex.IsMatch(emailAddress);
        }

        #endregion
    }
}
