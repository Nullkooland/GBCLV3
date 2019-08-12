using System.Windows;
using GBCLV3.Models;
using GBCLV3.Services;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels
{
    class DownloadViewModel : Screen
    {
        #region Private Members

        // IoC
        private readonly IWindowManager _windowManager;

        private DownloadService _downloadService;

        #endregion

        #region Constructor

        [Inject]
        public DownloadViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        #endregion

        #region Bindings

        public DownloadType Type { get; private set; }

        public int FailedCount { get; private set; }

        public string CountProgress { get; private set; }

        public string BytesProgress { get; private set; }

        public double Percentage { get; private set; }

        public string Speed { get; private set; }

        public void Cancel()
        {
            if (_windowManager.ShowMessageBox("${WhetherCancelDownload}", "${CancelDownload}", 
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _downloadService.Cancel();
            }
        }

        #endregion

        #region Public Methods

        public void NewDownload(DownloadType type, DownloadService downloadService)
        {
            _downloadService = downloadService;
            _downloadService.ProgressChanged += progress =>
            {
                FailedCount = progress.FailedCount;
                CountProgress = $"{progress.CompletedCount} / {progress.TotalCount}";
                BytesProgress = GetBytesProgressText(progress.DownloadedBytes, progress.TotalBytes);
                Percentage = GetPercentage(progress.DownloadedBytes, progress.TotalBytes);

                Speed = GetSpeedText(progress.Speed);
            };
            _downloadService.Completed += OnCompleted;

            // Reset
            Type = type;
            FailedCount = 0;
            CountProgress = null;
            BytesProgress = null;
            Percentage = 0.0;
            Speed = null;
        }

        #endregion

        #region Private Methods

        private double GetPercentage(int downloaded, int total) => (total > 0) ? (double)downloaded / total : 0.0;

        private static string GetBytesProgressText(int downloaded, int total)
        {
            if (total < 1024) return $"{downloaded} / {total} B";
            if (total < 1024 * 1024) return $"{downloaded / 1024}/{total / 1024} KB";
            if (total < 1024 * 1024 * 1024)
            {
                string GetMB(int bytes) => (bytes / (1024.0 * 1024.0)).ToString("0.00");
                return $"{GetMB(downloaded)} / {GetMB(total)} MB";
            }
            return null;
        }  

        private static string GetSpeedText(double speed)
        {
            if (speed < 1024.0) return speed.ToString("0") + " B/s";
            if (speed < 1024.0 * 1024.0) return (speed / 1024.0).ToString("0.0") + " KB/s";
            if (speed < 1024.0 * 1024.0 * 1024.0) return (speed / (1024.0 * 1024.0)).ToString("0.00") + " MB/s";
            return "0";
        }

        private void OnCompleted(DownloadResult result)
        {
            if (result == DownloadResult.Succeeded || result == DownloadResult.Canceled)
            {
                this.RequestClose();
                return;
            }

            var retryMessage = $"{FailedCount} " + "${DownloadFailures}" + '\n' + "${WhetherRetryDownload}";

            if (_windowManager.ShowMessageBox(retryMessage, "${IncompleteDownload}",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _downloadService.Retry();
            }
            else
            {
                _downloadService.Cancel();
            }
        }

        #endregion
    }
}
