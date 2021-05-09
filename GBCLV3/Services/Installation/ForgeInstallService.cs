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
using System.Windows;
using GBCLV3.Models.Download;
using GBCLV3.Models.Installation;
using GBCLV3.Models.Launch;
using GBCLV3.Services.Download;
using GBCLV3.Services.Launch;
using GBCLV3.Utils;
using StyletIoC;
using Version = GBCLV3.Models.Launch.Version;

namespace GBCLV3.Services.Installation
{
    public class ForgeInstallService
    {
        #region Events

        public event Action<string> InstallProgressChanged;

        #endregion

        #region Private Fields

        private const string FORGE_INSTALL_BOOTSTRAPPER =
            "pack://application:,,,/Resources/Tools/forge-install-bootstrapper.jar";

        // IoC
        private readonly GamePathService _gamePathService;
        private readonly DownloadUrlService _urlService;
        private readonly VersionService _versionService;
        private readonly LogService _logService;
        private readonly HttpClient _client;

        #endregion

        #region Constructor

        [Inject]
        public ForgeInstallService(
            GamePathService gamePathService,
            DownloadUrlService urlService,
            VersionService versionService,
            LogService logService,
            HttpClient client)
        {
            _gamePathService = gamePathService;
            _urlService = urlService;
            _versionService = versionService;
            _logService = logService;
            _client = client;
        }

        #endregion

        #region Public Methods

        public bool IsInstallerNeeded(Forge forge)
        {
            return forge.ReleaseTime.Year >= 2019;
        }

        public async ValueTask<IEnumerable<Forge>> GetDownloadListAsync(string jarID)
        {
            _logService.Info(nameof(ForgeInstallService), $"Fetching download list for version \"{jarID}\"");

            try
            {
                byte[] json = await _client.GetByteArrayAsync(_urlService.Base.ForgeList + jarID);
                var forgeList = JsonSerializer.Deserialize<List<JForgeVersion>>(json);

                return forgeList.Select(jforge =>
                    new Forge
                    {
                        Build = jforge.build,
                        Version = jforge.version,
                        ID = $"{jforge.mcversion}-forge-{jforge.version}",
                        FullName = $"{jforge.mcversion}-{jforge.version}" +
                                   (jforge.branch != null ? $"-{jforge.branch}" : null),
                        ReleaseTime = jforge.modified,
                    }
                ).OrderByDescending(forge => forge.Build);
            }
            catch (HttpRequestException ex)
            {
                _logService.Error(nameof(ForgeInstallService), $"Failed to fetch download list: HTTP error\n{ex.Message}");
                return null;
            }
            catch (OperationCanceledException)
            {
                // Timeout
                _logService.Error(nameof(ForgeInstallService), $"Failed to fetch download list: Timeout");
                return null;
            }
        }

        public IEnumerable<DownloadItem> GetDownload(Forge forge, bool isInstallerNeeded)
        {
            var item = new DownloadItem
            {
                Name = $"Forge-{forge.FullName}",

                Path = isInstallerNeeded
                    ? $"{_gamePathService.RootDir}/{forge.FullName}-installer.jar"
                    : $"{_gamePathService.ForgeLibDir}/{forge.FullName}/forge-{forge.FullName}.jar",

                Url = $"{_urlService.Base.Forge}{forge.FullName}/forge-{forge.FullName}" +
                      $"-{(isInstallerNeeded ? "installer" : "universal")}.jar",

                IsCompleted = false,
                DownloadedBytes = 0,
            };

            return Enumerable.Repeat(item, 1);
        }

        public IEnumerable<JLibrary> GetJLibraries(Forge forge)
        {
            string installerPath = $"{_gamePathService.RootDir}/{forge.FullName}-installer.jar";
            using var archive = ZipFile.OpenRead(installerPath);

            IEnumerable<JLibrary> jlibs;

            using (var memoryStream = new MemoryStream())
            {
                var versionEntry = archive.GetEntry("version.json");
                using var versionStream = versionEntry.Open();
                versionStream.CopyTo(memoryStream);
                var jver = JsonSerializer.Deserialize<JVersion>(memoryStream.ToArray());
                jlibs = jver.libraries;
            }

            using (var memoryStream = new MemoryStream())
            {
                var profileEntry = archive.GetEntry("install_profile.json");
                using var profileStream = profileEntry.Open();
                profileStream.CopyTo(memoryStream);
                var installProfile = JsonSerializer.Deserialize<JForgeInstallProfile>(memoryStream.ToArray());

                jlibs = jlibs.Union(installProfile.libraries);
            }

            return jlibs;
        }

        public Version InstallOld(Forge forge)
        {
            _logService.Info(nameof(ForgeInstallService), $"Installing old forge. Version: {forge.ID} Build: {forge.Build}");

            string jsonPath = $"{_gamePathService.VersionsDir}/{forge.ID}/{forge.ID}.json";
            string jarPath = $"{_gamePathService.ForgeLibDir}/{forge.FullName}/forge-{forge.FullName}.jar";

            if (!File.Exists(jarPath))
            {
                _logService.Warn(nameof(ForgeInstallService), $"Cannot find forge jar, installation aborted");

                return null;
            }

            using var archive = ZipFile.OpenRead(jarPath);
            var entry = archive.GetEntry("version.json");

            using var reader = new StreamReader(entry.Open(), Encoding.UTF8);
            string json = reader.ReadToEnd();

            // The early forge versions' naming conventions are just obnoxious
            json = Regex.Replace(json, "\"id\":\\s\".*\"", $"\"id\": \"{forge.ID}\"");

            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
            File.WriteAllText(jsonPath, json);

            return _versionService.AddNew(jsonPath);
        }

        public async ValueTask<Version> InstallAsync(Forge forge)
        {
            _logService.Info(nameof(ForgeInstallService), $"Installing forge. Version: {forge.ID} Build: {forge.Build}");

            // Just a dummy json...but required by forge installer
            string profilePath = $"{_gamePathService.RootDir}/launcher_profiles.json";
            if (!File.Exists(profilePath))
            {
                File.WriteAllText(profilePath, "{}");
            }

            // Extract forge-install-bootstrapper to disk
            // See https://github.com/bangbang93/forge-install-bootstrapper
            string bootstrapperPath = $"{_gamePathService.RootDir}/forge-install-bootstrapper.jar";
            var embeddedStream = Application.GetResourceStream(new Uri(FORGE_INSTALL_BOOTSTRAPPER)).Stream;
            var extractFileStream = File.OpenWrite(bootstrapperPath);

            embeddedStream.CopyTo(extractFileStream);
            embeddedStream.Dispose();
            extractFileStream.Dispose();

            _logService.Info(nameof(ForgeInstallService), "Install bootstrapper extracted");

            // Prepare arguments for bootstrapper
            string installerPath = $"{_gamePathService.RootDir}/{forge.FullName}-installer.jar";

            string args = $"-cp \"{bootstrapperPath};{installerPath}\" " +
                       "com.bangbang93.ForgeInstaller .";

            _logService.Debug(nameof(ForgeInstallService), $"Launching install bootstrapper, args:\n{args}");

            var startInfo = new ProcessStartInfo
            {
                FileName = _gamePathService.JavawPath,
                WorkingDirectory = _gamePathService.RootDir,
                Arguments = $"-cp \"{bootstrapperPath};{installerPath}\" " +
                            "com.bangbang93.ForgeInstaller .",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            };

            try
            {
                bool isSuccessful = false;

                var process = Process.Start(startInfo);
                process.EnableRaisingEvents = true;

                process.OutputDataReceived += (_, e) =>
                {
                    string message = e.Data;

                    if (message == "true")
                    {
                        isSuccessful = true;
                    }

                    InstallProgressChanged?.Invoke(message);
                };

                process.BeginOutputReadLine();
                await process.WaitForExitAsync();

                string jsonPath = $"{_gamePathService.VersionsDir}/{forge.ID}/{forge.ID}.json";

                if (!isSuccessful)
                {
                    // Cleanup remaining json
                    Directory.Delete(jsonPath);

                    _logService.Warn(nameof(ForgeInstallService), "Installation failed");

                    return null;
                }

                // Now we're ready to load the installed forge version
                return _versionService.AddNew(jsonPath);
            }
            catch (Exception ex)
            {
                _logService.Warn(nameof(ForgeInstallService), $"Installation failed\n{ex.Message}");

                return null;
            }
            finally
            {
                // Clean up
                File.Delete(bootstrapperPath);
                File.Delete(installerPath);
            }
        }

        #endregion

        #region Helpers

        #endregion
    }
}