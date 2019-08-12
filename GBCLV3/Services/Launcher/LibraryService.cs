using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using GBCLV3.Models;
using GBCLV3.Models.Launcher;
using StyletIoC;

namespace GBCLV3.Services.Launcher
{
    class LibraryService
    {
        #region Private Members

        // IoC
        private readonly GamePathService _gamePathService;
        private readonly UrlService _urlService;

        #endregion

        #region Constructor

        [Inject]
        public LibraryService(GamePathService gamePathService, UrlService urlService)
        {
            _gamePathService = gamePathService;
            _urlService = urlService;
        }

        #endregion

        #region Public Methods

        public void ExtractNatives(IEnumerable<Library> nativeLibraries)
        {
            if (!Directory.Exists(_gamePathService.NativeDir))
            {
                Directory.CreateDirectory(_gamePathService.NativeDir);
            }

            foreach (var native in nativeLibraries)
            {
                using (var archive = ZipFile.OpenRead($"{_gamePathService.LibDir}/{native.Path}"))
                {
                    // You know what, the "Exclude" property is a joke...
                    foreach (var entry in archive.Entries.Where(e => !e.FullName.StartsWith("META-INF")))
                    {
                        entry.ExtractToFile($"{_gamePathService.NativeDir}/{entry.FullName}", true);
                    }
                }
            }
        }

        public IEnumerable<Library> CheckIntegrity(IEnumerable<Library> libraries)
        {
            return libraries.Where(lib =>
            {
                string path = $"{_gamePathService.LibDir}/{lib.Path}";
                return !File.Exists(path) || (lib.SHA1 != null && lib.SHA1 != Utils.CryptUtil.GetFileSHA1(path));
            }).ToList();
        }

        public (DownloadType, IEnumerable<DownloadItem>) GetDownloadInfo(IEnumerable<Library> libraries)
        {
            var items = libraries.Select(lib => new DownloadItem
            {
                Name = lib.Name,
                Size = lib.Size,
                Path = $"{_gamePathService.LibDir}/{lib.Path}",
                Url = (lib.Type == LibraryType.Forge ? _urlService.Base.Maven : _urlService.Base.Library) + lib.Path,
                IsCompleted = false,
                DownloadedBytes = 0,
            }).ToList();

            return (DownloadType.Libraries, items);
        }

        #endregion
    }
}
