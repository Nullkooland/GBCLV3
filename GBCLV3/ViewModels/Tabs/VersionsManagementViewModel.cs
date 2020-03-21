using GBCLV3.Models;
using GBCLV3.Models.Installation;
using GBCLV3.Services;
using GBCLV3.Services.Launch;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using Version = GBCLV3.Models.Launch.Version;

namespace GBCLV3.ViewModels.Tabs
{
    public class VersionsManagementViewModel : Screen
    {
        #region Events

        public event Action<Version, InstallType> NavigateInstallView;

        #endregion

        #region Private Fields

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

            // OnVersionsLoaded
            _versionService.Loaded += hasAny =>
            {
                string selectedVersion = _config.SelectedVersion;
                Versions.Clear();
                Versions.AddRange(_versionService.GetAll());
                SelectedVersionID = _versionService.Has(selectedVersion)
                    ? selectedVersion
                    : Versions.FirstOrDefault()?.ID;
            };

            // OnVersionCreated
            _versionService.Created += version =>
            {
                Versions.Insert(0, version);
                SelectedVersionID = version.ID;
            };

            // OnVersionDeleted
            _versionService.Deleted += version =>
            {
                Versions.Remove(version);
                SelectedVersionID ??= Versions.FirstOrDefault()?.ID;
            };
        }

        #endregion

        #region Bindings

        public BindableCollection<Version> Versions { get; set; }

        public string SelectedVersionID
        {
            get => _config.SelectedVersion;
            set => _config.SelectedVersion = value;
        }

        public bool IsSegregateVersions
        {
            get => _config.SegregateVersions;
            set => _config.SegregateVersions = value;
        }

        public void Reload() => _versionService.LoadAll();

        public void OpenDir(string id)
        {
            string versionsDir = $"{_gamePathService.VersionsDir}/{id}";
            if (Directory.Exists(versionsDir)) SystemUtil.OpenLink(versionsDir);
        }

        public void OpenJson(string id)
        {
            string jsonPath = $"{_gamePathService.VersionsDir}/{id}/{id}.json";
            if (File.Exists(jsonPath)) SystemUtil.OpenLink(jsonPath);
        }

        public async void Delete(string id)
        {
            if (_windowManager.ShowMessageBox("${WhetherDeleteVersion} " + id + " ?", "${DeleteVersion}",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                await _versionService.DeleteFromDiskAsync(id, true);
            }
        }

        public void InstallNew() => NavigateInstallView?.Invoke(null, InstallType.Version);

        public void InstallForge(Version version) => NavigateInstallView?.Invoke(version, InstallType.Forge);

        public void InstallFabric(Version version) => NavigateInstallView?.Invoke(version, InstallType.Fabric);
        

        //public void InstallOptiFine()
        //{
        //}

        #endregion
    }
}
