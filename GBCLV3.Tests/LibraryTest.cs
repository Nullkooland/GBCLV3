using System.Linq;
using System.Diagnostics;
using GBCLV3.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GBCLV3.Services.Launcher;
using GBCLV3.Models.Launcher;

namespace GBCLV3.Tests
{
    [TestClass]
    public class LibraryTest
    {
        private const string GAME_ROOT_DIR = "G:/Minecraft/1.12.2/.minecraft";
        private const string ID = "1.6.4";

        private readonly ConfigService _configService;
        private readonly VersionService _versionService;
        private readonly LibraryService _libraryService;

        public LibraryTest()
        {
            _configService = new ConfigService();
            _configService.Load();
            _configService.Entries.GameDir = GAME_ROOT_DIR;
            _configService.Entries.SelectedVersion = ID;

            var gamePathService = new GamePathService(_configService);
            var urlServie = new UrlService(_configService);

            _versionService = new VersionService(gamePathService, urlServie);
            _libraryService = new LibraryService(gamePathService, urlServie);

            _versionService.LoadAll();
            Assert.IsTrue(_versionService.HasAny(), "No available versions!");
        }

        [TestMethod]
        public void CheckDamagedLibrariesTest()
        {
            var version = _versionService.GetByID(ID);
            var damagedLibs = _libraryService.CheckIntegrity(version.Libraries);

            foreach (var lib in damagedLibs)
            {
                Debug.WriteLine(lib.Name);
            }
        }

        [TestMethod]
        public void ExtractNativeLibrariesTest()
        {
            var version = _versionService.GetByID(ID);
            _libraryService.ExtractNatives(version.Libraries.Where(lib => lib.Type == LibraryType.Native));
        }

        [TestMethod]
        public void GetDownloadInfoTest()
        {
            var version = _versionService.GetByID(ID);
            var (type, items) = _libraryService.GetDownloadInfo(version.Libraries);
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
