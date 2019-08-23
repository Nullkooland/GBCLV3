using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using GBCLV3.Models;
using GBCLV3.Services;
using GBCLV3.Services.Launcher;
using Stylet;
using StyletIoC;
using Version = GBCLV3.Models.Launcher.Version;

namespace GBCLV3.ViewModels
{
    class VersionsManagementViewModel : Screen
    {
        #region Events

        public event Action<string> NavigateView;

        #endregion

        #region Private Members

        //IoC
        private readonly Config _config;
        private readonly GamePathService _gamePathService;
        private readonly VersionService _versionService;

        private readonly GameInstallViewModel _gameInstallVM;
        private readonly ForgeInstallViewModel _forgeInstallVM;

        private readonly IWindowManager _windowManager;

        #endregion

        #region Constructor

        [Inject]
        public VersionsManagementViewModel(
            ConfigService configService,
            GamePathService gamePathService,
            VersionService versionService,

            GameInstallViewModel gameInstallVM,
            ForgeInstallViewModel forgeInstallVM,

            IWindowManager windowManager)
        {
            _windowManager = windowManager;
            _config = configService.Entries;
            _gamePathService = gamePathService;
            _versionService = versionService;

            Versions = new BindableCollection<Version>();
            _versionService.Loaded += hasAny =>
            {
                Versions.Clear();
                Versions.AddRange(_versionService.GetAvailable());
            };
            _versionService.Created += version => Versions.Insert(0, version);
            _versionService.Deleted += version => Versions.Remove(version);

            _gameInstallVM = gameInstallVM;
            _forgeInstallVM = forgeInstallVM;
        }

        #endregion

        #region Bindings

        public BindableCollection<Version> Versions { get; set; }

        public bool IsSegregateVersions
        {
            get => _config.SegregateVersions;
            set => _config.SegregateVersions = value;
        }

        public string SelectedVersionID { get; set; }

        public void Reload() => _versionService.LoadAll();

        public void OpenDir()
        {
            string versionsDir = $"{_gamePathService.VersionsDir}/{SelectedVersionID}";
            if (Directory.Exists(versionsDir)) Process.Start(versionsDir);
        }

        public void OpenJson()
        {
            string jsonPath = $"{_gamePathService.VersionsDir}/{SelectedVersionID}/{SelectedVersionID}.json";
            if (File.Exists(jsonPath)) Process.Start(jsonPath);
        }

        public async void Delete()
        {
            if (_windowManager.ShowMessageBox("${WhetherDeleteVersion} " + SelectedVersionID + " ?", "${DeleteVersion}",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await _versionService.DeleteFromDiskAsync(SelectedVersionID, true);
            }
        }

        public void InstallNew() => NavigateView?.Invoke(null);

        public void InstallForge()
        {
            var version = _versionService.GetByID(SelectedVersionID);
            NavigateView?.Invoke(version.JarID);
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
