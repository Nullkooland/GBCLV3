using System.Diagnostics;
using GBCLV3.Models.Download;
using GBCLV3.Services;
using GBCLV3.Services.Authentication;
using GBCLV3.Services.Download;
using GBCLV3.Services.Launch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StyletIoC;

namespace GBCLV3.Tests.Services.Authentication
{
    [TestClass]
    public class AuthlibInjectorServiceTests
    {
        private IContainer _ioc;

        [TestInitialize]
        public void Init()
        {
            var configService = new ConfigService();
            configService.Load();
            
            var builder = new StyletIoCBuilder();
            builder.Bind<ConfigService>().ToInstance(configService);
            builder.Bind<GamePathService>().ToSelf().InSingletonScope();
            builder.Bind<DownloadUrlService>().ToSelf().InSingletonScope();
            builder.Bind<AuthlibInjectorService>().ToSelf().InSingletonScope();

            _ioc = builder.BuildContainer();
        }

        [TestMethod]
        [DataRow(DownloadSource.Official)]
        [DataRow(DownloadSource.BMCLAPI)]
        [DataRow(DownloadSource.MCBBS)]
        public void GetLatestTest(DownloadSource downloadSource)
        {
            var config= _ioc.Get<ConfigService>().Entries;
            config.DownloadSource = downloadSource;

            var authlibInjectorInstallService = _ioc.Get<AuthlibInjectorService>();
            var latest = authlibInjectorInstallService.GetLatest().Result;
            Assert.IsNotNull(latest);
            Assert.IsTrue(latest.Build > 0);
            Assert.IsNotNull(latest.Url);

            Debug.WriteLine("---------------------------------------------------------------");
            Debug.WriteLine($"Build:        {latest.Build}");
            Debug.WriteLine($"Version:      {latest.Version}");
            Debug.WriteLine($"Url:          {latest.Url}");
            Debug.WriteLine($"SHA-256:      {latest.SHA256}");
        }

        [TestMethod]
        public void CheckLocalBuildTest()
        {
            var authlibInjectorInstallService = _ioc.Get<AuthlibInjectorService>();
            int build = authlibInjectorInstallService.GetLocalBuild();
            Assert.IsTrue(build > 0);
            Debug.WriteLine($"Authlib-Injector Build Number: {build}");
        }
    }
}