using System.Linq;
using System.Diagnostics;
using GBCLV3.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GBCLV3.Services.Launcher;

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
            Assert.IsTrue(_versionService.HasAny(), "No available versions!");
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

            var (type, items) = _assetService.GetDownloadInfo(version.AssetsInfo.Objects.Values);
            int totalBytes = items.Sum(obj => obj.Size);

            Debug.WriteLine($"Type: {type.ToString()}");
            Debug.WriteLine($"Items Count: {items.Count()}");
            Debug.WriteLine($"Items TotalBytes: {totalBytes}");

            Debug.WriteLine("[Item Infos]");

            foreach (var item in items)
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
