using GBCLV3.Models.Installation;
using GBCLV3.Models.Launch;
using GBCLV3.Services.Download;
using GBCLV3.Services.Launch;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Version = GBCLV3.Models.Launch.Version;

namespace GBCLV3.Services.Installation
{
    public class FabricInstallService
    {
        #region Private Fields

        private const string FABRIC_MAVEN_URL = "https://maven.fabricmc.net/";

        private readonly JsonSerializerOptions _jsonOptions
            = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IgnoreNullValues = true };

        // IoC
        private readonly GamePathService _gamePathService;
        private readonly DownloadUrlService _urlService;
        private readonly VersionService _versionService;
        private readonly LogService _logService;
        private readonly HttpClient _client;

        #endregion

        #region Constructor

        [Inject]
        public FabricInstallService(
            GamePathService gamePathService,
            DownloadUrlService downloadUrlService,
            VersionService versionService,
            LogService logService,
            HttpClient client)
        {
            _gamePathService = gamePathService;
            _urlService = downloadUrlService;
            _versionService = versionService;
            _logService = logService;
            _client = client;
        }

        #endregion

        #region Public Methods

        public async ValueTask<IEnumerable<Fabric>> GetDownloadListAsync(string jarID)
        {
            _logService.Info(nameof(FabricInstallService), $"Fetching download list for version \"{jarID}\"");

            try
            {
                var json = await _client.GetByteArrayAsync(_urlService.Base.Fabric + jarID);
                return JsonSerializer.Deserialize<List<Fabric>>(json, _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                _logService.Error(nameof(FabricInstallService), $"Failed to fetch download list: HTTP error\n{ex.Message}");
                return null;
            }
            catch (OperationCanceledException)
            {
                // Timeout
                _logService.Error(nameof(FabricInstallService), $"Failed to fetch download list: Timeout");
                return null;
            }

        }

        public Version Install(Fabric fabric)
        {
            _logService.Info(nameof(FabricInstallService), $"Installing fabric for \"{fabric.Intermediary.Version}\". Version: {fabric.Loader.Version} Build: {fabric.Loader.Build}");

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
