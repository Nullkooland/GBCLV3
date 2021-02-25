using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
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
        private readonly HttpClient _client;

        private AuthlibInjector _cached;

        #endregion

        #region Constructor

        [Inject]
        public AuthlibInjectorService(
            GamePathService gamePathService,
            DownloadUrlService downloadUrlService,
            HttpClient client)
        {
            _gamePathService = gamePathService;
            _downloadUrlService = downloadUrlService;
            _client = client;
        }

        #endregion

        #region Public Methods

        public async ValueTask<AuthlibInjector> GetLatest()
        {
            if (_cached != null) return _cached;

            try
            {
                var json = await _client.GetByteArrayAsync(_downloadUrlService.Base.AuthlibInjector);
                var info = JsonDocument.Parse(json).RootElement;

                _cached = new AuthlibInjector
                {
                    Build = info.GetProperty("build_number").GetInt32(),
                    Version = info.GetProperty("version").GetString(),
                    Url = info.GetProperty("download_url").GetString(),
                    SHA256 = info.GetProperty("checksums").GetProperty("sha256").GetString(),
                };

                return _cached;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
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
                        return int.Parse(line[14..]);
                    }
                }
            }
            catch
            {
                Debug.WriteLine("Failed to get local authlib-injector");
            }

            return -1;
        }

        public IEnumerable<DownloadItem> GetDownload(AuthlibInjector authlibInjector)
        {
            var item=  new DownloadItem
            {
                Path = $"{_gamePathService.RootDir}/authlib-injector.jar",
                Url = authlibInjector.Url,
                IsCompleted = false
            };

            return new ImmutableArray<DownloadItem> { item };
        }

        #endregion
    }
}