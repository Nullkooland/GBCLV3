using GBCLV3.Models;
using GBCLV3.Models.Installation;
using GBCLV3.Services;
using GBCLV3.Services.Launch;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;
using System;
using System.IO;
using System.Windows;
using Version = GBCLV3.Models.Launch.Version;

namespace GBCLV3.ViewModels.Tabs
{
    class VersionsManagementViewModel : Screen
    {
        #region Events

        public event Action<string, InstallType> NavigateInstallView;

        #endregion

        #region Private Members

        //IoC
        private readonly Config _config;
        private readonly GamePathService _gamePathService;
        private readonly VersionService _versionService;

        private readonly IWindowManager _windowManager;

        #endregion

        #region Constructor

        [Inject]
        public VersionsManagementViewModel(
            ConfigService configService,
            GamePathService gamePathService,
            VersionService versionService,

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
            if (Directory.Exists(versionsDir)) SystemUtil.OpenLink(versionsDir);
        }

        public void OpenJson()
        {
            string jsonPath = $"{_gamePathService.VersionsDir}/{SelectedVersionID}/{SelectedVersionID}.json";
            if (File.Exists(jsonPath)) SystemUtil.OpenLink(jsonPath);
        }

        public async void Delete()
        {
            if (_windowManager.ShowMessageBox("${WhetherDeleteVersion} " + SelectedVersionID + " ?", "${DeleteVersion}",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await _versionService.DeleteFromDiskAsync(SelectedVersionID, true);
            }
        }

        public void InstallNew() => NavigateInstallView?.Invoke(null, InstallType.Version);

        public void InstallForge()
        {
            var version = _versionService.GetByID(SelectedVersionID);
            NavigateInstallView?.Invoke(version.JarID, InstallType.Forge);
        }

        public void InstallFabric()
        {
            var version = _versionService.GetByID(SelectedVersionID);
            NavigateInstallView?.Invoke(version.JarID, InstallType.Fabric);
        }

        public void InstallOptiFine()
        {
            var version = _versionService.GetByID(SelectedVersionID);
            NavigateInstallView?.Invoke(version.JarID, InstallType.OptiFine);
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
