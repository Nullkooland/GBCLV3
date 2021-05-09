using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using GBCLV3.Models;
using GBCLV3.Models.Download;
using GBCLV3.Models.Installation;
using GBCLV3.Models.Launch;
using GBCLV3.Services;
using GBCLV3.Services.Download;
using GBCLV3.Services.Launch;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels.Tabs
{
    public class GameInstallViewModel : Conductor<DownloadStatusViewModel>.Collection.OneActive
    {
        #region Private Fields

        private bool _isVersionListLoaded;
        private bool _isReleaseOnly;
        private readonly BindableCollection<VersionDownload> _versionDownloads;

        //IoC
        private readonly Config _config;
        private readonly GamePathService _gamePathService;
        private readonly VersionService _versionService;
        private readonly LibraryService _libraryService;
        private readonly AssetService _assetService;
        private readonly DownloadService _downloadService;

        private readonly DownloadStatusViewModel _downloadStatusVM;

        private readonly IWindowManager _windowManager;

        #endregion

        #region Constructor

        [Inject]
        public GameInstallViewModel(
            ConfigService configService,
            GamePathService gamePathService,
            VersionService versionService,
            LibraryService libraryService,
            AssetService assetService,
            DownloadService downloadService,

            IWindowManager windowManager,
            DownloadStatusViewModel downloadVM)
        {
            _config = configService.Entries;
            _gamePathService = gamePathService;
            _versionService = versionService;
            _libraryService = libraryService;
            _assetService = assetService;
            _downloadService = downloadService;

            _versionDownloads = new BindableCollection<VersionDownload>();
            VersionDownloads = CollectionViewSource.GetDefaultView(_versionDownloads);
            VersionDownloads.Filter = obj =>
            {
                if (_isReleaseOnly)
                {
                    return (obj as VersionDownload).Type == VersionType.Release;
                }

                return true;
            };

            _windowManager = windowManager;
            _downloadStatusVM = downloadVM;

            _isVersionListLoaded = false;
            _isReleaseOnly = true;
        }

        #endregion

        #region Bindings

        public VersionInstallStatus Status { get; private set; }

        public bool IsLoading =>
            Status == VersionInstallStatus.ListLoading ||
            Status == VersionInstallStatus.FetchingJson ||
            Status == VersionInstallStatus.DownloadingDependencies;

        public bool CanInstall => Status == VersionInstallStatus.ListLoaded;

        public ICollectionView VersionDownloads { get; set; }

        public bool IsReleaseOnly
        {
            get => _isReleaseOnly;
            set
            {
                _isReleaseOnly = value;
                VersionDownloads.Refresh();
            }
        }

        public bool IsDownloadAssets
        {
            get => _config.DownloadAssetsOnInstall;
            set => _config.DownloadAssetsOnInstall = value;
        }

        public async void InstallSelectedVersion(VersionDownload download)
        {
            if (_versionService.Has(download.ID))
            {
                _windowManager.ShowMessageBox("${VersionAlreadyExists}", "${VersionInstallFailed}",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            Status = VersionInstallStatus.FetchingJson;
            byte[] json = await _versionService.GetJsonAsync(download);

            if (json == null)
            {
                _windowManager.ShowMessageBox("${VersionJsonFetchFailed}", "${VersionInstallFailed}",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                Status = VersionInstallStatus.ListLoaded;
                return;
            }

            string jsonPath = $"{_gamePathService.VersionsDir}/{download.ID}/{download.ID}.json";

            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath));
            File.WriteAllBytes(jsonPath, json);

            var version = _versionService.AddNew(jsonPath);

            // Download essential dependencies
            Status = VersionInstallStatus.DownloadingDependencies;
            var downloads = _versionService.GetDownload(version);

            var missingLibs = await _libraryService.CheckIntegrityAsync(version.Libraries);
            downloads = downloads.Concat(_libraryService.GetDownloads(missingLibs));

            // Download assets index json
            await _assetService.DownloadIndexJsonAsync(version.AssetsInfo);

            // Download assets objects on user's discretion
            if (IsDownloadAssets && _assetService.LoadAllObjects(version.AssetsInfo))
            {
                var missingAssets = await _assetService.CheckIntegrityAsync(version.AssetsInfo);
                downloads = downloads.Concat(_assetService.GetDownloads(missingAssets));
            }

            if (!await StartDownloadAsync(DownloadType.InstallNewVersion, downloads))
            {
                _windowManager.ShowMessageBox("${TryCompleteDependenciesOnLaunch}", "${VersionDependenciesIncomplete}",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            _windowManager.ShowMessageBox("${VersionInstallSuccessful} " + download.ID, "${InstallSuccessful}");
            Status = VersionInstallStatus.ListLoaded;

            // Normally the user won't install multiple game versions at once...
            this.RequestClose();
        }

        public void GoBack() => this.RequestClose();


        #endregion

        #region Private Methods

        private async ValueTask<bool> StartDownloadAsync(DownloadType type, IEnumerable<DownloadItem> items)
        {
            _downloadService.Setup(items);
            _downloadStatusVM.Setup(type, _downloadService);
            this.ActivateItem(_downloadStatusVM);

            return await _downloadService.StartAsync();
        }

        protected override async void OnActivate()
        {
            if (!_isVersionListLoaded)
            {
                Status = VersionInstallStatus.ListLoading;
                var downloads = await _versionService.GetDownloadListAsync();

                if (downloads != null)
                {
                    _versionDownloads.Clear();
                    _versionDownloads.AddRange(downloads);

                    Status = VersionInstallStatus.ListLoaded;
                    _isVersionListLoaded = true;
                }
                else
                {
                    Status = VersionInstallStatus.ListLoadFailed;
                }
            }
        }

        #endregion
    }
}
