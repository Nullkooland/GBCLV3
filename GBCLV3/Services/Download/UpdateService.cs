using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GBCLV3.Models.Download;
using GBCLV3.Utils;
using StyletIoC;

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
        private readonly ConfigService _configService;
        private readonly HttpClient _client;
        private readonly LogService _logService;

        #endregion

        #region Constructor

        [Inject]
        public UpdateService(
            ConfigService configService, 
            HttpClient client,
            LogService logService)
        {
            _configService = configService;
            _client = client;
            _client.DefaultRequestHeaders.Add("User-Agent", "request");
            _logService = logService;
        }

        #endregion

        #region Public Methods

        public async ValueTask<UpdateInfo> CheckAsync()
        {
            if (_cachedInfo != null)
            {
                return _cachedInfo;
            }

            _logService.Info(nameof(UpdateService), "Checking launcher update");

            try
            {
                CheckStatusChanged?.Invoke(CheckUpdateStatus.Checking);
                byte[] json = await _client.GetByteArrayAsync(CHECK_UPDATE_URL);
                var info = JsonSerializer.Deserialize<UpdateInfo>(json);

                if (HasNewVersion(info.Version))
                {
                    _logService.Info(nameof(UpdateService), $"New update available. Version: {info.Version}");

                    CheckStatusChanged?.Invoke(CheckUpdateStatus.UpdateAvailable);
                    _cachedInfo = info;
                    return info;
                }
            }
            catch (Exception ex)
            {
                _logService.Error(nameof(UpdateService), $"Failed to check update.\n{ex.Message}");

                CheckStatusChanged?.Invoke(CheckUpdateStatus.CheckFailed);
                return null;
            }

            _logService.Info(nameof(UpdateService), "Launcher is up to date");

            CheckStatusChanged?.Invoke(CheckUpdateStatus.UpToDate);
            return null;
        }

        public async ValueTask<UpdateChangelog> GetChangelogAsync(UpdateInfo info)
        {
            _logService.Info(nameof(UpdateService), "Fetching changelog");

            var changelogAsset = info.Assets.Find(asset => asset.Name == "changelog.json");
            try
            {
                byte[] json = await _client.GetByteArrayAsync(changelogAsset.Url);
                var changelogByLang = JsonSerializer.Deserialize<Dictionary<string, UpdateChangelog>>(json);
                string langTag = _configService.Entries.Language;

                if (changelogByLang.TryGetValue(langTag, out var changelog))
                {
                    _logService.Info(nameof(UpdateService), $"Got changelog in {langTag}");

                    return changelog;
                }

                // Fallback to en-US changelog
                _logService.Info(nameof(UpdateService), $"Got fallback changelog in en-US");

                return changelogByLang["en-US"];
            }
            catch (Exception ex)
            {
                _logService.Error(nameof(UpdateService), $"Failed to fetch changelog.\n{ex.Message}");
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
            };

            return Enumerable.Repeat(item, 1);
        }

        public void Update()
        {
            _logService.Info(nameof(UpdateService), "Self-updating using PowerShell script");

            int currentPID = Process.GetCurrentProcess().Id;

            _logService.Info(nameof(UpdateService), $"Current PID: {currentPID}");

            // Remember to save log and config file before update.
            _logService.Finish();
            _configService.Save();

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
                _logService.Error(nameof(UpdateService), $"Faile to execute update script.\n{ex.Message}");
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