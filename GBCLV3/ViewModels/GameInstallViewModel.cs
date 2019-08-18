using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GBCLV3.Models;
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

        //IoC
        private readonly VersionService _versionService;
        private readonly LibraryService _libraryService;
        private readonly AssetService _assetService;

        private readonly IWindowManager _windowManager;
        private readonly DownloadViewModel _downloadVM;

        #endregion

        #region Constructor

        [Inject]
        public GameInstallViewModel(
            VersionService versionService,
            LibraryService libraryService,
            AssetService assetService,

            IWindowManager windowManager,
            DownloadViewModel downloadVM)
        {
            _versionService = versionService;
            _libraryService = libraryService;
            _assetService = assetService;

            VersionDownloads = new BindableCollection<VersionDownload>();

            _windowManager = windowManager;
            _downloadVM = downloadVM;

            _isVersionListLoaded = false;
        }

        #endregion

        #region Bindings

        public VersionInstallStatus Status { get; private set; }

        public bool IsLoading => 
            Status == VersionInstallStatus.ListLoading ||
            Status == VersionInstallStatus.JsonFetching ||
            Status == VersionInstallStatus.DependenciesDownloading;

        public bool CanInstall => Status == VersionInstallStatus.ListLoaded;

        public BindableCollection<VersionDownload> VersionDownloads { get; set; }

        public bool IsDownloadAssets { get; set; }

        public async void InstallSelectedVersion(VersionDownload download)
        {
            if (_versionService.HasVersion(download.ID))
            {
                _windowManager.ShowMessageBox("${VersionAlreadyExists}", "${VersionInstallFailed}",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Status = VersionInstallStatus.JsonFetching;
            var json = await _versionService.GetJsonAsync(download);

            if (json == null)
            {
                _windowManager.ShowMessageBox("${VersionJsonFetchFailed}", "${VersionInstallFailed}",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                Status = VersionInstallStatus.ListLoaded;
                return;
            }

            var version = _versionService.AddNew(json);

            // Download essential dependencies
            Status = VersionInstallStatus.DependenciesDownloading;
            var downloads = _versionService.GetDownload(version);

            var missingLibs = _libraryService.CheckIntegrity(version.Libraries);
            downloads = downloads.Concat(_libraryService.GetDownloads(missingLibs));

            if (IsDownloadAssets)
            {
                if (await _assetService.DownloadIndexJsonAsync(version.AssetsInfo))
                {
                    _assetService.LoadAllObjects(version.AssetsInfo);
                }

                var missingAssets = await _assetService.CheckIntegrityAsync(version.AssetsInfo);
                downloads = downloads.Concat(_assetService.GetDownloads(missingAssets));
            }

            if (!await StartDownloadAsync(DownloadType.InstallNewVersion, downloads))
            {
                _windowManager.ShowMessageBox("${TryCompleteDependenciesOnLaunch}", "${VersionDependenciesIncomplete}",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            _windowManager.ShowMessageBox("${VersionInstallSuccessful} " + download.ID);
            Status = VersionInstallStatus.ListLoaded;
        }

        public void GoBack() => this.RequestClose();


        #endregion

        #region Private Methods

        private async Task<bool> StartDownloadAsync(DownloadType type, IEnumerable<DownloadItem> items)
        {
            using (var downloadService = new DownloadService(items))
            {
                _downloadVM.NewDownload(type, downloadService);
                this.ActivateItem(_downloadVM);

                return await downloadService.StartAsync();
            }
        }

        protected override async void OnActivate()
        {
            if (!_isVersionListLoaded)
            {
                Status = VersionInstallStatus.ListLoading;
                var (downloads, latestVersion) = await _versionService.GetDownloadListAsync();

                if (downloads != null)
                {
                    VersionDownloads.Clear();
                    VersionDownloads.AddRange(downloads);

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
