using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using GBCLV3.Models;
using GBCLV3.Models.Installation;
using GBCLV3.Models.Launcher;
using GBCLV3.Services;
using GBCLV3.Services.Installation;
using GBCLV3.Services.Launcher;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels
{
    class ForgeInstallViewModel : Conductor<DownloadViewModel>.Collection.OneActive
    {
        #region Private Members

        //IoC
        private readonly ForgeInstallService _forgeInstallService;
        private readonly VersionService _versionService;
        private readonly LibraryService _libraryService;

        private readonly DownloadViewModel _downloadVM;

        private readonly IWindowManager _windowManager;

        #endregion

        #region Constructor

        [Inject]
        public ForgeInstallViewModel(
            ForgeInstallService forgeInstallService,
            VersionService versionService,
            LibraryService libraryService,

            DownloadViewModel downloadVM,
            IWindowManager windowManager)
        {
            _forgeInstallService = forgeInstallService;
            _versionService = versionService;
            _libraryService = libraryService;

            ForgeDownloads = new BindableCollection<Forge>();

            _windowManager = windowManager;
            _downloadVM = downloadVM;
        }

        #endregion

        #region Bindings

        public string GameVersion { get; set; }

        public ForgeInstallStatus Status { get; private set; }

        public bool IsLoading => Status != ForgeInstallStatus.ListLoaded;

        public bool CanInstall => Status == ForgeInstallStatus.ListLoaded;

        public BindableCollection<Forge> ForgeDownloads { get; private set; }

        public async void InstallSelectedForge(Forge forge)
        {
            bool hasLocal = _versionService.GetAvailable()
                                           .Where(v => v.Type == VersionType.Forge || v.Type == VersionType.NewForge)
                                           .Any(v => v.ID.EndsWith(forge.Version));

            if (hasLocal)
            {
                _windowManager.ShowMessageBox("${VersionAlreadyExists}", "${ForgeInstallFailed}",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                Status = ForgeInstallStatus.ListLoaded;
                return;
            }

            Status = ForgeInstallStatus.DownloadingInstaller;
            var download = _forgeInstallService.GetDownload(forge);

            if (!await StartDownloadAsync(DownloadType.InstallForge, download))
            {
                _windowManager.ShowMessageBox("${ForgeJarDownloadFailed}", "${ForgeInstallFailed}",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                Status = ForgeInstallStatus.ListLoaded;
                return;
            }

            Version version = null;

            if (forge.IsAutoInstall)
            {
                version = _forgeInstallService.AutoInstall(forge);
            }
            else
            {
                Status = ForgeInstallStatus.ManualInstalling;
                version = await _forgeInstallService.ManualInstall(forge);
            }

            if (version == null)
            {
                _windowManager.ShowMessageBox("${ForgeExtractFailed}", "${ForgeInstallFailed}",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                Status = ForgeInstallStatus.ListLoaded;
                return;
            }

            Status = ForgeInstallStatus.DownloadingLibraries;

            var missingLibs = _libraryService.CheckIntegrity(version.Libraries);
            var downloads = _libraryService.GetDownloads(missingLibs);

            if (!await StartDownloadAsync(DownloadType.Libraries, downloads))
            {
                _windowManager.ShowMessageBox("${TryCompleteDependenciesOnLaunch}", "${VersionDependenciesIncomplete}",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            _windowManager.ShowMessageBox("${ForgeInstallSuccessful} " + version.ID);

            Status = ForgeInstallStatus.ListLoaded;
            this.RequestClose();
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
            if (Status != ForgeInstallStatus.DownloadingInstaller &&
                Status != ForgeInstallStatus.ManualInstalling &&
                Status != ForgeInstallStatus.DownloadingLibraries)
            {
                Status = ForgeInstallStatus.ListLoading;
                ForgeDownloads.Clear();

                var downloads = await _forgeInstallService.GetDownloadListAsync(GameVersion);

                // Since the user has clicked the return button
                // Nobody cares about the fetching result!
                // まっそんなのもう関係ないですけどね！
                if (!this.IsActive) return;

                if (downloads == null)
                {
                    _windowManager.ShowMessageBox("${ForgeListLoadFailed}", "${ForgeInstallFailed}",
                        MessageBoxButton.OK, MessageBoxImage.Error);

                    this.RequestClose();
                    return;
                }

                if (!downloads.Any())
                {
                    _windowManager.ShowMessageBox("${NoAvailableForge}", "${ForgeInstallFailed}",
                        MessageBoxButton.OK, MessageBoxImage.Error);

                    this.RequestClose();
                    return;
                }

                ForgeDownloads.AddRange(downloads);
                Status = ForgeInstallStatus.ListLoaded;
            }
        }

        #endregion
    }
}
