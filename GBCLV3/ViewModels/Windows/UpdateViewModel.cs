using GBCLV3.Models.Download;
using GBCLV3.Services;
using GBCLV3.Services.Download;
using Stylet;
using StyletIoC;
using System.Text;
using System.Windows;

namespace GBCLV3.ViewModels.Windows
{
    public class UpdateViewModel : Screen
    {
        #region Private Fields

        // IoC
        private readonly UpdateService _updateService;
        private DownloadService _downloadService;

        private readonly IWindowManager _windowManager;

        #endregion

        #region Constructor

        [Inject]
        public UpdateViewModel(
            ThemeService themeService,
            UpdateService updateService,
            DownloadService downloadService,
            IWindowManager windowManager)
        {
            _updateService = updateService;
            _downloadService = downloadService;
            _windowManager = windowManager;

            ThemeService = themeService;
        }

        #endregion

        #region Public Method

        public void Setup(UpdateInfo info)
        {
            var download = _updateService.GetDownload(info);
            _downloadService.Setup(download);
 
            IsDownloading = false;
            DisplayUpdateInfo(info);
        }


        #endregion

        #region Bindings

        public ThemeService ThemeService { get; }

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

        public void OnWindowLoaded(Window window, RoutedEventArgs _)
        {
            ThemeService.SetBackgroundEffect(window);
        }

        #endregion

        #region Private Methods

        public void OnDownloadCompleted(DownloadResult result)
        {
            if (result == DownloadResult.Incomplete) _downloadService.Cancel();

            _downloadService.Completed -= OnDownloadCompleted;
            _downloadService.ProgressChanged -= OnDownloadProgressChanged;
        }

        public void OnDownloadProgressChanged(DownloadProgress progress)
        {
            DownloadProgress = (double)progress.DownloadedBytes / progress.TotalBytes;
            Percentage = (DownloadProgress * 100.0).ToString("0.0") + '%';
        }

        private async void DisplayUpdateInfo(UpdateInfo info)
        {
            Version = $"{info.Name} - {info.ReleaseTime:yyyy/MM/dd}";

            // Download and display changelog
            var changelog = await _updateService.GetChangelogAsync(info);
            if (changelog == null) return;

            var builder = new StringBuilder(1024);
            foreach (string line in changelog.Details)
            {
                builder.Append("❯ ").Append(line).AppendLine();
            }

            ChangelogTitle = changelog.Title;
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
