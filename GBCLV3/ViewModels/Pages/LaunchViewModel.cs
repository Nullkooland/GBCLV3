using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GBCLV3.Models;
using GBCLV3.Models.Launcher;
using GBCLV3.Services;
using GBCLV3.Services.Launcher;
using GBCLV3.Utils;
using GBCLV3.ViewModels.Windows;
using Stylet;
using Version = GBCLV3.Models.Launcher.Version;

namespace GBCLV3.ViewModels.Pages
{
    class LaunchViewModel : Conductor<IScreen>.Collection.OneActive
    {
        #region Private Members

        private readonly StringBuilder _logger;

        // IoC
        private readonly Config _config;
        private readonly VersionService _versionService;
        private readonly LibraryService _libraryService;
        private readonly AssetService _assetService;
        private readonly LaunchService _launchService;
        private readonly SkinService _skinService;

        private readonly LaunchStatusViewModel _statusVM;
        private readonly DownloadViewModel _downloadVM;
        private readonly ErrorReportViewModel _errorReportVM;

        private readonly IWindowManager _windowManager;
        private readonly IEventAggregator _eventAggregator;

        #endregion

        #region Constructor

        public LaunchViewModel(
            IWindowManager windowManager,
            IEventAggregator eventAggregator,

            ConfigService configService,
            ThemeService themeService,
            VersionService versionService,
            LibraryService libraryService,
            AssetService assetService,
            LaunchService launchService,
            SkinService skinService,

            GreetingViewModel greetingVM,
            LaunchStatusViewModel statusVM,
            DownloadViewModel downloadVM,
            ErrorReportViewModel errorReportVM)
        {
            _windowManager = windowManager;
            _eventAggregator = eventAggregator;

            _config = configService.Entries;

            _versionService = versionService;
            _libraryService = libraryService;
            _assetService = assetService;
            _launchService = launchService;
            _skinService = skinService;

            _launchService.ErrorReceived += errorMessage => _logger.Append(errorMessage);
            _launchService.Exited += OnGameExited;

            Versions = new BindableCollection<Version>();

            // OnVersionLoaded
            _versionService.Loaded += hasAny =>
            {
                Versions.Clear();
                Versions.AddRange(_versionService.GetAvailable());

                if (hasAny)
                {
                    if (!_versionService.Has(SelectedVersionID))
                    {
                        SelectedVersionID = Versions.FirstOrDefault().ID;
                    }

                    CanLaunch = true;
                }
                else
                {
                    CanLaunch = false;
                }
            };

            // OnVersionCreated
            _versionService.Created += version =>
            {
                Versions.Insert(0, version);
                SelectedVersionID = version.ID;
                CanLaunch = true;
            };

            // OnVersionDeleted
            _versionService.Deleted += version =>
            {
                Versions.Remove(version);

                if (SelectedVersionID == null)
                {
                    SelectedVersionID = Versions.FirstOrDefault()?.ID;
                }

                if (!Versions.Any()) CanLaunch = false;
            };

            _logger = new StringBuilder(4096);

            _statusVM = statusVM;
            _downloadVM = downloadVM;
            _errorReportVM = errorReportVM;

            _statusVM.Closed += (sender, e) => OnLaunchCompleted();

            ThemeService = themeService;
            GreetingVM = greetingVM;
        }

        #endregion

        #region Bindings

        public GreetingViewModel GreetingVM { get; private set; }

        public ThemeService ThemeService { get; private set; }

        public BindableCollection<Version> Versions { get; private set; }

        public string SelectedVersionID
        {
            get => _config.SelectedVersion;
            set => _config.SelectedVersion = value;
        }

        public bool CanLaunch { get; private set; }

        public async void Launch()
        {
            // Check JRE
            if (_config.JreDir == null)
            {
                _windowManager.ShowMessageBox("${JreNotFoundError}\n${PleaseInstallJre}", null,
                    MessageBoxButton.OK, MessageBoxImage.Error);

                _statusVM.Status = LaunchStatus.Failed;
                return;
            }

            CanLaunch = false;

            _statusVM.GameOutputLog = null;
            this.ActivateItem(_statusVM);

            _statusVM.Status = LaunchStatus.LoggingIn;

            var authResult =
                _config.OfflineMode ? AuthService.GetOfflineProfile(_config.Username) :
                _config.UseToken ? await AuthService.RefreshAsync(_config.ClientToken, _config.AccessToken) :
                                   await AuthService.LoginAsync(_config.Email, _config.Password);

            if (authResult.IsSuccessful)
            {
                _config.UseToken = true;
                _config.ClientToken = authResult.ClientToken;
                _config.AccessToken = authResult.AccessToken;

                _config.Username = authResult.Username;
                _config.UUID = authResult.UUID;
                _eventAggregator.Publish(new UsernameChangedEvent());
            }
            else
            {
                _statusVM.Status = LaunchStatus.Failed;

                if (authResult.ErrorType == AuthErrorType.InvalidToken)
                {
                    // Clear the invalid token, using email and password for authentication next time
                    _config.AccessToken = null;
                    _config.UseToken = false;
                }

                _windowManager.ShowMessageBox(authResult.ErrorMessage, "${AuthFailed}",
                    MessageBoxButton.OK, MessageBoxImage.Error);

                return;
            }

            _statusVM.Status = LaunchStatus.ProcessingDependencies;
            var launchVersion = _versionService.GetByID(SelectedVersionID);

            // Check main jar and fix possible damage
            if (!_versionService.CheckIntegrity(launchVersion))
            {
                var download = _versionService.GetDownload(launchVersion);
                if (!await StartDownloadAsync(DownloadType.MainJar, download))
                {
                    _statusVM.Status = LaunchStatus.Failed;
                    return;
                }
            }

            // Check dependent libraries and fix possible damage
            var damagedLibs = _libraryService.CheckIntegrity(launchVersion.Libraries);
            if (damagedLibs.Any())
            {
                // For 1.13.2+ forge versions, there is no way to fix damaged forge jar unless reinstall
                if (launchVersion.Type == VersionType.NewForge && damagedLibs.Any(lib => lib.Type == LibraryType.Forge))
                {
                    _windowManager.ShowMessageBox("${ForgeJarDamagedError}\n${PleaseReinstallForge}", null,
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    // Delete the damaged forge version (but retain the libraries)
                    // force user to reinstall it
                    await _versionService.DeleteFromDiskAsync(launchVersion.ID, false);

                    _statusVM.Status = LaunchStatus.Failed;
                    return;
                }

                var downloads = _libraryService.GetDownloads(damagedLibs);
                if (!await StartDownloadAsync(DownloadType.Libraries, downloads))
                {
                    _statusVM.Status = LaunchStatus.Failed;
                    return;
                }
            }

            // Extract native libraries
            _libraryService.ExtractNatives(launchVersion.Libraries.Where(lib => lib.Type == LibraryType.Native));

            // Try loading assets
            if (!_assetService.LoadAllObjects(launchVersion.AssetsInfo))
            {
                if (await _assetService.DownloadIndexJsonAsync(launchVersion.AssetsInfo))
                {
                    // Successfully downloaded the missing index json, load assets
                    _assetService.LoadAllObjects(launchVersion.AssetsInfo);
                }
                // if index json download failed (what are the odds!), not gonna retry
                // Prepare for enjoying a silent game XD
            }

            // Check assets and fix possible damage on user's discretion
            var damagedAssets = await _assetService.CheckIntegrityAsync(launchVersion.AssetsInfo);
            if ((damagedAssets?.Any() ?? false) &&
                _windowManager.ShowMessageBox("${AssetsDamagedError}\n${WhetherFixNow}", null,
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                var downloads = _assetService.GetDownloads(damagedAssets);
                await StartDownloadAsync(DownloadType.Assets, downloads);
            }

            // For legacy versions (1.7.2 or earlier), copy hashed asset objects to virtual files
            await _assetService.CopyToVirtualAsync(launchVersion.AssetsInfo);

            // All good to go, now build launch profile
            var profile = new LaunchProfile
            {
                IsDebugMode = _config.JavaDebugMode,
                JvmArgs = _config.JvmArgs,
                MaxMemory = _config.JavaMaxMem,
                Username = _config.Username,
                UUID = authResult.UUID,
                AccessToken = authResult.AccessToken,
                UserType = authResult.UserType,
                VersionType = AssemblyUtil.Title,
                WinWidth = _config.WindowWidth,
                WinHeight = _config.WindowHeight,
                IsFullScreen = _config.FullScreen,
                ServerAddress = _config.ServerAddress,
                ExtraArgs = _config.ExtraMinecraftArgs,
            };

            _statusVM.Status = LaunchStatus.StartingProcess;

            void UpdateLogDisplay(string logMessage) => _statusVM.GameOutputLog = logMessage;
            _launchService.LogReceived += UpdateLogDisplay;

            if (!await _launchService.LaunchGameAsync(profile, launchVersion))
            {
                _statusVM.Status = LaunchStatus.Failed;
                _launchService.LogReceived -= UpdateLogDisplay;
                return;
            }

            _statusVM.Status = LaunchStatus.Running;

            _launchService.LogReceived -= UpdateLogDisplay;
            _statusVM.GameOutputLog = "_(:3」∠)_";
        }

        #endregion

        #region Private Methods

        private async Task<bool> StartDownloadAsync(DownloadType type, IEnumerable<DownloadItem> items)
        {
            _statusVM.Status = LaunchStatus.Downloading;

            using var downloadService = new DownloadService(items);
            _downloadVM.Setup(type, downloadService);
            this.ActivateItem(_downloadVM);

            bool isSuccessful = await downloadService.StartAsync();

            this.ActivateItem(_statusVM);
            _statusVM.Status = LaunchStatus.ProcessingDependencies;

            return isSuccessful;
        }

        private void OnLaunchCompleted()
        {
            if (_statusVM.Status == LaunchStatus.Failed)
            {
                CanLaunch = true;
                return;
            }

            if (_statusVM.Status == LaunchStatus.Running)
            {
                if (_config.AfterLaunch == AfterLaunchBehavior.Exit)
                {
                    Application.Current.Shutdown(0);
                }

                if (_config.AfterLaunch == AfterLaunchBehavior.Hide)
                {
                    Application.Current.MainWindow.Hide();
                }
            }
        }

        private void OnGameExited(int exitCode)
        {
            CanLaunch = true;

            Execute.OnUIThread(() =>
            {
                if (_config.AfterLaunch != AfterLaunchBehavior.Exit && exitCode != 0 && _logger.Length > 0)
                {
                    _errorReportVM.ErrorMessage = $"Exit Code: {exitCode}\n" + _logger.ToString();
                    _errorReportVM.Type = ErrorReportType.UnexpectedExit;

                    _windowManager.ShowDialog(_errorReportVM);

                    Debug.WriteLine("[Game exited with errors]");
                    Debug.WriteLine(_logger.ToString());
                    _logger.Clear();
                }

                if (_config.AfterLaunch == AfterLaunchBehavior.Hide)
                {
                    Application.Current.MainWindow.Show();
                    Application.Current.MainWindow.Activate();
                }
            });
        }

        #endregion

        #region Override Methods

        protected override void OnViewLoaded()
        {
            if (_statusVM.Status == LaunchStatus.Failed) CanLaunch = true;
        }

        #endregion
    }
}
