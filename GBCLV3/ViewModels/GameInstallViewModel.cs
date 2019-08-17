using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using GBCLV3.Models.Launcher;
using GBCLV3.Services.Launcher;
using Stylet;

namespace GBCLV3.ViewModels
{
    class GameInstallViewModel : Screen
    {
        #region Private Members

        #endregion

        private readonly VersionService _versionService;

        #region Constructor

        public GameInstallViewModel(VersionService versionService)
        {
            _versionService = versionService;

            VersionDownloads = new BindableCollection<VersionDownload>();
        }

        #endregion

        #region Bindings

        public bool IsLoading => Status == VersionListStatus.Loading;

        public VersionListStatus Status { get; private set; }

        public BindableCollection<VersionDownload> VersionDownloads { get; set; }

        public void GoBack() => this.RequestClose();

        #endregion

        #region Private Methods


        protected override async void OnActivate()
        {
            if (Status != VersionListStatus.Loaded)
            {
                Status = VersionListStatus.Loading;
                var (downloads, latestVersion) = await _versionService.GetDownloadListAsync();

                if (downloads != null)
                {
                    VersionDownloads.Clear();
                    VersionDownloads.AddRange(downloads);
                    Status = VersionListStatus.Loaded;
                }
                else
                {
                    Status = VersionListStatus.LoadFailed;
                }
            }
        }

        #endregion
    }
}
