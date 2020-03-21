using GBCLV3.Models.Authentication;
using GBCLV3.Models.Download;
using GBCLV3.Models.Launch;
using System.Collections.Generic;

namespace GBCLV3.Models
{
    public class Config
    {
        #region Game Configurations

        public string GameDir { get; set; }

        public string SelectedVersion { get; set; }

        public bool SegregateVersions { get; set; }

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

        #region User Accounts
        public List<Account> Accounts { get; set; }

        #endregion

        #region Launcher Configurations

        public string Language { get; set; }

        public bool AutoCheckUpdate { get; set; }

        public AfterLaunchBehavior AfterLaunch { get; set; }

        public DownloadSource DownloadSource { get; set; }

        public string FontFamily { get; set; }

        public string FontWeight { get; set; }

        public bool UseBackgroundImage { get; set; }

        public string BackgroundImagePath { get; set; }

        // public bool UseSystemColor { get; set; }
        //
        // public string AccentColor { get; set; }

        public int Build { get; set; }

        #endregion

        #region Misc

        public bool DownloadAssetsOnInstall { get; set; }

        public bool CopyMods { get; set; }

        #endregion
    }
}
