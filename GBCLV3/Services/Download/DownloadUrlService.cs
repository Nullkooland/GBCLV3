using GBCLV3.Models;
using GBCLV3.Models.Download;
using StyletIoC;

namespace GBCLV3.Services.Download
{
    public interface IDownloadUrlBase
    {
        string VersionList { get; }

        string Version { get; }

        string Library { get; }

        string Maven { get; }

        string Json { get; }

        string Asset { get; }

        string Forge { get; }
    }

    class DownloadUrlService
    {
        #region Properties

        public IDownloadUrlBase Base => _config.DownloadSource switch
        {
            DownloadSource.Official => _officialUrlBase,
            DownloadSource.BMCLAPI => _bmclapiUrlBase,
            _ => _officialUrlBase,
        };

        #endregion

        #region Private Fields

        private readonly Config _config;

        #endregion

        #region Constructor

        [Inject]
        public DownloadUrlService(ConfigService configService)
        {
            _config = configService.Entries;
        }

        #endregion

        #region Official URLs

        private static readonly OfficialUrlBase _officialUrlBase = new OfficialUrlBase();

        private class OfficialUrlBase : IDownloadUrlBase
        {
            public string VersionList => "https://launchermeta.mojang.com/mc/game/version_manifest.json";

            public string Version => "https://launcher.mojang.com/";

            public string Library => "https://libraries.minecraft.net/";

            public string Maven => "https://files.minecraftforge.net/maven/";

            public string Json => "https://launchermeta.mojang.com/";

            public string Asset => "https://resources.download.minecraft.net/";

            public string Forge => "https://files.minecraftforge.net/maven/net/minecraftforge/forge/";
        }

        #endregion

        #region BMCLAPI URLs

        private static readonly BMCLAPIUrlBase _bmclapiUrlBase = new BMCLAPIUrlBase();

        private class BMCLAPIUrlBase : IDownloadUrlBase
        {
            public string VersionList => "https://bmclapi2.bangbang93.com/mc/game/version_manifest.json";

            public string Version => "https://bmclapi2.bangbang93.com/";

            public string Library => "https://bmclapi2.bangbang93.com/libraries/";

            public string Maven => "https://bmclapi2.bangbang93.com/maven/";

            public string Json => "https://bmclapi2.bangbang93.com/";

            public string Asset => "https://bmclapi2.bangbang93.com/assets/";

            public string Forge => "https://bmclapi2.bangbang93.com/maven/net/minecraftforge/forge/";
        }

        #endregion
    }
}
