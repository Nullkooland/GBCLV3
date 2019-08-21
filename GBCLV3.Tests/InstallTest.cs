using System.Linq;
using System.Diagnostics;
using GBCLV3.Services;
using GBCLV3.Services.Launcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GBCLV3.Tests
{
    [TestClass]
    public class InstallTest
    {
        private const string GAME_ROOT_DIR = "G:/Minecraft/1.12.2/.minecraft";
        private const string ID = "1.12.2";

        private readonly ConfigService _configService;
        private readonly VersionService _versionService;
        private readonly ForgeInstallService _forgeInstallService;

        public InstallTest()
        {
            _configService = new ConfigService();
            _configService.Load();
            _configService.Entries.GameDir = GAME_ROOT_DIR;
            _configService.Entries.SelectedVersion = ID;
            _configService.Entries.SegregateVersions = false;
            _configService.Entries.JavaDebugMode = false;

            var gamePathService = new GamePathService(_configService);
            var urlServie = new UrlService(_configService);

            _versionService = new VersionService(gamePathService, urlServie);
            _forgeInstallService = new ForgeInstallService(gamePathService, urlServie, _versionService);
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
        public void GetForgeDownloadListTest()
        {
            var forgeDownloads = _forgeInstallService.GetDownloadListAsync(ID).Result;

            Debug.WriteLine($"[Available Forge Downloads for {ID}]");

            foreach (var download in forgeDownloads)
            {
                Debug.WriteLine("---------------------------------------------------------------");
                Debug.WriteLine($"ID:       {download.Build}");
                Debug.WriteLine($"Version:  {download.Version}");
                Debug.WriteLine($"Date:     {download.ReleaseTime}");
            }
        }

        [TestMethod]
        public void ForgeInstallTest()
        {
            var forgeDownloads = _forgeInstallService.GetDownloadListAsync(ID).Result;
            var download = _forgeInstallService.GetDownload(forgeDownloads.Last(), true);
            var forge = download.Last();

            Debug.WriteLine($"Name:     {forge.Name}");
            Debug.WriteLine($"Path:     {forge.Path}");
            Debug.WriteLine($"Url:      {forge.Url}");
        }
    }
}
