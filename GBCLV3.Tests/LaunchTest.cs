using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GBCLV3.Tests
{
    [TestClass]
    public class LaunchTest
    {
        private const string GAME_ROOT_DIR = "G:/Minecraft/1.12.2/.minecraft";
        private const string ID = "1.12.2-forge1.12.2-14.23.5.2838";

        private readonly ConfigService _configService;
        private readonly VersionService _versionService;
        private readonly LibraryService _libraryService;
        private readonly LaunchService _launchService;

        public LaunchTest()
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
            _libraryService = new LibraryService(gamePathService, urlServie);

            _launchService = new LaunchService(gamePathService);

            _versionService.LoadAll();
            Assert.IsTrue(_versionService.Any(), "No available versions!");
        }

        #region 附加测试特性
        //
        // 编写测试时，可以使用以下附加特性: 
        //
        // 在运行类中的第一个测试之前使用 ClassInitialize 运行代码
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // 在类中的所有测试都已运行之后使用 ClassCleanup 运行代码
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // 在运行每个测试之前，使用 TestInitialize 来运行代码
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // 在每个测试运行完之后，使用 TestCleanup 来运行代码
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void LaunchGameTest()
        {
            var version = _versionService.GetByID(ID);

            _libraryService.ExtractNatives(version.Libraries.Where(lib => lib.Type == LibraryType.Native));

            var authResult = AuthService.GetOfflineProfile("goose_bomb");

            var proile = new LaunchProfile
            {
                MaxMemory = 4096,
                Username = authResult.Username,
                UUID = authResult.UUID,
                AccessToken = authResult.AccessToken,
                UserType = authResult.UserType,
                VersionType = "GBCLV3",
                WinWidth = 854,
                WinHeight = 480,
                IsFullScreen = false,
            };

            _launchService.ErrorReceived += OnErrorReceived;
            _launchService.LogReceived += OnLogReceived;
            _launchService.Exited += OnGameExited;

            var result = _launchService.LaunchGameAsync(proile, version).Result;
            Debug.WriteLine(result);
        }

        private void OnGameExited(int exitCode)
        {
            Debug.WriteLine($"Exit Code: {exitCode}");
        }

        private void OnLogReceived(string msg)
        {
            Debug.WriteLine("Log: " + msg);
        }

        private void OnErrorReceived(string err)
        {
            Debug.WriteLine("Error: " + err);
        }
    }
}
