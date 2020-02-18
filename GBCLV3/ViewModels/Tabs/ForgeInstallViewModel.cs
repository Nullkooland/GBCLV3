using GBCLV3.Models.Download;
using GBCLV3.Models.Installation;
using GBCLV3.Models.Launch;
using GBCLV3.Services.Download;
using GBCLV3.Services.Installation;
using GBCLV3.Services.Launch;
using Stylet;
using StyletIoC;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GBCLV3.ViewModels.Tabs
{
    public class ForgeInstallViewModel : Conductor<DownloadStatusViewModel>.Collection.OneActive
    {
        #region Private Fields

        //IoC
        private readonly ForgeInstallService _forgeInstallService;
        private readonly VersionService _versionService;
        private readonly LibraryService _libraryService;

        private readonly DownloadStatusViewModel _downloadStatusVM;

        private readonly IWindowManager _windowManager;

        #endregion

        #region Constructor

        [Inject]
        public ForgeInstallViewModel(
            ForgeInstallService forgeInstallService,
            VersionService versionService,
            LibraryService libraryService,

            DownloadStatusViewModel downloadStatusVM,
            IWindowManager windowManager)
        {
            _forgeInstallService = forgeInstallService;
            _versionService = versionService;
            _libraryService = libraryService;

            Forges = new BindableCollection<Forge>();

            _windowManager = windowManager;
            _downloadStatusVM = downloadStatusVM;
        }

        #endregion

        #region Bindings

        public string GameVersion { get; set; }

        public ForgeInstallStatus Status { get; private set; }

        public bool IsLoading => Status != ForgeInstallStatus.ListLoaded;

        public bool CanInstall => Status == ForgeInstallStatus.ListLoaded;

        public BindableCollection<Forge> Forges { get; }

        public async void InstallSelected(Forge forge)
        {
            bool hasLocal = _versionService.GetAll()
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
                version = await _forgeInstallService.ManualInstallAsync(forge);
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

            _windowManager.ShowMessageBox("${ForgeInstallSuccessful} " + version.ID, "${InstallSuccessful}");
            this.RequestClose();
        }

        public void GoBack() => this.RequestClose();

        #endregion

        #region Private Methods

        private async ValueTask<bool> StartDownloadAsync(DownloadType type, IEnumerable<DownloadItem> items)
        {
            using var downloadService = new DownloadService(items);
            _downloadStatusVM.Setup(type, downloadService);
            this.ActivateItem(_downloadStatusVM);

            return await downloadService.StartAsync();
        }

        protected override async void OnActivate()
        {
            if (Status != ForgeInstallStatus.DownloadingInstaller &&
                Status != ForgeInstallStatus.ManualInstalling &&
                Status != ForgeInstallStatus.DownloadingLibraries)
            {
                Status = ForgeInstallStatus.ListLoading;
                Forges.Clear();

                var forges = await _forgeInstallService.GetDownloadListAsync(GameVersion);

                // Since the user has clicked the return button
                // Nobody cares about the fetching result!
                // まっそんなのもう関係ないですけどね！
                if (!this.IsActive) return;

                if (forges == null)
                {
                    _windowManager.ShowMessageBox("${ForgeListLoadFailed}", "${ForgeInstallFailed}",
                        MessageBoxButton.OK, MessageBoxImage.Error);

                    this.RequestClose();
                    return;
                }

                if (!forges.Any())
                {
                    _windowManager.ShowMessageBox("${NoAvailableForge}", "${ForgeInstallFailed}",
                        MessageBoxButton.OK, MessageBoxImage.Error);

                    this.RequestClose();
                    return;
                }

                Forges.AddRange(forges);
                Status = ForgeInstallStatus.ListLoaded;
            }
        }

        #endregion
    }
}
