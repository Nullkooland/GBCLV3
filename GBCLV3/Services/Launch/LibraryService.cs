using GBCLV3.Models.Download;
using GBCLV3.Models.Launch;
using GBCLV3.Services.Download;
using StyletIoC;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using GBCLV3.Utils;

namespace GBCLV3.Services.Launch
{
    public class LibraryService
    {
        #region Private Fields

        // IoC
        private readonly GamePathService _gamePathService;
        private readonly DownloadUrlService _urlService;

        #endregion

        #region Constructor

        [Inject]
        public LibraryService(GamePathService gamePathService, DownloadUrlService urlService)
        {
            _gamePathService = gamePathService;
            _urlService = urlService;
        }

        #endregion

        #region Public Methods

        public IEnumerable<Library> Process(IEnumerable<JLibrary> jlibs)
        {
            foreach (var jlib in jlibs)
            {
                // Totally unnecessary libs, game can run without them
                // Ironically, these libs only appear in 1.7.10 - 1.8.9, not found in latter versions' json :D
                // Also can cause troubles latter (and I don't wanna deal with that particular scenario)
                // Might as well just ignore them! (Yes, I'm slacking off, LOL)
                if (jlib.name.StartsWith("tv.twitch")) continue;

                var names = jlib.name.Split(':');
                if (names.Length != 3 || !IsLibAllowed(jlib.rules)) continue;

                if (jlib.natives == null)
                {
                    var libInfo = jlib.downloads?.artifact;
                    var lib = new Library
                    {
                        Name = jlib.name,
                        Path = libInfo?.path ??
                               string.Format("{0}/{1}/{2}/{1}-{2}.jar", names[0].Replace('.', '/'), names[1], names[2]),
                        Size = libInfo?.size ?? 0,
                        SHA1 = libInfo?.sha1
                    };

                    if (names[0] == "net.minecraftforge" && names[1] == "forge")
                    {
                        lib.Type = LibraryType.ForgeMain;
                        lib.Url = $"{names[2]}/forge-{names[2]}-universal.jar";
                    }
                    else if (jlib.downloads?.artifact.url.StartsWith("https://files.minecraftforge.net/maven/") ??
                             jlib.url == "http://files.minecraftforge.net/maven/")
                    {
                        lib.Type = LibraryType.Forge;
                        lib.Url = jlib.downloads?.artifact.url[39..];
                    }
                    else if (jlib.url == "https://maven.fabricmc.net/")
                    {
                        lib.Type = LibraryType.Fabric;
                        lib.Url = jlib.url + lib.Path;
                    }
                    else
                    {
                        lib.Type = LibraryType.Minecraft;
                        lib.Url = jlib.downloads?.artifact.url[32..];
                    }

                    yield return lib;
                }
                else
                {
                    string suffix = jlib.natives["windows"];
                    if (suffix.EndsWith("${arch}")) suffix = suffix.Replace("${arch}", "64");

                    var nativeLibInfo = jlib.downloads?.classifiers[suffix];

                    yield return new Library
                    {
                        Name = $"{names[1]}-{names[2]}-{suffix}",
                        Type = LibraryType.Native,
                        Path = nativeLibInfo?.path ??
                               string.Format("{0}/{1}/{2}/{1}-{2}-{3}.jar",
                                   names[0].Replace('.', '/'), names[1], names[2], suffix),
                        Size = nativeLibInfo?.size ?? 0,
                        SHA1 = nativeLibInfo?.sha1,
                        Exclude = jlib.extract?.exclude
                    };
                }
            }
        }

        public void ExtractNatives(IEnumerable<Library> nativeLibraries)
        {
            // Make sure directory exists
            Directory.CreateDirectory(_gamePathService.NativesDir);

            foreach (var native in nativeLibraries)
            {
                using var archive = ZipFile.OpenRead($"{_gamePathService.LibrariesDir}/{native.Path}");
                // You know what, the "Exclude" property is a joke...
                foreach (var entry in archive.Entries.Where(e => !e.FullName.StartsWith("META-INF")))
                {
                    entry.ExtractToFile($"{_gamePathService.NativesDir}/{entry.FullName}", true);
                }
            }
        }

        public Task<Library[]> CheckIntegrityAsync(IEnumerable<Library> libraries)
        {
            var query = libraries.Where(lib =>
            {
                string libPath = $"{_gamePathService.LibrariesDir}/{lib.Path}";
                return !File.Exists(libPath) || (lib.SHA1 != null && !CryptUtil.ValidateFileSHA1(libPath, lib.SHA1));
            });

            return Task.FromResult(query.ToArray());
        }

        public IEnumerable<DownloadItem> GetDownloads(IEnumerable<Library> libraries)
        {
            string GetUrl(Library lib)
            {
                switch (lib.Type)
                {
                    case LibraryType.ForgeMain:
                        return _urlService.Base.Forge + lib.Url;
                    case LibraryType.Minecraft:
                    case LibraryType.Native:
                        return _urlService.Base.Library + (lib.Url ?? lib.Path);
                    case LibraryType.Forge:
                        return _urlService.Base.ForgeMaven + (lib.Url ?? lib.Path);
                    case LibraryType.Fabric:
                        return _urlService.Base.FabricMaven + lib.Path;
                    default: return null;
                }
            }

            return libraries.Select(lib =>
                new DownloadItem
                {
                    Name = lib.Name,
                    Size = lib.Size,
                    Path = $"{_gamePathService.LibrariesDir}/{lib.Path}",
                    Url = GetUrl(lib),
                    IsCompleted = false,
                    DownloadedBytes = 0,
                });
        }

        #endregion

        #region Private Methods

        private static bool IsLibAllowed(List<JRule> rules)
        {
            if (rules == null) return true;

            var isAllowed = false;
            foreach (var rule in rules)
            {
                if (rule.os == null)
                {
                    isAllowed = rule.action == "allow";
                    continue;
                }

                if (rule.os.name == "windows") isAllowed = rule.action == "allow";
            }

            return isAllowed;
        }

        #endregion
    }
}