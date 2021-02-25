using GBCLV3.Models;
using GBCLV3.Models.Download;
using GBCLV3.Utils;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace GBCLV3.Services.Download
{
    public class UpdateService
    {
        #region Events

        public event Action<CheckUpdateStatus> CheckStatusChanged;

        #endregion

        #region Private Fields

        private const string CHECK_UPDATE_URL = "https://api.github.com/repos/Goose-Bomb/GBCLV3/releases/latest";

        private UpdateInfo _cachedInfo;

        // IoC
        private readonly Config _config;
        private readonly HttpClient _client;

        #endregion

        #region Constructor

        [Inject]
        public UpdateService(ConfigService configService, HttpClient client)
        {
            _config = configService.Entries;
            _client = client;
            _client.DefaultRequestHeaders.Add("User-Agent", "request");
        }

        #endregion

        #region Public Methods

        public async ValueTask<UpdateInfo> CheckAsync()
        {
            if (_cachedInfo != null) return _cachedInfo;

            try
            {
                CheckStatusChanged?.Invoke(CheckUpdateStatus.Checking);
                var json = await _client.GetByteArrayAsync(CHECK_UPDATE_URL);
                var info = JsonSerializer.Deserialize<UpdateInfo>(json);

                if (HasNewVersion(info.Version))
                {
                    CheckStatusChanged?.Invoke(CheckUpdateStatus.UpdateAvailable);
                    _cachedInfo = info;
                    return info;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                CheckStatusChanged?.Invoke(CheckUpdateStatus.CheckFailed);
                return null;
            }

            CheckStatusChanged?.Invoke(CheckUpdateStatus.UpToDate);
            return null;
        }

        public async ValueTask<UpdateChangelog> GetChangelogAsync(UpdateInfo info)
        {
            var changelogAsset = info.Assets.Find(asset => asset.Name == "changelog.json");

            try
            {
                var json = await _client.GetByteArrayAsync(changelogAsset.Url);
                var changelogByLang = JsonSerializer.Deserialize<Dictionary<string, UpdateChangelog>>(json);

                return changelogByLang.ContainsKey(_config.Language)
                    ? changelogByLang[_config.Language]
                    : changelogByLang["en-US"];
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

            return new ImmutableArray<DownloadItem> { item };
        }

        public void Update()
        {
            int currentPID = Process.GetCurrentProcess().Id;
            string psCommand =
                $"Stop-Process -Id {currentPID} -Force;" +
                $"Wait-Process -Id {currentPID} -ErrorAction SilentlyContinue;" +
                "Start-Sleep -Milliseconds 500;" +
                "Remove-Item GBCL.exe -Force;" +
                "Rename-Item GBCL.update GBCL.exe;" +
                "Start-Process GBCL.exe -Args updated;";

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = psCommand,
                    WorkingDirectory = Directory.GetCurrentDirectory(),
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        #endregion

        #region Private Methods

        private static bool HasNewVersion(string remoteVersion)
        {
            int localVersionNum = int.Parse(AssemblyUtil.Version.Split('.').Last());
            int remoteVersionNum = int.Parse(remoteVersion.Split('.').Last());
            return remoteVersionNum > localVersionNum;
        }

        #endregion
    }
}