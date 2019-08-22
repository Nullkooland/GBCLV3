using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;
using GBCLV3.Models;
using GBCLV3.Services;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels.Windows
{
    class ErrorReportViewModel : Screen
    {
        #region Private Members

        private const string _issuesUrl = "https://github.com/Goose-Bomb/GBCLV3/issues";
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

        public ThemeService ThemeService { get; private set; }

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

        public void Report() => Process.Start(_issuesUrl);

        public void CopyMessage() => Clipboard.SetText(ErrorMessage, TextDataFormat.UnicodeText);

        #endregion

        #region Override Methods

        protected override void OnViewLoaded() => SystemSounds.Exclamation.Play();

        #endregion
    }
}
