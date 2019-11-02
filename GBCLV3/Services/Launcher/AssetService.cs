using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GBCLV3.Models;
using GBCLV3.Models.JsonClasses;
using GBCLV3.Models.Launcher;
using StyletIoC;

namespace GBCLV3.Services.Launcher
{
    class AssetService
    {
        #region Private Members

        private readonly HttpClient _client;

        // IoC
        private readonly GamePathService _gamePathService;
        private readonly UrlService _urlService;

        #endregion

        #region Constructor

        [Inject]
        public AssetService(GamePathService gamePathService, UrlService urlService)
        {
            _gamePathService = gamePathService;
            _urlService = urlService;

            _client = new HttpClient() { Timeout = TimeSpan.FromSeconds(15) };
        }

        #endregion

        #region Public Methods

        public bool LoadAllObjects(AssetsInfo info)
        {
            string jsonPath = $"{_gamePathService.AssetsDir}/indexes/{info.ID}.json";
            if (!File.Exists(jsonPath)) return false;

            if (info.Objects != null) return true;

            using var reader = new StreamReader(jsonPath, Encoding.UTF8);
            var opetions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var jasset = JsonSerializer.Deserialize<JAsset>(reader.ReadToEnd(), opetions);

            info.Objects = jasset.objects;
            return true;
        }

        public async Task<IEnumerable<AssetObject>> CheckIntegrityAsync(AssetsInfo info)
        {
            return await Task.Run(() =>
            info.Objects?
                .Select(pair => pair.Value)
                .AsParallel()
                .Where(obj =>
                {
                    string path = $"{_gamePathService.AssetsDir}/objects/{obj.Path}";
                    return !File.Exists(path) || obj.Hash != Utils.CryptUtil.GetFileSHA1(path);
                })
                .ToList()
            );
        }

        public async Task CopyToVirtualAsync(AssetsInfo info)
        {
            // For legacy versions (1.7.2 or earlier) only!
            if (!info.IsLegacy) return;

            await Task.Run(() =>
            info.Objects?
                .AsParallel()
                .ForAll(pair =>
                {
                    string objectPath = $"{_gamePathService.AssetsDir}/objects/{pair.Value.Path}";
                    string virtualPath = $"{_gamePathService.AssetsDir}/virtual/legacy/{pair.Key}";
                    string virtualDir = Path.GetDirectoryName(virtualPath);

                    if (!File.Exists(objectPath) || File.Exists(virtualPath)) return;

                    // Make sure directory exists
                    Directory.CreateDirectory(virtualDir);
                    File.Copy(objectPath, virtualPath);
                })
            );
        }

        public async Task<bool> DownloadIndexJsonAsync(AssetsInfo info)
        {
            try
            {
                string json = await _client.GetStringAsync(info.IndexUrl);
                string indexDir = $"{_gamePathService.AssetsDir}/indexes";

                //Make sure directory exists
                Directory.CreateDirectory(indexDir);
                File.WriteAllText($"{indexDir}/{info.ID}.json", json, Encoding.UTF8);
                return true;
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex.ToString());
                return false;
            }
            catch (OperationCanceledException)
            {
                Debug.WriteLine("[ERROR] Index json download time out");
                return false;
            }
        }

        public IEnumerable<DownloadItem> GetDownloads(IEnumerable<AssetObject> assetObjects)
        {
            return assetObjects.Select(obj =>
            new DownloadItem
            {
                Name = obj.Hash,
                Path = $"{_gamePathService.AssetsDir}/objects/{obj.Path}",
                Url = _urlService.Base.Asset + obj.Path,
                Size = obj.Size,
                IsCompleted = false,
                DownloadedBytes = 0,
            }).ToList();
        }

        #endregion
    }
}
