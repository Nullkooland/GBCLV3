using System.Diagnostics;
using System.Linq;
using GBCLV3.Models;
using GBCLV3.Services;
using GBCLV3.Services.Launcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GBCLV3.Tests
{
    [TestClass]
    public class AssetTest
    {
        private const string GAME_ROOT_DIR = "G:/Minecraft/1.12.2/.minecraft";
        private const string ID = "1.6.4";

        private readonly ConfigService _configService;
        private readonly VersionService _versionService;
        private readonly AssetService _assetService;

        public AssetTest()
        {
            _configService = new ConfigService();
            _configService.Load();
            _configService.Entries.GameDir = GAME_ROOT_DIR;
            _configService.Entries.SelectedVersion = ID;

            var gamePathService = new GamePathService(_configService);
            var urlServie = new UrlService(_configService);

            _versionService = new VersionService(gamePathService, urlServie);
            _assetService = new AssetService(gamePathService, urlServie);

            _versionService.LoadAll();
            Assert.IsTrue(_versionService.Any(), "No available versions!");
        }

        [TestMethod]
        public void AssetsTest()
        {
            var version = _versionService.GetByID(ID);
            _assetService.LoadAllObjects(version.AssetsInfo);

            Debug.WriteLine(version.AssetsInfo.Objects.Count);

            var damagedAssets = _assetService.CheckIntegrityAsync(version.AssetsInfo).Result;
            foreach (var asset in damagedAssets)
            {
                Debug.WriteLine(asset.Path);
            }
        }

        [TestMethod]
        public void GetDownloadInfoTest()
        {
            var version = _versionService.GetByID(ID);
            _assetService.LoadAllObjects(version.AssetsInfo);

            var downloads = _assetService.GetDownloads(version.AssetsInfo.Objects.Values);
            int totalBytes = downloads.Sum(obj => obj.Size);

            Debug.WriteLine($"Type: {DownloadType.Assets}");
            Debug.WriteLine($"downloads Count: {downloads.Count()}");
            Debug.WriteLine($"downloads TotalBytes: {totalBytes}");

            Debug.WriteLine("[Asset Downloads]");
            foreach (var item in downloads)
            {
                Debug.WriteLine("---------------------------------------------------------------");
                Debug.WriteLine($"Name: {item.Name}");
                Debug.WriteLine($"Size: {item.Size}");
                Debug.WriteLine($"Url:  {item.Url}");
                Debug.WriteLine($"Path: {item.Path}");
            }
        }
    }
}
