using GBCLV3.Models.Download;
using GBCLV3.Models.Installation;
using GBCLV3.Services.Download;
using GBCLV3.Services.Launch;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Version = GBCLV3.Models.Launch.Version;

namespace GBCLV3.Services.Installation
{
    class ForgeInstallService
    {
        #region Private Fields

        private readonly HttpClient _client;

        // IoC
        private readonly GamePathService _gamePathService;
        private readonly DownloadUrlService _urlService;
        private readonly VersionService _versionService;

        #endregion

        #region Constructor

        [Inject]
        public ForgeInstallService(
            GamePathService gamePathService,
            DownloadUrlService urlService,
            VersionService versionService)
        {
            _gamePathService = gamePathService;
            _urlService = urlService;
            _versionService = versionService;

            _client = new HttpClient() { Timeout = TimeSpan.FromSeconds(10) };
        }

        #endregion

        #region Public Methods

        public async ValueTask<IEnumerable<Forge>> GetDownloadListAsync(string id)
        {
            try
            {
                var json = await _client.GetByteArrayAsync(_urlService.Base.ForgeList + id);
                var forgeList = JsonSerializer.Deserialize<List<JForgeVersion>>(json);

                int[] nums = id.Split('.')
                    .Select(numStr =>
                    {
                        if (int.TryParse(numStr, out int num))
                        {
                            return num;
                        }

                        return -1;
                    })
                    .ToArray();

                bool hasSuffix = ((nums[1] == 7 || nums[1] == 8) && nums[2] != 2);
                bool isAutoInstall = (nums[1] < 13);

                return forgeList.Select(jforge =>
                    new Forge
                    {
                        Version = jforge.version,
                        Build = jforge.build,
                        ReleaseTime = jforge.modified,
                        Branch = jforge.branch,
                        GameVersion = jforge.mcversion,
                        HasSuffix = hasSuffix,
                        IsAutoInstall = isAutoInstall,
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
                // AuthTimeout
                Debug.WriteLine("[ERROR] Get forge download list timeout");
                return null;
            }
        }

        public IEnumerable<DownloadItem> GetDownload(Forge forge)
        {
            string fullName = $"{forge.GameVersion}-{forge.Version}" +
                              (forge.HasSuffix ? $"-{forge.GameVersion}" : null);

            var item = new DownloadItem
            {
                Name = $"Forge-{fullName}",

                Path = forge.IsAutoInstall
                    ? $"{_gamePathService.ForgeLibDir}/{fullName}/forge-{fullName}.jar"
                    : $"{_gamePathService.RootDir}/{fullName}-installer.jar",

                Url = forge.IsAutoInstall
                    ? $"{_urlService.Base.Forge}{fullName}/forge-{fullName}-universal.jar"
                    : $"{_urlService.Base.Forge}{fullName}/forge-{fullName}-installer.jar",

                IsCompleted = false,
                DownloadedBytes = 0,
            };

            return new[] { item };
        }

        public async ValueTask<Version> ManualInstallAsync(Forge forge)
        {
            string id = $"{forge.GameVersion}-forge-{forge.Version}";
            string jsonPath = $"{_gamePathService.VersionsDir}/{id}/{id}.json";
            string installerPath = $"{_gamePathService.RootDir}/{forge.GameVersion}-{forge.Version}-installer.jar";

            try
            {
                string profilePath = $"{_gamePathService.RootDir}/launcher_profiles.json";
                // Just a dummy json...but required by forge installer
                if (!File.Exists(profilePath)) File.WriteAllText(profilePath, "{}");

                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = installerPath,
                    UseShellExecute = true,
                });

                await Task.Run(() => process.WaitForExit());
                File.Delete(installerPath);
                File.Delete($"{forge.GameVersion}-{forge.Version}-installer.jar.log");

                return _versionService.AddNew(jsonPath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
        }

        public Version AutoInstall(Forge forge)
        {
            string id = $"{forge.GameVersion}-forge-{forge.Version}";
            string jsonPath = $"{_gamePathService.VersionsDir}/{id}/{id}.json";
            string fullName = $"{forge.GameVersion}-{forge.Version}" +
                              (forge.HasSuffix ? $"-{forge.GameVersion}" : null);
            string jarPath = $"{_gamePathService.ForgeLibDir}/{fullName}/forge-{fullName}.jar";

            if (!File.Exists(jarPath))
            {
                return null;
            }

            using var archive = ZipFile.OpenRead(jarPath);
            var entry = archive.GetEntry("version.json");

            using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
            string json = reader.ReadToEnd();

            json = Regex.Replace(json, "\"id\":\\s\".*\"", $"\"id\": \"{id}\"");

            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
            File.WriteAllText(jsonPath, json);

            return _versionService.AddNew(jsonPath);
        }

        #endregion
    }
}