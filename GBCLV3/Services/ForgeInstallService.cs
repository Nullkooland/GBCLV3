using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Documents;
using GBCLV3.Models;
using GBCLV3.Models.JsonClasses;
using GBCLV3.Services.Launcher;

namespace GBCLV3.Services
{
    class ForgeInstallService
    {
        #region Private Members

        private const string FORGE_LIST_URL = "https://bmclapi2.bangbang93.com/forge/minecraft/";

        private readonly HttpClient _client;

        // IoC
        private readonly GamePathService _gamePathService;
        private readonly UrlService _urlService;
        private readonly VersionService _versionService;

        #endregion

        #region Constructor

        public ForgeInstallService(
            GamePathService gamePathService,
            UrlService urlService,
            VersionService versionService)
        {
            _gamePathService = gamePathService;
            _urlService = urlService;
            _versionService = versionService;

            _client = new HttpClient() { Timeout = System.TimeSpan.FromSeconds(10) };
        }

        #endregion

        #region Public Methods

        public async Task<IEnumerable<ForgeDownload>> GetDownloadListAsync(string id)
        {
            try
            {
                string json = await _client.GetStringAsync(FORGE_LIST_URL + id);
                var forgeList = JsonSerializer.Deserialize<List<JForgeVersion>>(json);

                return forgeList.Select(jforge =>
                    new ForgeDownload
                    {
                        Build = jforge.build,
                        Date = jforge.modified,
                        Branch = jforge.branch,
                        GameVersion = jforge.mcversion,
                        Version = jforge.version,
                    }
                );
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        public IEnumerable<DownloadItem> GetDownload(ForgeDownload forge)
        {
            string fullName = $"{forge.GameVersion}-{forge.Version}";
            string downloadName = fullName + (forge.Branch == null ? null : $"-{forge.Branch}");

            DownloadItem item = new DownloadItem
            {
                Name = $"Forge-{fullName}",
                Path = $"{_gamePathService.ForgeLibDir}/{fullName}/forge-{fullName}.jar",
                Url = $"{_urlService.Base.Forge}{downloadName}/forge-{downloadName}-universal.jar",
                IsCompleted = false,
                DownloadedBytes = 0,
            };

            return new List<DownloadItem>(1) { item };
        }

        public bool Install(ForgeDownload forge)
        {
            string fullName = $"{forge.GameVersion}-{forge.Version}";
            string jarPath = $"{_gamePathService.ForgeLibDir}/{fullName}/forge-{fullName}.jar";
            string jsonPath = $"{_gamePathService.VersionDir}/{fullName}/{fullName}.json";

            if (!File.Exists(jarPath))
            {
                return false;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));

            using (var archive = ZipFile.OpenRead(jarPath))
            {
                var entry = archive.GetEntry("version.json");

                using (var reader = new StreamReader(entry.Open(), Encoding.UTF8))
                {
                    var forgeJsonInstance = JsonSerializer.Deserialize<JVersion>(reader.ReadToEnd());
                    forgeJsonInstance.id = fullName;
                    string forgeJson = JsonSerializer.Serialize(forgeJsonInstance);

                    File.WriteAllText(jsonPath, forgeJson, Encoding.UTF8);
                    _versionService.AddNew(jsonPath);
                }
            }

            return true;
        }

        #endregion

    }
}
