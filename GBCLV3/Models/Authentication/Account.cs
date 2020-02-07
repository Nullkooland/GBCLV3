using GBCLV3.Models.Auxiliary;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows.Media.Imaging;

namespace GBCLV3.Models.Authentication
{
    class Account
    {
        public string Username { get; set; }

        [JsonIgnore]
        public BitmapSource Avatar => Skin?.Face ?? 
                                      new BitmapImage(new Uri("/Resources/Images/enderman.png", UriKind.Relative));

        [JsonIgnore]
        public Skin Skin { get; set; }

        public string SkinProfile { get; set; }

        public AuthMode AuthMode { get; set; }

        public string Email { get; set; }

        public string ClientToken { get; set; }

        public string AccessToken { get; set; }

        public string UUID { get; set; }

        public string AuthServer { get; set; }

        public bool IsSelected { get; set; }
    }
}
