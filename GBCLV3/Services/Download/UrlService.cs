using GBCLV3.Models;
using GBCLV3.Models.Download;
using StyletIoC;

namespace GBCLV3.Services.Download
{
    public interface IUrlBase
    {
        string VersionList { get; }

        string Version { get; }

        string Library { get; }

        string Maven { get; }

        string Json { get; }

        string Asset { get; }

        string Forge { get; }
    }

    class UrlService
    {
        #region Properties

        public IUrlBase Base
        {
            get
            {
                switch (_config.DownloadSource)
                {
                    case DownloadSource.Official: return _officialUrlBase;
                    case DownloadSource.BMCLAPI: return _bmclapiUrlBase;
                    default: return _officialUrlBase;
                }
            }
        }

        #endregion

        #region Private Fields

        private readonly Config _config;

        #endregion

        #region Constructor

        [Inject]
        public UrlService(ConfigService configService)
        {
            _config = configService.Entries;
        }

        #endregion

        #region Official URLs

        private static readonly OfficialUrlBase _officialUrlBase = new OfficialUrlBase();

        private class OfficialUrlBase : IUrlBase
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

        private class BMCLAPIUrlBase : IUrlBase
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
