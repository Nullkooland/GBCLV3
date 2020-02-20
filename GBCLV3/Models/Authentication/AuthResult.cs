using GBCLV3.Utils;
using System.Collections.Generic;
using System.ComponentModel;

namespace GBCLV3.Models.Authentication
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum AuthErrorType
    {
        [LocalizedDescription(nameof(AuthTimeout))]
        AuthTimeout,

        [LocalizedDescription(nameof(NoInternetConnection))]
        NoInternetConnection,

        [LocalizedDescription(nameof(InvalidCredentials))]
        InvalidCredentials,

        [LocalizedDescription(nameof(InvalidToken))]
        InvalidToken,

        [LocalizedDescription(nameof(Unknown))]
        Unknown,
    }

    public class AuthResult
    {
        public AuthUserProfile SelectedProfile { get; set; }

        public List<AuthUserProfile> AvailableProfiles { get; set; }

        public string ClientToken { get; set; }

        public string AccessToken { get; set; }

        public string UserType { get; set; }

        public string Properties { get; set; }

        public bool IsSuccessful { get; set; }

        public AuthErrorType ErrorType { get; set; }

        public string ErrorMessage { get; set; }
    }
}
