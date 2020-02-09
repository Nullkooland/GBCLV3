using GBCLV3.Models;
using GBCLV3.Services;
using GBCLV3.Services.Launch;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;
using System.IO;

namespace GBCLV3.ViewModels.Tabs
{
    class GameSettingsViewModel : Screen
    {
        #region Private Fields

        const string JRE_DOWNLOAD_URL = "https://bmclapi.bangbang93.com/java/jre_x64.exe";

        // IoC
        private readonly Config _config;
        private readonly LanguageService _languageService;
        private readonly VersionService _versionService;

        private readonly IEventAggregator _eventAggregator;

        #endregion

        #region Constructor

        [Inject]
        public GameSettingsViewModel(
            ConfigService configService,
            LanguageService languageService,
            VersionService versionService,

            IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;

            _config = configService.Entries;
            _languageService = languageService;
            _versionService = versionService;
        }

        #endregion

        #region Bindings

        public string JreDir
        {
            get => _config.JreDir;
            set => _config.JreDir = value;
        }

        public uint JavaMaxMemory
        {
            get => _config.JavaMaxMem;
            set
            {
                if (value > NativeUtil.GetAvailablePhysicalMemory())
                {
                    _config.JavaMaxMem = NativeUtil.GetRecommendedMemory();
                    NotifyOfPropertyChange(nameof(JavaMaxMemory));
                }
                else
                {
                    _config.JavaMaxMem = value;
                }
            }
        }

        public bool IsDebugMode
        {
            get => _config.JavaDebugMode;
            set => _config.JavaDebugMode = value;
        }

        public string AvailableMemory { get; private set; }

        public string GameDir
        {
            get => _config.GameDir;
            set
            {
                _config.GameDir = value;
                _versionService.LoadAll();
            }
        }

        public uint WindowWidth
        {
            get => _config.WindowWidth;
            set => _config.WindowWidth = value;
        }

        public uint WindowHeight
        {
            get => _config.WindowHeight;
            set => _config.WindowHeight = value;
        }

        public bool IsFullScreen
        {
            get => _config.FullScreen;
            set => _config.FullScreen = value;
        }

        //public AuthMode AuthMode
        //{
        //    get => _config.AuthMode;
        //    set => _config.AuthMode = value;
        //}

        //public bool IsOffline => AuthMode == AuthMode.Offline;

        //public bool IsExternalAuth => AuthMode == AuthMode.AuthLibInjector;

        //public string Username
        //{
        //    get => (AuthMode == AuthMode.Offline) ? _config.Username : _config.Email;
        //    set
        //    {
        //        if (AuthMode == AuthMode.Offline)
        //        {
        //            _config.Username = value;
        //            _eventAggregator.Publish(new UsernameChangedEvent());
        //        }
        //        else _config.Email = value;
        //    }
        //}

        public string ServerAddress
        {
            get => _config.ServerAddress;
            set => _config.ServerAddress = value;
        }

        public string JvmArgs
        {
            get => _config.JvmArgs;
            set => _config.JvmArgs = value;
        }

        public string ExtraMinecraftArgs
        {
            get => _config.ExtraMinecraftArgs;
            set => _config.ExtraMinecraftArgs = value;
        }

        public void SelectJrePath()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = _languageService.GetEntry("SelectJrePath"),
                Filter = "JRE | javaw.exe; java.exe",
            };

            if (dialog.ShowDialog() ?? false)
            {
                JreDir = Path.GetDirectoryName(dialog.FileName);
            }
        }

        public void DonwloadJreInstaller() => SystemUtil.OpenLink(JRE_DOWNLOAD_URL);

        public void SelectGameDir()
        {
            // How ludicrous! WPF doesn't even have a built-in OpenFolderDialog?
            // They thought nobody ever needed such a functionality?
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog()
            {
                Description = _languageService.GetEntry("SelectGameDir"),
                UseDescriptionForTitle = true,
            };

            if (dialog.ShowDialog().GetValueOrDefault(false))
            {
                GameDir = dialog.SelectedPath;
                _versionService.LoadAll();
            }
        }

        public void UpdateAvailableMemoryDisplay()
        {
            AvailableMemory =
                _languageService.GetEntry("AvailableMem") + $" {NativeUtil.GetAvailablePhysicalMemory()} MB";
        }

        #endregion

        #region Private Methods

        #endregion

        #region Override Methods

        protected override void OnViewLoaded()
        {
            UpdateAvailableMemoryDisplay();
        }

        #endregion
    }
}
