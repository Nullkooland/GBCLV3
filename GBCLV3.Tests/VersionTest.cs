using GBCLV3.Services;
using GBCLV3.Services.Launcher;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Linq;

namespace GBCLV3.Tests
{
    [TestClass]
    public class VersionTest
    {
        private const string GAME_ROOT_DIR = "G:/Minecraft/1.12.2/.minecraft";
        private const string ID = "1.12.2";

        private readonly ConfigService _configService;
        private readonly VersionService _versionService;

        public  VersionTest()
        {
            _configService = new ConfigService();
            _configService.Load();
            _configService.Entries.GameDir = GAME_ROOT_DIR;

            var gamePathService = new GamePathService(_configService);
            var urlServie = new UrlService(_configService);

            _versionService = new VersionService(gamePathService, urlServie);

            _versionService.LoadAll();
            Assert.IsTrue(_versionService.HasAny(), "No available versions!");

            foreach (var version in _versionService.GetAvailable())
            {
                Debug.WriteLine(version.ID);
                //Debug.WriteLine();
            }
        }


        [TestMethod]
        public void InheritedVersionTest()
        {
            var version = _versionService.GetByID(ID);
            var parent = _versionService.GetByID(version.InheritsFrom);

            if (parent != null)
            {
                version.Libraries = parent.Libraries.Union(version.Libraries).ToList();
                version.AssetsInfo = parent.AssetsInfo;
            }

            Debug.WriteLine($"Target Version: {version.ID}");

            Debug.WriteLine("[Merged Libraries]");
            foreach (var lib in version.Libraries)
            {
                Debug.WriteLine("---------------------------------------------------------------");
                Debug.WriteLine($"Name: {lib.Name}");
                Debug.WriteLine($"Size: {lib.Size}");
                Debug.WriteLine($"Path: {lib.Path}");
                Debug.WriteLine($"SHA1: {lib.SHA1}");
            }
        }
    }
}
