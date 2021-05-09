using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using GBCLV3.Models.Download;
using GBCLV3.Models.Installation;
using GBCLV3.Services.Download;
using GBCLV3.Services.Launch;
using GBCLV3.Utils;
using StyletIoC;

namespace GBCLV3.Services.Authentication
{
    public class AuthlibInjectorService
    {
        #region Private Fields

        private readonly GamePathService _gamePathService;
        private readonly DownloadUrlService _downloadUrlService;
        private readonly LogService _logService;
        private readonly HttpClient _client;

        private AuthlibInjector _cached;

        #endregion

        #region Constructor

        [Inject]
        public AuthlibInjectorService(
            GamePathService gamePathService,
            DownloadUrlService downloadUrlService,
            LogService logService,
            HttpClient client)
        {
            _gamePathService = gamePathService;
            _downloadUrlService = downloadUrlService;
            _logService = logService;
            _client = client;
        }

        #endregion

        #region Public Methods

        public async ValueTask<AuthlibInjector> GetLatest()
        {
            if (_cached != null)
            {
                return _cached;
            }

            _logService.Info(nameof(AuthlibInjectorService), "Fetching latest download info");

            try
            {
                byte[] json = await _client.GetByteArrayAsync(_downloadUrlService.Base.AuthlibInjector);
                var info = JsonDocument.Parse(json).RootElement;

                _cached = new AuthlibInjector
                {
                    Build = info.GetProperty("build_number").GetInt32(),
                    Version = info.GetProperty("version").GetString(),
                    Url = info.GetProperty("download_url").GetString(),
                    SHA256 = info.GetProperty("checksums").GetProperty("sha256").GetString(),
                };

                _logService.Info(nameof(AuthlibInjectorService), $"Download info fetched. Version: {_cached.Version} Build: {_cached.Build}");

                return _cached;
            }
            catch (Exception ex)
            {
                _logService.Error(nameof(AuthlibInjectorService), $"Failed to fetch download info\n{ex.Message}");
                return null;
            }
        }

        public bool CheckIntegrity(string sha256)
        {
            string path = $"{_gamePathService.RootDir}/authlib-injector.jar";
            return !string.IsNullOrEmpty(sha256) && File.Exists(path) && CryptoUtil.ValidateFileSHA256(path, sha256);
        }

        public int GetLocalBuild()
        {
            string path = $"{_gamePathService.RootDir}/authlib-injector.jar";

            _logService.Info(nameof(AuthlibInjectorService), "Checking local authlib-injector");

            try
            {
                using var archive = ZipFile.OpenRead(path);
                var entry = archive.GetEntry("META-INF/MANIFEST.MF");

                using var infoStream = entry.Open();
                using var reader = new StreamReader(infoStream);

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("Build-Number:"))
                    {
                        int build = int.Parse(line[14..]);

                        _logService.Info(nameof(AuthlibInjectorService), $"Local authlib-injector checked. Build: {build}");

                        return build;
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Error(nameof(AuthlibInjectorService), $"Failed to check local authlib-injector\n{ex.Message}");
            }

            return -1;
        }

        public IEnumerable<DownloadItem> GetDownload(AuthlibInjector authlibInjector)
        {
            var item = new DownloadItem
            {
                Path = $"{_gamePathService.RootDir}/authlib-injector.jar",
                Url = authlibInjector.Url,
                IsCompleted = false
            };

            return Enumerable.Repeat(item, 1);
        }

        #endregion
    }
}