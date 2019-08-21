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

        public bool LoadAll()
        {
            _versions.Clear();

            if (!Directory.Exists(_gamePathService.VersionsDir))
            {
                Loaded?.Invoke(false);
                return false;
            }

            var availableVersions =
                Directory.EnumerateDirectories(_gamePathService.VersionsDir)
                         .Select(dir => $"{dir}/{Path.GetFileName(dir)}.json")
                         .Where(jsonPath => File.Exists(jsonPath))
                         .Select(jsonPath => File.ReadAllText(jsonPath, Encoding.UTF8))
                         .Select(json => Load(json))
                         .Where(version => version != null);

            var inheritVersions = new List<Version>(8);

            foreach (var verion in availableVersions)
            {
                _versions.Add(verion.ID, verion);
                if (verion.InheritsFrom != null)
                {
                    inheritVersions.Add(verion);
                }
            }

            foreach (var version in inheritVersions)
            {
                InheritParentProperties(version);
            }

            Loaded?.Invoke(_versions.Any());
            return _versions.Any();
        }

        public bool Any() => _versions.Any();

        public bool Has(string id) => (id != null) ? _versions.ContainsKey(id) : false;

        public IEnumerable<Version> GetAvailable() => _versions.Values;

        public Version GetByID(string id)
        {
            if (id != null && _versions.TryGetValue(id, out var version))
            {
                return version;
            }

            return null;
        }

        public Version AddNew(string json)
        {
            var newVersion = Load(json);

            if (newVersion != null)
            {
                string jsonPath = $"{_gamePathService.VersionsDir}/{newVersion.ID}/{newVersion.ID}.json";

                if (!File.Exists(jsonPath))
                {
                    // Make sure directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
                    File.WriteAllText(jsonPath, json, Encoding.UTF8);
                }

                if (newVersion.InheritsFrom != null) InheritParentProperties(newVersion);
                _versions.Add(newVersion.ID, newVersion);
                Created?.Invoke(newVersion);
            }

            return newVersion;
        }

        public async Task DeleteFromDiskAsync(string id, bool isDeleteLibs)
        {
            if (_versions.TryGetValue(id, out var versionToDelete))
            {
                _versions.Remove(id);
                Deleted?.Invoke(versionToDelete);

                // Delete version directory
                await SystemUtil.SendDirToRecycleBin($"{_gamePathService.VersionsDir}/{id}");

                // Delete unused libraries
                if (isDeleteLibs)
                {
                    var libsToDelete = versionToDelete.Libraries as IEnumerable<Library>;

                    if (_versions.Any())
                    {
                        foreach (var version in _versions.Values)
                        {
                            libsToDelete = libsToDelete.Except(version.Libraries);
                        }
                    }

                    foreach (var lib in libsToDelete)
                    {
                        var libPath = $"{_gamePathService.LibrariesDir}/{lib.Path}";
                        await SystemUtil.SendFileToRecycleBin(libPath);
                        SystemUtil.DeleteEmptyDirs(Path.GetDirectoryName(libPath));
                    }
                }
            }
        }

        public async Task<IEnumerable<VersionDownload>> GetDownloadListAsync()
        {
            try
            {
                string json = await _client.GetStringAsync(_urlService.Base.VersionList);
                var versionList = JsonSerializer.Deserialize<JVersionList>(json);

                return versionList.versions.Select(download =>
                new VersionDownload
                {
                    ID = download.id,
                    Url = download.url.Substring(32),
                    ReleaseTime = download.releaseTime,
                    Type = download.type == "release" ? VersionType.Release : VersionType.Snapshot,
                });
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
            catch (OperationCanceledException)
            {
                // Timeout
                Debug.WriteLine("[ERROR] Get version download list timeout");
                return null;
            }
        }

        public async Task<string> GetJsonAsync(VersionDownload download)
        {
            try
            {
                return await _client.GetStringAsync(_urlService.Base.Json + download.Url);
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine(ex.ToString());
                return null;
            }
            catch (OperationCanceledException)
            {
                // Timeout
                Debug.WriteLine("[ERROR] Get version download list timeout");
                return null;
            }
        }

        public bool CheckIntegrity(Version version)
        {
            var jarPath = $"{_gamePathService.VersionsDir}/{version.JarID}/{version.JarID}.jar";
            return File.Exists(jarPath) && (CryptUtil.GetFileSHA1(jarPath) == version.SHA1);
        }

        public IEnumerable<DownloadItem> GetDownload(Version version)
        {
            var item = new DownloadItem
            {
                Name = version.JarID + "jar",
                Path = $"{_gamePathService.VersionsDir}/{version.JarID}/{version.JarID}.jar",
                Url = _urlService.Base.Version + version.Url,
                Size = version.Size,
                IsCompleted = false,
                DownloadedBytes = 0,
            };

            return new List<DownloadItem>(1) { item };
        }

        #endregion

        #region Private Methods

        private static Version Load(string json)
        {
            JVersion jver;
            try
            {
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
                MainClass = jver.mainClass,
                Libraries = new List<Library>(),
                Type = (jver.type == "release") ? VersionType.Release : VersionType.Snapshot,
            };

            string[] args;

            // For 1.13+ versions
            if (jver.arguments != null)
            {
                args = jver.arguments.game
                    .Where(element => element.ValueKind == JsonValueKind.String)
                    .Select(element => element.GetString())
                    .ToArray();
            }
            else
            {
                args = jver.minecraftArguments.Split(' ');
            }

            version.MinecarftArgsDict = Enumerable.Range(0, args.Length / 2)
                                                  .ToDictionary(i => args[i * 2], i => args[i * 2 + 1]);

            if (args.Any(arg => arg.Contains("fml")))
            {
                // Invalid forge version
                if (version.InheritsFrom == null) return null;
                var idNums = version.InheritsFrom.Split('.');
                version.Type = (int.Parse(idNums[1]) >= 13) ? VersionType.NewForge : VersionType.Forge;
            }

            foreach (var jlib in jver.libraries)
            {
                if (jlib.name.StartsWith("tv.twitch"))
                {
                    // Totally unnecessary libs, game can run without them
                    // Ironically, these libs only appear in 1.7.10 - 1.8.9, not found in latter versions' json :D
                    // Also can cause troubles latter (and I don't wanna deal with that particular scenario)
                    // Might as well just ignore them! (Yes, I'm slacking off, LOL)
                    continue;
                }

                string[] names = jlib.name.Split(':');
                if (names.Length != 3 || !IsLibAllowed(jlib.rules))
                {
                    continue;
                }

                if (jlib.natives == null)
                {
                    var libInfo = jlib.downloads?.artifact;
                    var lib = new Library
                    {
                        Name = $"{names[1]}-{names[2]}",
                        Path = libInfo?.path ??
                               string.Format("{0}/{1}/{2}/{1}-{2}.jar", names[0].Replace('.', '/'), names[1], names[2]),
                        Size = libInfo?.size ?? 0,
                        SHA1 = libInfo?.sha1,
                    };

                    if (names[0] == "net.minecraftforge" && names[1] == "forge")
                    {
                        lib.Type = LibraryType.Forge;
                        lib.Url = $"{names[2]}/forge-{names[2]}-universal.jar";
                    }
                    else if (jlib.downloads != null)
                    {
                        lib.Type = (jlib.downloads.artifact.url.StartsWith("https://files.minecraftforge.net/maven/"))
                                 ? LibraryType.Maven : LibraryType.Minecraft;
                    }
                    else
                    {
                        lib.Type = (jlib.url == null) ? LibraryType.Minecraft : LibraryType.Maven;
                    }

                    if (lib.Type == LibraryType.Minecraft) lib.Url = jlib.downloads?.artifact.url.Substring(32);
                    if (lib.Type == LibraryType.Maven) lib.Url = jlib.downloads?.artifact.url.Substring(39);

                    version.Libraries.Add(lib);
                }
                else
                {
                    string suffix = jlib.natives["windows"];
                    if (suffix.EndsWith("${arch}"))
                    {
                        suffix = suffix.Replace("${arch}", "64");
                    }

                    var nativeLibInfo = jlib.downloads?.classifiers[suffix];

                    version.Libraries.Add(new Library
                    {
                        Name = $"{names[1]}-{names[2]}-{suffix}",
                        Type = LibraryType.Native,
                        Path = nativeLibInfo?.path ??
                               string.Format("{0}/{1}/{2}/{1}-{2}-{3}.jar",
                               names[0].Replace('.', '/'), names[1], names[2], suffix),
                        Size = nativeLibInfo?.size ?? 0,
                        SHA1 = nativeLibInfo?.sha1,
                        Exclude = jlib.extract?.exclude,
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

        private static bool IsLibAllowed(List<JRule> rules)
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

        private void InheritParentProperties(Version version)
        {
            if (_versions.TryGetValue(version.InheritsFrom, out var parent))
            {
                version.JarID = parent.JarID;

                foreach (var arg in parent.MinecarftArgsDict)
                {
                    if (!version.MinecarftArgsDict.ContainsKey(arg.Key))
                    {
                        version.MinecarftArgsDict.Add(arg.Key, arg.Value);
                    }
                }

                version.Libraries = parent.Libraries.Union(version.Libraries).ToList();
                version.AssetsInfo = parent.AssetsInfo;
                version.Size = parent.Size;
                version.SHA1 = parent.SHA1;
                version.Url = parent.Url;
            }
        }
    }

    #endregion
}
