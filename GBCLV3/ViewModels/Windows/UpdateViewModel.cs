using System.Text;
using System.Windows;
using GBCLV3.Models;
using GBCLV3.Services;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels.Windows
{
    class UpdateViewModel : Screen
    {
        #region Private Members

        private DownloadService _downloadService;

        // IoC
        private readonly UpdateService _updateService;

        private readonly IWindowManager _windowManager;

        #endregion

        #region Constructor

        [Inject]
        public UpdateViewModel(
            ThemeService themeService,
            UpdateService updateService,
            IWindowManager windowManager)
        {
            _updateService = updateService;
            _windowManager = windowManager;

            ThemeService = themeService;
        }

        #endregion

        #region Public Method

        public void Setup(UpdateInfo info)
        {
            var download = _updateService.GetDownload(info);

            _downloadService = new DownloadService(download);

            _downloadService.ProgressChanged += progress =>
            {
                DownloadProgress = (double)progress.DownloadedBytes / progress.TotalBytes;
                Percentage = (DownloadProgress * 100.0).ToString("0.0") + '%';
            };

            _downloadService.Completed += result =>
            {
                if (result == DownloadResult.Incomplete) _downloadService.Cancel();
            };

            IsDownloading = false;

            DisplayUpdateInfo(info);
        }


        #endregion

        #region Bindings

        public ThemeService ThemeService { get; private set; }

        public string Version { get; private set; }

        public string ChangelogTitle { get; private set; }

        public string ChangelogDetails { get; private set; }

        public bool IsDownloading { get; private set; }

        public double DownloadProgress { get; private set; }

        public string Percentage { get; set; }

        public async void Update()
        {
            IsDownloading = true;

            if (await _downloadService.StartAsync())
            {
                _updateService.Update();
            }
            else
            {
                _windowManager.ShowMessageBox("${UpdateFailed}");
                this.RequestClose();
            }

            IsDownloading = false;
        }

        public void Defer() => this.RequestClose();

        #endregion

        #region Private Methods

        private async void DisplayUpdateInfo(UpdateInfo info)
        {
            Version = $"{info.Name} - {info.ReleaseTime.ToString("yyyy/MM/dd")}";

            // Download and display changelog
            var changelog = await _updateService.GetChangelog(info);
            ChangelogTitle = changelog.Title;

            var builder = new StringBuilder(512);
            foreach (string line in changelog.Details)
            {
                builder.Append("> ").Append(line).AppendLine();
            }

            ChangelogDetails = builder.ToString();
        }

        protected override void OnViewLoaded()
        {
            var window = this.View as Window;

            window.Closing += (sender, e) =>
            {
                if (IsDownloading && this.IsActive)
                {
                    if (_windowManager.ShowMessageBox("${WhetherCancelUpdate}", "${CancelUpdate}",
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _downloadService.Cancel();
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
            };
        }

        #endregion
    }
}
