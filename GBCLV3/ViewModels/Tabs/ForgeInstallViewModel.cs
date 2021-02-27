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
        private readonly DownloadService _downloadService;

        private readonly DownloadStatusViewModel _downloadStatusVM;

        private readonly IWindowManager _windowManager;

        #endregion

        #region Constructor

        [Inject]
        public ForgeInstallViewModel(
            ForgeInstallService forgeInstallService,
            VersionService versionService,
            LibraryService libraryService,
            DownloadService downloadService,

            IWindowManager windowManager,
            DownloadStatusViewModel downloadStatusVM
            )
        {
            _forgeInstallService = forgeInstallService;
            _versionService = versionService;
            _libraryService = libraryService;
            _downloadService = downloadService;

            _windowManager = windowManager;
            _downloadStatusVM = downloadStatusVM;

            _forgeInstallService.InstallProgressChanged += progress => InstallProgress = progress;

            Forges = new BindableCollection<Forge>();
        }

        #endregion

        #region Bindings

        public Version GameVersion { get; set; }

        public string InstallProgress { get; private set; }

        public ForgeInstallStatus Status { get; private set; }

        public bool IsLoading => Status != ForgeInstallStatus.ListLoaded;

        public bool IsInstalling => Status == ForgeInstallStatus.Installing;

        public bool CanInstall => Status == ForgeInstallStatus.ListLoaded;

        public BindableCollection<Forge> Forges { get; }

        public async void InstallSelected(Forge forge)
        {
            bool hasLocal = _versionService.GetAll()
                .Where(v => v.Type == VersionType.Forge || v.Type == VersionType.NewForge)
                .Any(v => v.ID == forge.ID);

            if (hasLocal)
            {
                _windowManager.ShowMessageBox("${VersionAlreadyExists}", "${ForgeInstallFailed}",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            bool isInstallerNeeded = _forgeInstallService.IsInstallerNeeded(forge);

            Status = ForgeInstallStatus.DownloadingInstaller;
            var download = _forgeInstallService.GetDownload(forge, isInstallerNeeded);

            if (!await StartDownloadAsync(DownloadType.InstallForge, download))
            {
                _windowManager.ShowMessageBox("${ForgeJarDownloadFailed}", "${ForgeInstallFailed}",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                Status = ForgeInstallStatus.ListLoaded;
                return;
            }

            Version version;

            // Use installer
            if (isInstallerNeeded)
            {
                Status = ForgeInstallStatus.DownloadingLibraries;

                var jlibs = _forgeInstallService.GetJLibraries(forge);
                var libs = _libraryService.Process(jlibs).Where(lib => lib.Type != LibraryType.ForgeMain);

                var missingLibs = await _libraryService.CheckIntegrityAsync(libs);
                if (missingLibs.Any())
                {
                    Status = ForgeInstallStatus.DownloadingLibraries;

                    var downloads = _libraryService.GetDownloads(missingLibs);
                    if (!await StartDownloadAsync(DownloadType.Libraries, downloads))
                    {
                        _windowManager.ShowMessageBox("${TryCompleteDependenciesOnLaunch}",
                            "${VersionDependenciesIncomplete}",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

                Status = ForgeInstallStatus.Installing;

                version = await _forgeInstallService.InstallAsync(forge);

                if (version == null)
                {
                    _windowManager.ShowMessageBox("${ForgeExtractFailed}", "${ForgeInstallFailed}",
                        MessageBoxButton.OK, MessageBoxImage.Error);

                    Status = ForgeInstallStatus.ListLoaded;
                    return;
                }
            }
            else // The old way (in fact has better experience)
            {
                version = _forgeInstallService.InstallOld(forge);

                if (version == null)
                {
                    _windowManager.ShowMessageBox("${ForgeExtractFailed}", "${ForgeInstallFailed}",
                        MessageBoxButton.OK, MessageBoxImage.Error);

                    Status = ForgeInstallStatus.ListLoaded;
                    return;
                }

                var damagedLibs = await _libraryService.CheckIntegrityAsync(version.Libraries);
                if (damagedLibs.Any())
                {
                    Status = ForgeInstallStatus.DownloadingLibraries;

                    var downloads = _libraryService.GetDownloads(damagedLibs);
                    if (!await StartDownloadAsync(DownloadType.Libraries, downloads))
                    {
                        _windowManager.ShowMessageBox("${TryCompleteDependenciesOnLaunch}",
                            "${VersionDependenciesIncomplete}",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }

            _windowManager.ShowMessageBox("${ForgeInstallSuccessful} " + version.ID, "${InstallSuccessful}");

            Status = ForgeInstallStatus.ListLoaded;
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
            if (Status != ForgeInstallStatus.DownloadingInstaller &&
                Status != ForgeInstallStatus.Installing &&
                Status != ForgeInstallStatus.DownloadingLibraries)
            {
                Status = ForgeInstallStatus.ListLoading;
                Forges.Clear();

                var forges = await _forgeInstallService.GetDownloadListAsync(GameVersion.JarID);

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

                Forges.AddRange(forges);

                if (!Forges.Any())
                {
                    _windowManager.ShowMessageBox("${NoAvailableForge}", "${ForgeInstallFailed}",
                        MessageBoxButton.OK, MessageBoxImage.Error);

                    this.RequestClose();
                    return;
                }

                Status = ForgeInstallStatus.ListLoaded;
            }
        }

        #endregion
    }
}