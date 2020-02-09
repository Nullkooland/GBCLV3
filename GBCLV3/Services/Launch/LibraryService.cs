using GBCLV3.Models.Download;
using GBCLV3.Models.Launch;
using GBCLV3.Services.Download;
using StyletIoC;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace GBCLV3.Services.Launch
{
    class LibraryService
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

        public IEnumerable<Library> CheckIntegrity(IEnumerable<Library> libraries)
        {
            return libraries.Where(lib =>
            {
                string path = $"{_gamePathService.LibrariesDir}/{lib.Path}";
                return !File.Exists(path) || lib.SHA1 != null && lib.SHA1 != Utils.CryptUtil.GetFileSHA1(path);
            }).ToList();
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
            }).ToList();
        }

        #endregion
    }
}
