using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using GBCLV3.Models;
using GBCLV3.Utils;
using StyletIoC;

namespace GBCLV3.Services
{
    class UpdateService
    {
        #region Private Members

        private const string CHECK_UPDATE_URL = "https://api.github.com/repos/Goose-Bomb/GBCLV3/releases/latest";

        private readonly HttpClient _client;

        // IoC
        private readonly Config _config;

        #endregion

        #region Constructor

        [Inject]
        public UpdateService(ConfigService configService)
        {
            _config = configService.Entries;

            _client = new HttpClient() { Timeout = TimeSpan.FromSeconds(30) };
            _client.DefaultRequestHeaders.Add("User-Agent", "request");
        }

        #endregion

        #region Public Methods

        public async Task<UpdateInfo> Check()
        {
            try
            {
                string json = await _client.GetStringAsync(CHECK_UPDATE_URL);
                var info = JsonSerializer.Deserialize<UpdateInfo>(json);
                info.IsCheckFailed = false;

                if (HasNewVersion(info.Version)) return info;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return new UpdateInfo { IsCheckFailed = true };
            }

            return null;
        }

        public async Task<UpdateChangelog> GetChangelog(UpdateInfo info)
        {
            var changelogAsset = info.Assets.Find(asset => asset.Name == "changelog.json");

            try
            {
                string json = await _client.GetStringAsync(changelogAsset.Url);
                var dictByLang = JsonSerializer.Deserialize<Dictionary<string, UpdateChangelog>>(json);

                return dictByLang[_config.Language];
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        public IEnumerable<DownloadItem> GetDownload(UpdateInfo info)
        {
            var executableAsset = info.Assets.Find(asset => asset.Name == "GBCL.exe");

            var item = new DownloadItem
            {
                Name = executableAsset.Name,
                Path = "GBCL.update",
                Url = executableAsset.Url,
                Size = executableAsset.Size,
                IsCompleted = false,
                DownloadedBytes = 0,
            };

            return new List<DownloadItem>(1) { item };
        }

        public void Update()
        {
            string currentPath = Application.ResourceAssembly.Location;
            string tempPath = Path.ChangeExtension(currentPath, "old");

            // 🌶️💉💧🐮🍺
            // This is magic...
            File.Delete(tempPath);
            File.Move(currentPath, tempPath);
            File.Move("GBCL.update", currentPath);

            Process.Start(currentPath, "updated");
            Application.Current.Shutdown();
        }

        #endregion

        #region Private Methods

        private static bool HasNewVersion(string remoteVersion)
        {
            int localVersionNum = int.Parse(AssemblyUtil.Version.Split('.').Last());
            int remoteVersionNum = int.Parse(remoteVersion.Split('.').Last());
            return (remoteVersionNum > localVersionNum);
        }

        #endregion
    }
}
