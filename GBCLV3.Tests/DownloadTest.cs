using GBCLV3.Services.Download;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Diagnostics;

namespace GBCLV3.Tests
{
    [TestClass]
    class DownloadTest
    {
        [TestMethod]
        public void TestSingleDownload()
        {
            var items = new List<DownloadItem>() {
                new DownloadItem
                {
                    Url = "https://github.com/Goose-Bomb/GBCLV3/releases/download/3.0.7.59/GBCL.exe",
                    Path = "./downloads/gbcl.exe",
                    Size = 1569294,
                    IsCompleted = false,
                }
            };

            var downloadService = new DownloadService(items);
            downloadService.ProgressChanged += e =>
            {
                Debug.WriteLine("---------------------------------------------------------------");
                Debug.WriteLine($"Progress: {e.DownloadedBytes} Bytes");
                Debug.WriteLine($"Speed: {e.Speed} Bytes/sec");
            };

            bool isSccessful = downloadService.StartAsync().Result;

            Assert.IsTrue(isSccessful);
        }
    }
}
