using System.Linq;
using GBCLV3.Models;
using GBCLV3.Models.Download;
using GBCLV3.Models.Launch;
using GBCLV3.Models.Theme;
using GBCLV3.Services;
using GBCLV3.Services.Download;
using GBCLV3.ViewModels.Windows;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels.Tabs
{
    public class LauncherSettingsViewModel : Screen
    {
        #region Private Fields

        // IoC
        private readonly Config _config;
        private readonly LanguageService _languageService;
        private readonly UpdateService _updateService;
        private readonly ThemeService _themeService;

        private readonly UpdateViewModel _updateVM;
        private readonly IWindowManager _windowManager;

        #endregion

        #region Constructor

        [Inject]
        public LauncherSettingsViewModel(
            ConfigService configService,
            LanguageService languageService,
            UpdateService updateService,
            ThemeService themeService,
            UpdateViewModel updateVM,
            IWindowManager windowManager)
        {
            _config = configService.Entries;
            _languageService = languageService;
            _themeService = themeService;
            _updateService = updateService;

            _updateService.CheckStatusChanged += status => CheckStatus = status;

            _updateVM = updateVM;
            _windowManager = windowManager;

            Languages = _languageService.GetAvailableLanguages()
                .Select(pair => new LabelledValue<string>(pair.Value, pair.Key))
                .ToArray();
        }

        #endregion

        #region Bindings

        public LabelledValue<string>[] Languages { get; }

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

        public BackgroundEffect SelectedBackgroundEffect
        {
            get => _config.BackgroundEffect;
            set
            {
                _config.BackgroundEffect = value;
                _themeService.SetBackgroundEffect();
            }
        }

        public bool IsAutoCheckUpdate
        {
            get => _config.AutoCheckUpdate;
            set => _config.AutoCheckUpdate = value;
        }

        public CheckUpdateStatus CheckStatus { get; private set; }

        public bool IsFreeToCheckUpdate => CheckStatus != CheckUpdateStatus.Checking;

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

        public async void CheckUpdate()
        {
            if (CheckStatus == CheckUpdateStatus.UpToDate) return;

            var info = await _updateService.CheckAsync();

            if (info != null)
            {
                _updateVM.Setup(info);
                _windowManager.ShowWindow(_updateVM);
            }
        }

        public void SelectBackgoundImagePath()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Title = _languageService.GetEntry("SelectImagePath"),
                Filter = "Images | *.jpg; *.jpeg; *.jfif; *.bmp; *.png; *.tif; *.tiff; *.webp;",
            };

            if (dialog.ShowDialog() ?? false)
            {
                BackgroundImagePath = dialog.FileName;
            }
        }

        #endregion

        #region Private Methods

        protected override void OnViewLoaded()
        {
            if (CheckStatus == CheckUpdateStatus.CheckFailed) CheckStatus = CheckUpdateStatus.Unknown;
        }

        #endregion
    }
}