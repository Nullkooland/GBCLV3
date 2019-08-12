using System;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using GBCLV3.Models;
using GBCLV3.Services;
using GBCLV3.Services.Launcher;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels
{
    class GameSettingsViewModel : Screen
    {
        #region Private Members

        // IoC
        private readonly IEventAggregator _eventAggregator;

        private readonly Config _config;
        private readonly LanguageService _languageService;
        private readonly VersionService _versionService;

        #endregion

        #region Constructor

        [Inject]
        public GameSettingsViewModel(
            IEventAggregator eventAggregator,

            ConfigService configService,
            LanguageService languageService,
            VersionService versionService)
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
                if (value > SystemUtil.GetAvailableMemory())
                {
                    _config.JavaMaxMem = SystemUtil.GetRecommendedMemory();
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

        public string AvailableMemory =>
            _languageService.GetEntry("AvailableMem") + $" {SystemUtil.GetAvailableMemory()} MB";

        public string GameDir
        {
            get => _config.GameDir;
            set => _config.GameDir = value;
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

        public string Username
        {
            get => (_config.OfflineMode) ? _config.Username : _config.Email;
            set
            {
                if (_config.OfflineMode)
                {
                    _config.Username = value;
                    _eventAggregator.Publish(new UsernameChangedEvent());
                }
                else _config.Email = value;
            }
        }

        public bool IsOfflineMode
        {
            get => _config.OfflineMode;
            set
            {
                _config.OfflineMode = value;
                NotifyOfPropertyChange(nameof(Username));
            }
        }

        public bool IsRefreshAuth
        {
            get => _config.RefreshAuth;
            set
            {
                _config.RefreshAuth = value;
            }
        }

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

        public bool IsShowAdvancedSettings { get; set; }

        public void OnPasswordLoaded(PasswordBox passwordBox, EventArgs args)
        {
            passwordBox.Password = _config.Password;
        }

        public void OnPasswordChanged(PasswordBox passwordBox, EventArgs args)
        {
            _config.Password = passwordBox.Password;
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

        #endregion

        #region Private Methods

        #endregion

        #region Override Methods

        protected override void OnViewLoaded()
        {
            IsShowAdvancedSettings =
                !(string.IsNullOrEmpty(_config.JvmArgs) && string.IsNullOrEmpty(_config.ExtraMinecraftArgs));

            NotifyOfPropertyChange(nameof(IsRefreshAuth));
        }

        #endregion
    }
}
