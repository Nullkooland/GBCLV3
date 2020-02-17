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
    public class FabricInstallViewModel : Conductor<DownloadStatusViewModel>.Collection.OneActive
    {
        #region Private Fields

        //// IoC
        private readonly FabricInstallService _fabricInstallService;
        private readonly VersionService _versionService;
        private readonly LibraryService _libraryService;

        private readonly IWindowManager _windowManager;

        private readonly DownloadStatusViewModel _downloadStatusVM;

        #endregion

        #region Constructor

        [Inject]
        public FabricInstallViewModel(
            FabricInstallService fabricInstallService,
            VersionService versionService,
            LibraryService libraryService,

            IWindowManager windowManager,
            DownloadStatusViewModel downloadVM)
        {
            _fabricInstallService = fabricInstallService;
            _versionService = versionService;
            _libraryService = libraryService;

            Fabrics = new BindableCollection<Fabric>();

            _windowManager = windowManager;
            _downloadStatusVM = downloadVM;
        }

        #endregion

        #region Bindings

        public string GameVersion { get; set; }

        public FabricInstallStatus Status { get; private set; }

        public bool IsLoading => Status != FabricInstallStatus.ListLoaded;

        public bool CanInstall => Status == FabricInstallStatus.ListLoaded;

        public BindableCollection<Fabric> Fabrics { get; private set; }

        public async void InstallSelected(Fabric fabric)
        {
            bool hasLocal = _versionService.GetAll()
                                           .Where(v => v.Type == VersionType.Fabric && v.JarID == GameVersion)
                                           .Any(v => v.ID.EndsWith(fabric.Loader.Version));

            if (hasLocal)
            {
                _windowManager.ShowMessageBox("${VersionAlreadyExists}", "${FabricInstallFailed}",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                Status = FabricInstallStatus.ListLoaded;
                return;
            }

            var version = _fabricInstallService.Install(fabric);

            var missingLibs = _libraryService.CheckIntegrity(version.Libraries);
            var downloads = _libraryService.GetDownloads(missingLibs);

            if (!await StartDownloadAsync(DownloadType.InstallFabric, downloads))
            {
                _windowManager.ShowMessageBox("${TryCompleteDependenciesOnLaunch}", "${VersionDependenciesIncomplete}",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            _windowManager.ShowMessageBox("${FabricInstallSuccessful} " + version.ID, "${InstallSuccessful}");
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
            if (Status != FabricInstallStatus.DownloadingLibraries)
            {
                Status = FabricInstallStatus.ListLoading;
                Fabrics.Clear();

                var fabrics = await _fabricInstallService.GetDownloadListAsync(GameVersion);

                // まっそんなのもう関係ないですけどね！
                if (!this.IsActive) return;

                if (fabrics == null)
                {
                    _windowManager.ShowMessageBox("${FabricListLoadFailed}", "${FabricInstallFailed}",
                        MessageBoxButton.OK, MessageBoxImage.Error);

                    this.RequestClose();
                    return;
                }

                if (!fabrics.Any())
                {
                    _windowManager.ShowMessageBox("${NoAvailableFabric}", "${FabricInstallFailed}",
                        MessageBoxButton.OK, MessageBoxImage.Error);

                    this.RequestClose();
                    return;
                }

                Fabrics.AddRange(fabrics);
                Status = FabricInstallStatus.ListLoaded;
            }
        }

        #endregion
    }
}
