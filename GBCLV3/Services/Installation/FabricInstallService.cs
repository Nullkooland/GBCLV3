using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Documents;
using GBCLV3.Models;
using GBCLV3.Models.Installation;
using GBCLV3.Models.JsonClasses;
using GBCLV3.Services.Launcher;
using StyletIoC;
using Version = GBCLV3.Models.Launcher.Version;

namespace GBCLV3.Services.Installation
{
    class FabricInstallService
    {
        #region Private Members

        private const string FABRIC_LIST_URL = "https://meta.fabricmc.net//v2/versions/loader/";
        private const string FABRIC_MAVEN_URL = "https://maven.fabricmc.net/";

        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions
            = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, IgnoreNullValues = true };

        // IoC
        private readonly GamePathService _gamePathService;
        private readonly VersionService _versionService;

        #endregion

        #region Constructor

        [Inject]
        public FabricInstallService(
            GamePathService gamePathService,
            VersionService versionService)
        {
            _gamePathService = gamePathService;
            _versionService = versionService;

            _client = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };
        }

        #endregion

        #region Public Methods

        public async Task<IEnumerable<Fabric>> GetDownloadListAsync(string id)
        {
            string json = await _client.GetStringAsync(FABRIC_LIST_URL + id);
            return JsonSerializer.Deserialize<List<Fabric>>(json, _jsonOptions);
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

            string json = JsonSerializer.Serialize(jver, _jsonOptions);
            return _versionService.AddNew(json);
        }

        #endregion
    }
}
