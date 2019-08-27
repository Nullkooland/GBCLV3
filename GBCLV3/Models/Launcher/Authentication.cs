using System.Collections.Generic;
using System.ComponentModel;
using GBCLV3.Utils;

namespace GBCLV3.Models.Launcher
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    enum AuthErrorType
    {
        [LocalizedDescription("UsernameEmptyError")]
        UsernameEmpty,

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

    class AuthRequest
    {
        public AuthAgent Agent => new AuthAgent { Name = "Minecraft", Version = 1 };

        public string Username { get; set; }

        public string Password { get; set; }

        public string ClientToken { get; set; }

        public bool RequestUser { get; set; }
    }

    class RefreshRequest
    {
        public string AccessToken { get; set; }

        public string ClientToken { get; set; }

        public AuthUserProfile SelectedProfile { get; set; }

        public bool RequestUser { get; set; }
    }

    class AuthResponse
    {
        public string AccessToken { get; set; }

        public string ClientToken { get; set; }

        public List<AuthUserProfile> AvailableProfiles { get; set; }

        public AuthUserProfile SelectedProfile { get; set; }
    }

    class AuthErrorResponse
    {
        public string Error { get; set; }

        public string ErrorMessage { get; set; }

        public string Cause { get; set; }
    }

    class AuthUserProfile
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool Legacy { get; set; }
    }

    class AuthAgent
    {
        public string Name { get; set; }

        public int Version { get; set; }
    }
}
