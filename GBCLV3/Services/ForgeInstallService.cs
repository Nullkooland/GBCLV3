using System;
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
using Version = GBCLV3.Models.Launcher.Version;

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

        public async Task<IEnumerable<Forge>> GetDownloadListAsync(string id)
        {
            try
            {
                var json = await _client.GetStringAsync(FORGE_LIST_URL + id);
                var forgeList = JsonSerializer.Deserialize<List<JForgeVersion>>(json);

                return forgeList.Select(jforge =>
                    new Forge
                    {
                        Build = jforge.build,
                        ReleaseTime = jforge.modified,
                        Branch = jforge.branch,
                        GameVersion = jforge.mcversion,
                        Version = jforge.version,
                    }
                ).OrderByDescending(forge => forge.Build);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
            catch (OperationCanceledException)
            {
                // Timeout
                Debug.WriteLine("[ERROR] Get forge download list timeout");
                return null;
            }
        }

        public IEnumerable<DownloadItem> GetDownload(Forge forge, bool isAutoInstall)
        {
            var fullName = $"{forge.GameVersion}-{forge.Version}";

            DownloadItem item = new DownloadItem
            {
                Name = $"Forge-{fullName}",

                Path = isAutoInstall ? $"{_gamePathService.ForgeLibDir}/{fullName}/forge-{fullName}.jar" 
                                     : $"{_gamePathService.RootDir}/{fullName}-installer.jar",

                Url = isAutoInstall ? $"{_urlService.Base.Forge}{fullName}/forge-{fullName}-universal.jar"
                                    : $"{_urlService.Base.Forge}{fullName}/forge-{fullName}-installer.jar",

                IsCompleted = false,
                DownloadedBytes = 0,
            };

            return new List<DownloadItem>(1) { item };
        }

        public async Task<Version> ManualInstall(Forge forge)
        {
            var id = $"{forge.GameVersion}-forge-{forge.Version}";
            var jsonPath = $"{_gamePathService.VersionsDir}/{id}/{id}.json";
            var installerPath = $"{_gamePathService.RootDir}/{forge.GameVersion}-{forge.Version}-installer.jar";

            try
            {
                var profilePath = $"{_gamePathService.RootDir}/launcher_profiles.json";
                // Just a dummy json...but required by forge installer
                if (!File.Exists(profilePath)) File.WriteAllText(profilePath, "{}");

                var process = Process.Start(installerPath);
                await Task.Run(() => process.WaitForExit());
                File.Delete(installerPath);

                if (!File.Exists(jsonPath)) return null;

                var json = File.ReadAllText(jsonPath, Encoding.UTF8);
                return _versionService.AddNew(json);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        public Version AutoInstall(Forge forge)
        {
            var fullName = $"{forge.GameVersion}-{forge.Version}";
            var jarPath = $"{_gamePathService.ForgeLibDir}/{fullName}/forge-{fullName}.jar";

            if (!File.Exists(jarPath))
            {
                return null;
            }

            using (var archive = ZipFile.OpenRead(jarPath))
            {
                var entry = archive.GetEntry("version.json");

                using (var reader = new StreamReader(entry.Open(), Encoding.UTF8))
                {
                    return _versionService.AddNew(reader.ReadToEnd());
                }
            }
        }

        #endregion

    }
}
