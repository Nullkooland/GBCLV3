using GBCLV3.Models;
using StyletIoC;

namespace GBCLV3.Services.Launcher
{
    class GamePathService
    {
        #region Properties

        public string RootDir => _config.GameDir;

        public string WorkingDir => _config.SegregateVersions ? $"{RootDir}/versions/{_config.SelectedVersion}" : RootDir;

        public string VersionDir => RootDir + "/versions";

        public string LibDir => RootDir + "/libraries";

        public string ForgeLibDir => LibDir + "/net/minecraftforge/forge";

        public string AssetDir => RootDir + "/assets";

        public string NativeDir => WorkingDir + "/natives";

        public string ModDir => WorkingDir + "/mods";

        public string ResourcePackDir => WorkingDir + "/resourcepacks";

        public string SaveDir => WorkingDir + "/saves";

        public string LogDir => WorkingDir + "/logs";

        public string JreExecutablePath => _config.JreDir + (_config.JavaDebugMode ? "/java.exe" : "/javaw.exe");

        #endregion

        #region Private Members

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
