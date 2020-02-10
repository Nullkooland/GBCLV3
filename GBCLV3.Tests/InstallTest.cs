using System.Diagnostics;
using System.Linq;
using GBCLV3.Services;
using GBCLV3.Services.Download;
using GBCLV3.Services.Installation;
using GBCLV3.Services.Launch;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GBCLV3.Tests
{
    [TestClass]
    public class InstallTest
    {
        private const string GAME_ROOT_DIR = "G:/Minecraft/1.14.4/.minecraft";
        private const string ID = "1.14.4";

        private readonly ConfigService _configService;
        private readonly VersionService _versionService;
        private readonly ForgeInstallService _forgeInstallService;
        private readonly FabricInstallService _fabricInstallService;

        public InstallTest()
        {
            _configService = new ConfigService();
            _configService.Load();
            _configService.Entries.GameDir = GAME_ROOT_DIR;
            _configService.Entries.SelectedVersion = ID;
            _configService.Entries.SegregateVersions = false;
            _configService.Entries.JavaDebugMode = false;

            var gamePathService = new GamePathService(_configService);
            var urlServie = new DownloadUrlService(_configService);

            _versionService = new VersionService(gamePathService, urlServie);
            _forgeInstallService = new ForgeInstallService(gamePathService, urlServie, _versionService);
            _fabricInstallService = new FabricInstallService(gamePathService, urlServie, _versionService);
        }

        [TestMethod]
        public void GetVersionDownloadListTest()
        {
            var downloads = _versionService.GetDownloadListAsync().Result;

            Debug.WriteLine("[Available Versions To Download]");
            foreach (var download in downloads)
            {
                Debug.WriteLine("---------------------------------------------------------------");
                Debug.WriteLine($"ID:       {download.ID}");
                Debug.WriteLine($"Date:     {download.ReleaseTime}");
                Debug.WriteLine($"Type:     {download.Type}");
                Debug.WriteLine($"Url:      {download.Url}");
            }
        }

        [TestMethod]
        public void ForgeInstallTest()
        {
            var forges = _forgeInstallService.GetDownloadListAsync(ID).Result.Take(5);

            Debug.WriteLine($"[Available Forge Downloads for {ID}]");

            foreach (var forge in forges)
            {
                Debug.WriteLine("---------------------------------------------------------------");
                Debug.WriteLine($"ID:       {forge.Build}");
                Debug.WriteLine($"Version:  {forge.Version}");
                Debug.WriteLine($"Date:     {forge.ReleaseTime}");
            }

            var installer = _forgeInstallService.GetDownload(forges.Last()).FirstOrDefault();

            Debug.WriteLine($"[Latest Forge Installer]");

            Debug.WriteLine($"Name:         {installer.Name}");
            Debug.WriteLine($"Path:         {installer.Path}");
            Debug.WriteLine($"Url:          {installer.Url}");
        }

        [TestMethod]
        public void FabricInstallTest()
        {
            var fabrics = _fabricInstallService.GetDownloadListAsync(ID).Result.Take(5);

            Debug.WriteLine($"[Available Fabric Downloads for {ID}]");

            foreach (var fabric in fabrics)
            {
                Debug.WriteLine("---------------------------------------------------------------");
                Debug.WriteLine($"ID:           {fabric.Loader.Build}");
                Debug.WriteLine($"Version:      {fabric.Loader.Version}");
            }
        }
    }
}
