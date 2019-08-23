using GBCLV3.Models;
using GBCLV3.Models.Launcher;
using GBCLV3.Services;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels
{
    class LauncherSettingsViewModel : Screen
    {
        #region Private Members

        // IoC
        private readonly Config _config;
        private readonly LanguageService _languageService;
        private readonly ThemeService _themeService;

        #endregion

        #region Constructor

        [Inject]
        public LauncherSettingsViewModel(
            ConfigService configService,
            LanguageService languageService,
            ThemeService themeService)
        {
            _config = configService.Entries;
            _languageService = languageService;
            _themeService = themeService;
        }

        #endregion

        #region Bindings

        public LabelledValue<string>[] Languages { get; } =
        {
            new LabelledValue<string>("简体中文", "ZH-CN"),
            new LabelledValue<string>("English", "EN-US"),
        };

        public string SelectedLanguage
        {
            get => _config.Language;
            set
            {
                _config.Language = value;
                _languageService.Change(value);
            }
        }

        public DownloadSource SelectedDownloadSource
        {
            get => _config.DownloadSource;
            set => _config.DownloadSource = value;
        }

        public AfterLaunchBehavior SelectedAfterLaunchBehavior
        {
            get => _config.AfterLaunch;
            set => _config.AfterLaunch = value;
        }

        public bool IsUseBackgroundImage
        {
            get => _config.UseBackgroundImage;
            set
            {
                _config.UseBackgroundImage = value;
                _themeService.UpdateBackgroundImage();
            }
        }

        public bool IsSelectRandomImage => string.IsNullOrWhiteSpace(BackgroundImagePath);

        public string BackgroundImagePath
        {
            get => _config.BackgroundImagePath;
            set
            {
                _config.BackgroundImagePath = value;
                _themeService.UpdateBackgroundImage();
            }
        }

        public string[] SystemFontNames => _themeService.GetSystemFontNames();

        public string SelectedFontFamily
        {
            get => _themeService.FontFamily;
            set => _themeService.FontFamily = value;
        }

        public string[] FontWeights => _themeService.GetFontWeights();

        public string SelectedFontWeight
        {
            get => _themeService.FontWeight;
            set => _themeService.FontWeight = value;
        }

        public void SelectBackgoundImagePath()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = _languageService.GetEntry("SelectImagePath"),
                Filter = "Images | *.jpg; *.jpeg; *.jfif; *.bmp; *.png, *.tif; *.tiff",
            };

            if (dialog.ShowDialog() ?? false)
            {
                BackgroundImagePath = dialog.FileName;
            }
        }

        #endregion

        #region Private Methods

        #endregion
    }
}
