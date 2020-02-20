using GBCLV3.Models;
using StyletIoC;

namespace GBCLV3.Services.Launch
{
    public class GamePathService
    {
        #region Properties

        public string RootDir => _config.GameDir;

        public string WorkingDir => _config.SegregateVersions ? $"{RootDir}/versions/{_config.SelectedVersion}" : RootDir;

        public string VersionsDir => RootDir + "/versions";

        public string LibrariesDir => RootDir + "/libraries";

        public string ForgeLibDir => LibrariesDir + "/net/minecraftforge/forge";

        public string AssetsDir => RootDir + "/assets";

        public string NativesDir => WorkingDir + "/natives";

        public string ModsDir => WorkingDir + "/mods";

        public string ResourcePacksDir => WorkingDir + "/resourcepacks";

        public string SavesDir => WorkingDir + "/saves";

        public string LogsDir => WorkingDir + "/logs";

        public string JreExecutablePath => _config.JreDir + (_config.JavaDebugMode ? "/java.exe" : "/javaw.exe");

        #endregion

        #region Private Fields

        private readonly Config _config;

        #endregion

        #region Constructor

        [Inject]
        public GamePathService(ConfigService configService)
        {
            _config = configService.Entries;
        }

        #endregion
    }
}
