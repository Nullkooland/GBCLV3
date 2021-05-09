using System.Media;
using System.Windows;
using GBCLV3.Models;
using GBCLV3.Services;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels.Windows
{
    public class ErrorReportViewModel : Screen
    {
        #region Private Fields

        private readonly LogService _logService;

        private const string ISSUES_URL = "https://github.com/Goose-Bomb/GBCLV3/issues";

        #endregion

        #region Constructor

        [Inject]
        public ErrorReportViewModel(LogService logService, ThemeService themeService)
        {
            _logService = logService;
            ThemeService = themeService;
        }

        #endregion

        #region Bindings

        public ThemeService ThemeService { get; }

        public ErrorReportType Type { get; private set; }

        public int MessageFontSize { get; private set; }

        public string ErrorMessage { get; private set; }

        public void Setup(ErrorReportType type)
        {
            string logs = _logService.ReadLogs();

            Type = type;
            MessageFontSize = (logs.Length < 512) ? 16 : 12;
            ErrorMessage = logs;
        }

        public void Close() => this.RequestClose();

        public void Report() => SystemUtil.OpenLink(ISSUES_URL);

        public void CopyMessage() => Clipboard.SetText($"```ini\n{ErrorMessage}```", TextDataFormat.UnicodeText);

        public void OnWindowLoaded(Window window, RoutedEventArgs _)
        {
            ThemeService.SetBackgroundEffect(window);
            window.Activate();
        }

        #endregion

        #region Override Methods

        protected override void OnViewLoaded() => SystemSounds.Exclamation.Play();

        #endregion
    }
}
