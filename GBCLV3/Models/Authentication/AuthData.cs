using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;
using GBCLV3.Models.Auxiliary;

namespace GBCLV3.Models.Authentication
{
    public class AuthRequest
    {
        public AuthAgent Agent => new AuthAgent { Name = "Minecraft", Version = 1 };

        public string Username { get; set; }

        public string Password { get; set; }

        public string ClientToken { get; set; }

        public bool RequestUser { get; set; }
    }

    public class RefreshRequest
    {
        public string AccessToken { get; set; }

        public string ClientToken { get; set; }

        public AuthUserProfile SelectedProfile { get; set; }

        public bool RequestUser { get; set; }
    }

    public class AuthResponse
    {
        public string AccessToken { get; set; }

        public string ClientToken { get; set; }

        public List<AuthUserProfile> AvailableProfiles { get; set; }

        public AuthUserProfile SelectedProfile { get; set; }
    }

    public class AuthErrorResponse
    {
        public string Error { get; set; }

        public string ErrorMessage { get; set; }

        public string Cause { get; set; }
    }

    public class AuthUserProfile
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public bool Legacy { get; set; }

        [JsonIgnore]
        public string Base64Profile { get; set; }

        [JsonIgnore]
        public Skin Skin { get; set; }
    }

    public class AuthAgent
    {
        public string Name { get; set; }

        public int Version { get; set; }
    }
}
