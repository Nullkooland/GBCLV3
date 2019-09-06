using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using GBCLV3.Models;
using GBCLV3.Models.Installation;
using GBCLV3.Models.Launcher;
using GBCLV3.Services;
using GBCLV3.Services.Launcher;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels
{
    class GameInstallViewModel : Conductor<DownloadViewModel>.Collection.OneActive
    {
        #region Private Members

        private bool _isVersionListLoaded;
        private bool _isReleaseOnly;
        private readonly BindableCollection<VersionDownload> _versionDownloads;

        //IoC
        private readonly Config _config;
        private readonly VersionService _versionService;
        private readonly LibraryService _libraryService;
        private readonly AssetService _assetService;

        private readonly DownloadViewModel _downloadVM;

        private readonly IWindowManager _windowManager;

        #endregion

        #region Constructor

        [Inject]
        public GameInstallViewModel(
            ConfigService configService,
            VersionService versionService,
            LibraryService libraryService,
            AssetService assetService,

            DownloadViewModel downloadVM,
            IWindowManager windowManager)
        {
            _config = configService.Entries;
            _versionService = versionService;
            _libraryService = libraryService;
            _assetService = assetService;

            _versionDownloads = new BindableCollection<VersionDownload>();
            VersionDownloads = CollectionViewSource.GetDefaultView(_versionDownloads);
            VersionDownloads.Filter = obj =>
            {
                if (_isReleaseOnly) return (obj as VersionDownload).Type == VersionType.Release;
                return true;
            };

            _windowManager = windowManager;
            _downloadVM = downloadVM;

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
            string json = await _versionService.GetJsonAsync(download);

            if (json == null)
            {
                _windowManager.ShowMessageBox("${VersionJsonFetchFailed}", "${VersionInstallFailed}",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                Status = VersionInstallStatus.ListLoaded;
                return;
            }

            var version = _versionService.AddNew(json);

            // Download essential dependencies
            Status = VersionInstallStatus.DownloadingDependencies;
            var downloads = _versionService.GetDownload(version);

            var missingLibs = _libraryService.CheckIntegrity(version.Libraries);
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
        }

        public void GoBack() => this.RequestClose();


        #endregion

        #region Private Methods

        private async Task<bool> StartDownloadAsync(DownloadType type, IEnumerable<DownloadItem> items)
        {
            using (var downloadService = new DownloadService(items))
            {
                _downloadVM.Setup(type, downloadService);
                this.ActivateItem(_downloadVM);

                return await downloadService.StartAsync();
            }
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
