using System.Diagnostics;
using System.Windows;
using GBCLV3.Models;
using GBCLV3.Models.Launcher;
using GBCLV3.Services;
using GBCLV3.Services.Launcher;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels
{
    class VersionManagementViewModel : Screen
    {
        #region Private Members

        private readonly IWindowManager _windowManager;
        private readonly Config _config;
        private readonly GamePathService _gamePathService;
        private readonly VersionService _versionService;
        private readonly LanguageService _languageService;

        #endregion

        #region Constructor

        [Inject]
        public VersionManagementViewModel(
            IWindowManager windowManager,
            ConfigService configService,
            GamePathService gamePathService,
            VersionService versionService,
            LanguageService languageService)
        {
            _windowManager = windowManager;
            _config = configService.Entries;
            _gamePathService = gamePathService;
            _versionService = versionService;
            _languageService = languageService;

            Versions = new BindableCollection<Version>();
            _versionService.Loaded += hasAny =>
            {
                Versions.Clear();
                Versions.AddRange(_versionService.GetAvailable());
            };
            _versionService.Created += version => Versions.Add(version);
            _versionService.Deleted += version => Versions.Remove(version);
        }

        #endregion

        #region Bindings

        public BindableCollection<Version> Versions { get; set; }

        public string SelectedVersionID { get; set; }

        public void Reload() => _versionService.LoadAll();

        public void OpenVersionsDir() => Process.Start(_gamePathService.VersionDir);

        public void OpenVersionDir()
            => Process.Start($"{_gamePathService.VersionDir}/{SelectedVersionID}");

        public void OpenVersionJson()
            => Process.Start($"{_gamePathService.VersionDir}/{SelectedVersionID}/{SelectedVersionID}.json");

        public void InstallForge()
        {
            var version = _versionService.GetByID(SelectedVersionID);
            // TO-DO: jump to forgeInstall View
        }

        public async void DeleteVersion()
        {
            if (_windowManager.ShowMessageBox("${WhetherDeleteVersion} \"" + SelectedVersionID + "\" ?", "${DeleteVersion}",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await _versionService.DeleteFromDiskAsync(SelectedVersionID);
            }
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
