using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using GBCLV3.Models;
using GBCLV3.Utils;

namespace GBCLV3.Services
{
    class UpdateService
    {
        #region Private Members

        private const string CHECK_UPDATE_URL = "https://api.github.com/repos/Goose-Bomb/GBCLV3/releases/latest";

        private readonly HttpClient _client;

        #endregion

        #region Constructor

        public UpdateService()
        {
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

        public IEnumerable<DownloadItem> GetDownload(UpdateInfo info)
        {
            var item = new DownloadItem
            {
                Name = info.Name,
                Path = "GBCL.update",
                Url = info.Assets[0].Url,
                Size = info.Assets[0].Size,
                IsCompleted = false,
                DownloadedBytes = 0,
            };

            return new List<DownloadItem>(1) { item };
        }

        public void Update()
        {
            string currentPath = Application.ResourceAssembly.Location;
            string tempPath = Path.ChangeExtension(currentPath, "old");

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
