using System.Diagnostics;
using System.Linq;
using GBCLV3.Services;
using GBCLV3.Services.Download;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GBCLV3.Tests
{
    [TestClass]
    public class UpdateTest
    {
        private readonly UpdateService _updateService;

        public UpdateTest()
        {
            var configService = new ConfigService();
            configService.Load();

            _updateService = new UpdateService(configService);
        }

        [TestMethod]
        public void GetUpdateInfoTest()
        {
            var info = _updateService.Check().Result;

            if (info == null)
            {
                Debug.WriteLine("New Version Not Found");
            }
            else
            {
                Debug.WriteLine("New Version Found\n");

                Debug.WriteLine("[Info]");
                Debug.WriteLine($"Name:         {info.Name}");
                Debug.WriteLine($"Version:      {info.Version}");
                Debug.WriteLine($"PreRelease:   {info.PreRelease}");
                Debug.WriteLine($"ReleaseTime:  {info.Description}");

                var changelog = _updateService.GetChangelog(info).Result;

                Debug.WriteLine("[Changelog]");
                Debug.WriteLine($"Title:        {changelog.Title}");
                foreach (string detial in changelog.Details)
                {
                    Debug.WriteLine($"- {detial}");
                }

                var download = _updateService.GetDownload(info).FirstOrDefault();

                Debug.WriteLine("[Download]");
                Debug.WriteLine($"Name: {download.Name}");
                Debug.WriteLine($"Size: {download.Size}");
                Debug.WriteLine($"Url:  {download.Url}");
                Debug.WriteLine($"Path: {download.Path}");
            }
        }
    }
}
