using GBCLV3.Models.Auxiliary;
using System;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace GBCLV3.Models.Authentication
{
    public class Account
    {
        public AuthMode AuthMode { get; set; }

        [JsonIgnore] public string DisplayName => AuthMode != AuthMode.Offline ? $"{Email} ({Username})" : Username;

        public string Username { get; set; }

        public string UUID { get; set; }

        public string Email { get; set; }

        public string ClientToken { get; set; }

        public string AccessToken { get; set; }

        public string AuthServerBase { get; set; }

        public string AuthlibInjectorSHA256 { get; set; } // Very ugly...

        [JsonIgnore] 
        public string AuthServer => AuthServerBase != null ? $"{AuthServerBase}/authserver" : null;

        [JsonIgnore]
        public string ProfileServer =>
            AuthServerBase != null ? $"{AuthServerBase}/sessionserver/session/minecraft/profile" : null;

        [JsonIgnore] 
        public string PrefetchedAuthServerInfo { get; set; }


        public string Profile { get; set; }

        [JsonIgnore]
        public BitmapSource Avatar => Skin?.Face ??
                                      new BitmapImage(new Uri("/Resources/Images/enderman.png", UriKind.Relative));

        [JsonIgnore] public Skin Skin { get; set; }

        public bool IsSelected { get; set; }
    }
}