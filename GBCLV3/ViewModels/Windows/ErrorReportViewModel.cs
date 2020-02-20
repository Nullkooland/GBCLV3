using GBCLV3.Models;
using GBCLV3.Services;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;
using System.Media;
using System.Windows;

namespace GBCLV3.ViewModels.Windows
{
    public class ErrorReportViewModel : Screen
    {
        #region Private Fields

        private const string ISSUES_URL = "https://github.com/Goose-Bomb/GBCLV3/issues";
        private string _errorMessage;

        #endregion

        #region Constructor

        [Inject]
        public ErrorReportViewModel(ThemeService themeService)
        {
            ThemeService = themeService;
        }

        #endregion

        #region Bindings

        public ThemeService ThemeService { get; }

        public ErrorReportType Type { get; set; }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                MessageFontSize = (value.Length < 512) ? 16 : 12;
                _errorMessage = $"{value}\n[Launcher Version: {AssemblyUtil.Version}]";
            }
        }

        public int MessageFontSize { get; private set; }

        public void Close() => this.RequestClose();

        public void Report() => SystemUtil.OpenLink(ISSUES_URL);

        public void CopyMessage() => Clipboard.SetText(ErrorMessage, TextDataFormat.UnicodeText);

        #endregion

        #region Override Methods

        protected override void OnViewLoaded() => SystemSounds.Exclamation.Play();

        #endregion
    }
}
