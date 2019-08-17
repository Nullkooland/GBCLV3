using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Documents;
using GBCLV3.Models;
using GBCLV3.Models.JsonClasses;
using GBCLV3.Models.Launcher;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;
using Version = GBCLV3.Models.Launcher.Version;

namespace GBCLV3.Services.Launcher
{
    class VersionService
    {
        #region Events 

        public event Action<bool> Loaded;

        public event Action<Version> Deleted;

        public event Action<Version> Created;

        #endregion

        #region Private Members

        private readonly Dictionary<string, Version> _versions;

        private readonly HttpClient _client;

        // IoC
        private readonly GamePathService _gamePathService;
        private readonly UrlService _urlService;

        #endregion

        #region Constructor

        [Inject]
        public VersionService(GamePathService gamePathService, UrlService urlService)
        {
            _gamePathService = gamePathService;
            _urlService = urlService;
            _versions = new Dictionary<string, Version>(8);

            _client = new HttpClient() { Timeout = System.TimeSpan.FromSeconds(15) };
        }

        #endregion

        #region Public Methods

        public void LoadAll()
        {
            _versions.Clear();

            if (!Directory.Exists(_gamePathService.VersionDir))
            {
                Loaded?.Invoke(false);
                return;
            }

            var allVersions =
                Directory.EnumerateDirectories(_gamePathService.VersionDir)
                         .Select(dir => $"{dir}/{Path.GetFileName(dir)}.json")
                         .Select(jsonPath => Load(jsonPath))
                         .Where(version => version != null);

            foreach (var verion in allVersions)
            {
                _versions.Add(verion.ID, verion);
            }

            Loaded?.Invoke(_versions.Any());
        }

        public bool HasAny() => _versions.Any();

        public bool HasVersion(string id) => (id != null) ? _versions.ContainsKey(id) : false;

        public IEnumerable<Version> GetAvailable() => _versions.Values;

        public Version GetByID(string id)
        {
            if (id != null && _versions.TryGetValue(id, out var version))
            {
                return version;
            }

            return null;
        }

        public void AddNew(string jsonPath)
        {
            var newVersion = Load(jsonPath);
            _versions.Add(newVersion.ID, newVersion);
            Created?.Invoke(newVersion);
        }

        public async Task DeleteFromDiskAsync(string id)
        {
            if (_versions.TryGetValue(id, out var versionToDelete))
            {
                await SystemUtil.SendDirToRecycleBin($"{_gamePathService.VersionDir}/{id}");
                Deleted?.Invoke(versionToDelete);
                _versions.Remove(id);

                // Clear libraries
                var usedLibs = _versions.Values
                                        .Select(version => version.Libraries as IEnumerable<Library>)
                                        .Aggregate((prev, current) => prev.Union(current));

                foreach (var libToDelete in versionToDelete.Libraries.Except(usedLibs))
                {
                    var libPath = $"{_gamePathService.LibDir }/{libToDelete.Path}";
                    await SystemUtil.SendFileToRecycleBin(libPath);
                    SystemUtil.DeleteEmptyDirs(Path.GetDirectoryName(libPath));
                }
            }
        }

        public async Task<(IEnumerable<VersionDownload>, LatestVersion)> GetDownloadListAsync()
        {
            try
            {
                string json = await _client.GetStringAsync(_urlService.Base.VersionList);
                var versionList = JsonSerializer.Deserialize<JVersionList>(json);

                var downloads = versionList.versions.Select(download =>
                    new VersionDownload
                    {
                        ID = download.id,
                        Url = download.url.Substring(32),
                        ReleaseTime = download.releaseTime,
                        Type = download.type == "release" ? VersionType.Release : VersionType.Snapshot,
                    });

                var latestVersion = new LatestVersion
                {
                    Release = versionList.latest.release,
                    Snapshot = versionList.latest.snapshot,
                };

                return (downloads, latestVersion);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex.ToString());
                return (null, null);
            }
            catch (OperationCanceledException)
            {
                // Timeout
                Debug.WriteLine("[ERROR] Get version download list timeout");
                return (null, null);
            }
        }

        public bool CheckJarIntegrity(Version version)
        {
            var jarPath = $"{_gamePathService.VersionDir}/{version.JarID}/{version.JarID}.jar";
            return File.Exists(jarPath) && (CryptUtil.GetFileSHA1(jarPath) == version.SHA1);
        }

        public (DownloadType, IEnumerable<DownloadItem>) GetJarDownloadInfo(Version version)
        {
            var item = new DownloadItem
            {
                Name = version.JarID + "jar",
                Path = $"{_gamePathService.VersionDir}/{version.JarID}/{version.JarID}.jar",
                Url = _urlService.Base.Version + version.Url,
                Size = version.Size,
                IsCompleted = false,
                DownloadedBytes = 0,
            };

            return (DownloadType.MainJar, new List<DownloadItem>(1) { item });
        }

        #endregion

        #region Private Methods

        private static Version Load(string jsonPath)
        {
            JVersion jver;
            try
            {
                string json = File.ReadAllText(jsonPath, Encoding.UTF8);
                jver = JsonSerializer.Deserialize<JVersion>(json);
            }
            catch
            {
                return null;
            }

            if (!IsValidVersion(jver))
            {
                return null;
            }

            var version = new Version
            {
                ID = jver.id,
                JarID = jver.jar ?? jver.id,
                Size = jver.downloads?.client.size ?? 0,
                SHA1 = jver.downloads?.client.sha1,
                Url = jver.downloads?.client.url.Substring(28),
                InheritsFrom = jver.inheritsFrom,
                MinecarftArguments = jver.minecraftArguments,
                MainClass = jver.mainClass,
                Libraries = new List<Library>(),
                Type = GetType(jver),
            };

            // For 1.13+ versions
            if (jver.arguments != null)
            {
                version.MinecarftArguments = jver.arguments.game
                    .Where(element => element.ValueKind == JsonValueKind.String)
                    .Select(element => element.GetString())
                    .Aggregate((current, next) => current + ' ' + next);
            }

            foreach (var lib in jver.libraries)
            {
                if (lib.name.StartsWith("tv.twitch"))
                {
                    // Totally unnecessary libs, game can run without them
                    // Ironically, these libs only appear in 1.7.10 - 1.8.9, not found in latter versions' json :D
                    // Also can cause troubles latter (and I don't wanna deal with that particular scenario)
                    // Might as well just ignore them! (Yes, I'm slacking off, LOL)
                    continue;
                }

                string[] names = lib.name.Split(':');
                if (names.Length != 3 || !IsAllowedLib(lib.rules))
                {
                    continue;
                }

                if (lib.natives == null)
                {
                    var libInfo = lib.downloads?.artifact;
                    version.Libraries.Add(new Library
                    {
                        Name = $"{names[1]}-{names[2]}.jar",
                        Type = (lib.downloads != null) ? LibraryType.Minecraft : LibraryType.Forge,
                        Path = libInfo?.path ??
                                string.Format("{0}/{1}/{2}/{1}-{2}.jar", names[0].Replace('.', '/'), names[1], names[2]),
                        Size = libInfo?.size ?? 0,
                        SHA1 = libInfo?.sha1,
                    });
                }
                else
                {
                    string suffix = lib.natives["windows"];
                    if (suffix.EndsWith("${arch}"))
                    {
                        suffix = suffix.Replace("${arch}", "64");
                    }

                    var nativeLibInfo = lib.downloads?.classifiers[suffix];

                    version.Libraries.Add(new Library
                    {
                        Name = $"{names[1]}-{names[2]}-{suffix}.jar",
                        Type = LibraryType.Native,
                        Path = nativeLibInfo?.path ??
                               string.Format("{0}/{1}/{2}/{1}-{2}-{3}.jar",
                               names[0].Replace('.', '/'), names[1], names[2], suffix),
                        Size = nativeLibInfo?.size ?? 0,
                        SHA1 = nativeLibInfo?.sha1,
                        Exclude = lib.extract?.exclude,
                    });
                }
            }

            if (version.InheritsFrom == null)
            {
                version.AssetsInfo = new AssetsInfo
                {
                    ID = jver.assetIndex.id,
                    IndexSize = jver.assetIndex.size,
                    IndexUrl = jver.assetIndex.url.Substring(32),
                    IndexSHA1 = jver.assetIndex.sha1,
                    TotalSize = jver.assetIndex.totalSize,
                };
            }

            return version;
        }

        private static bool IsValidVersion(JVersion jver)
        {
            return (!string.IsNullOrWhiteSpace(jver.id)
                && (!string.IsNullOrWhiteSpace(jver.minecraftArguments) || jver.arguments != null)
                && !string.IsNullOrWhiteSpace(jver.mainClass)
                && jver.libraries != null);
        }

        private static VersionType GetType(JVersion jver)
        {
            if (jver.inheritsFrom != null) return VersionType.Forge;
            if (jver.type == "release") return VersionType.Release;
            if (jver.type == "snapshot") return VersionType.Snapshot;
            return VersionType.Release;
        }

        private static bool IsAllowedLib(List<JRule> rules)
        {
            if (rules == null)
            {
                return true;
            }

            bool isAllowed = false;
            foreach (var rule in rules)
            {
                if (rule.os == null)
                {
                    isAllowed = (rule.action == "allow");
                    continue;
                }
                if (rule.os.name == "windows")
                {
                    isAllowed = (rule.action == "allow");
                }
            }
            return isAllowed;
        }
    }

    #endregion
}
