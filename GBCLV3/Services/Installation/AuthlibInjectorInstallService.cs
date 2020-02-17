using System;
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
using StyletIoC;

namespace GBCLV3.Services.Installation
{
    public class AuthlibInjectorInstallService
    {
        #region Private Fields

        private readonly GamePathService _gamePathService;
        private readonly DownloadUrlService _downloadUrlService;
        private readonly HttpClient _client;

        private AuthlibInjector _chached;

        #endregion

        #region Constructor

        [Inject]
        public AuthlibInjectorInstallService(
            GamePathService gamePathService,
            DownloadUrlService downloadUrlService)
        {
            _gamePathService = gamePathService;
            _downloadUrlService = downloadUrlService;
            _client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        }

        #endregion

        #region Public Methods

        public async ValueTask<AuthlibInjector> GetLatest()
        {
            if (_chached != null) return _chached;

            try
            {
                var json = await _client.GetByteArrayAsync(_downloadUrlService.Base.AuthlibInjector);
                var info = JsonDocument.Parse(json).RootElement;

                _chached = new AuthlibInjector
                {
                    Build = info.GetProperty("build_number").GetInt32(),
                    Version = info.GetProperty("version").GetString(),
                    Url = info.GetProperty("download_url").GetString(),
                    SHA256 = info.GetProperty("checksums").GetProperty("sha256").GetString(),
                };

                return _chached;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return null;
            }
        }

        public int CheckLocalBuild()
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

        public DownloadItem GetDownload(AuthlibInjector authlibInjector)
        {
            return new DownloadItem
            {
                Path = $"{_gamePathService.RootDir}/authlib-injector.jar",
                Url = authlibInjector.Url,
                IsCompleted = false
            };
        }

        #endregion
    }
}