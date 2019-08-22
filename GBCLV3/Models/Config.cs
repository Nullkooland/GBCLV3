using System.Text.Json.Serialization;
using GBCLV3.Models.Launcher;
using Stylet;

namespace GBCLV3.Models
{
    class UsernameChangedEvent { }

    class Config
    {
        #region Game Configurations

        public string GameDir { get; set; }

        public string SelectedVersion { get; set; }

        public bool SegregateVersions { get; set; }

        public string Username { get; set; }

        public bool OfflineMode { get; set; }

        public string UUID { get; set; }

        public string Email { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

        public bool RefreshAuth { get; set; }

        public string AccessToken { get; set; }

        public string JreDir { get; set; }

        public uint JavaMaxMem { get; set; }

        public bool JavaDebugMode { get; set; }

        public string JvmArgs { get; set; }

        public bool FullScreen { get; set; }

        public uint WindowWidth { get; set; }

        public uint WindowHeight { get; set; }

        public string ServerAddress { get; set; }

        public string ExtraMinecraftArgs { get; set; }

        #endregion

        #region Launcher Configurations

        public string Language { get; set; }

        public string FontFamily { get; set; }

        public string FontWeight { get; set; }

        public AfterLaunchBehavior AfterLaunch { get; set; }

        public DownloadSource DownloadSource { get; set; }

        public bool DownloadAssetsOnInstall { get; set; }

        public bool UseBackgroundImage { get; set; }

        public string BackgroundImagePath { get; set; }

        public bool UseSystemColor { get; set; }

        public string ThemeColor { get; set; }

        #endregion
    }
}
