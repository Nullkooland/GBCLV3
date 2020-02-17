using GBCLV3.Models.Installation;
using GBCLV3.Models.Launch;
using GBCLV3.Services.Download;
using GBCLV3.Services.Launch;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Version = GBCLV3.Models.Launch.Version;

namespace GBCLV3.Services.Installation
{
    class FabricInstallService
    {
        #region Private Fields

        private const string FABRIC_MAVEN_URL = "https://maven.fabricmc.net/";

        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions
            = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IgnoreNullValues = true };

        // IoC
        private readonly GamePathService _gamePathService;
        private readonly DownloadUrlService _urlService;
        private readonly VersionService _versionService;

        #endregion

        #region Constructor

        [Inject]
        public FabricInstallService(
            GamePathService gamePathService,
            DownloadUrlService downloadUrlService,
            VersionService versionService)
        {
            _gamePathService = gamePathService;
            _urlService = downloadUrlService;
            _versionService = versionService;

            _client = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };
        }

        #endregion

        #region Public Methods

        public async ValueTask<IEnumerable<Fabric>> GetDownloadListAsync(string id)
        {
            try
            {
                var json = await _client.GetByteArrayAsync(_urlService.Base.Fabric + id);
                return JsonSerializer.Deserialize<List<Fabric>>(json, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
            catch (OperationCanceledException)
            {
                // AuthTimeout
                Debug.WriteLine("[ERROR] Get fabric download list timeout");
                return null;
            }

        }

        public Version Install(Fabric fabric)
        {
            var jver = new JVersion
            {
                id = $"{fabric.Intermediary.Version}-fabric-{fabric.Loader.Version}",
                inheritsFrom = fabric.Intermediary.Version,
                type = "release",
                mainClass = "net.fabricmc.loader.launch.knot.KnotClient",
                arguments = new JArguments(),
                libraries = fabric.LauncherMeta.Libraries.Common
                                  .Concat(fabric.LauncherMeta.Libraries.Client)
                                  .Concat(fabric.LauncherMeta.Libraries.Server)
                                  .Append(new JLibrary { name = fabric.Intermediary.Maven, url = FABRIC_MAVEN_URL })
                                  .Append(new JLibrary { name = fabric.Loader.Maven, url = FABRIC_MAVEN_URL })
                                  .ToList(),
            };
            jver.arguments.game = new List<JsonElement>(0);

            string jsonPath = $"{_gamePathService.VersionsDir}/{jver.id}/{jver.id}.json";
            var json = JsonSerializer.SerializeToUtf8Bytes(jver, _jsonOptions);

            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
            File.WriteAllBytes(jsonPath, json);
            return _versionService.AddNew(jsonPath);
        }

        #endregion
    }
}
