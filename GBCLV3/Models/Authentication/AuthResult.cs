using GBCLV3.Utils;
using System.Collections.Generic;
using System.ComponentModel;

namespace GBCLV3.Models.Authentication
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum AuthErrorType
    {
        [LocalizedDescription("EmptyUsernameError")]
        EmptyUsername,

        [LocalizedDescription("InvalidEmailError")]
        InvalidEmail,

        [LocalizedDescription("AuthTimeoutError")]
        Timeout,

        [LocalizedDescription("NoInternetConnectionError")]
        NoInternetConnection,

        [LocalizedDescription("InvalidCredentialsError")]
        InvalidCredentials,

        [LocalizedDescription("InvalidTokenError")]
        InvalidToken,

        [LocalizedDescription("UnknownError")]
        Unknown,
    }

    class AuthResult
    {
        public string Username { get; set; }

        public string UUID { get; set; }

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
