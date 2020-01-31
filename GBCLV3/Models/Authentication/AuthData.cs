using System.Collections.Generic;

namespace GBCLV3.Models.Authentication
{
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
