using GBCLV3.Models;
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
        public void GetDownloadsTest()
        {
            var downloads = _versionService.GetAvailable()
                                           .Select(version => _versionService.GetDownload(version))
                                           .Aggregate((prev, current) => prev.Concat(current));

            int totalBytes = downloads.Sum(obj => obj.Size);

            Debug.WriteLine($"Type: {DownloadType.MainJar}");
            Debug.WriteLine($"downloads Count: {downloads.Count()}");
            Debug.WriteLine($"downloads TotalBytes: {totalBytes}");

            Debug.WriteLine("[Main Jar Downloads]");
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
