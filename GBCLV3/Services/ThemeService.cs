using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GBCLV3.Models;
using Stylet;
using StyletIoC;

namespace GBCLV3.Services
{
    class ThemeService : PropertyChangedBase
    {
        #region Properties

        public BitmapImage BackgroundImage { get; private set; }

        public string FontFamily
        {
            get => _config.FontFamily;
            set => _config.FontFamily = value;
        }

        public string FontWeight
        {
            get => _config.FontWeight;
            set => _config.FontWeight = value;
        }

        #endregion

        #region Private Members

        private const string DEFAULT_BACKGROUND_IMAGE = "pack://application:,,,/Resources/Images/default_background.png";
        private readonly Config _config;

        #endregion

        #region Constructor

        [Inject]
        public ThemeService(ConfigService configService)
        {
            _config = configService.Entries;
            if (string.IsNullOrEmpty(_config.FontFamily))
            {
                _config.FontFamily = "Microsoft YaHei UI";
            }
        }

        #endregion

        #region Public Methods

        public void UpdateBackgroundImage()
        {
            if (!_config.UseBackgroundImage)
            {
                BackgroundImage = null;
                return;
            }

            string imgPath = null;

            if (File.Exists(_config.BackgroundImagePath))
            {
                imgPath = _config.BackgroundImagePath;
            }
            else
            {
                _config.BackgroundImagePath = null;

                string imgSearchDir = Environment.CurrentDirectory + "/bg";
                if (Directory.Exists(imgSearchDir))
                {
                    string[] imgExtensions = { ".png", ".jpg", ".jpeg", ".jfif", ".bmp", ".tif", ".tiff" };
                    string[] imgFiles = Directory.EnumerateFiles(imgSearchDir)
                                                 .Where(file => imgExtensions.Any(file.ToLower().EndsWith))
                                                 .ToArray();

                    if (imgFiles.Any())
                    {
                        var rand = new Random();
                        imgPath = imgFiles[rand.Next(imgFiles.Length)];
                    }
                }
            }

            BackgroundImage = new BitmapImage();
            BackgroundImage.BeginInit();
            BackgroundImage.UriSource = new Uri(imgPath ?? DEFAULT_BACKGROUND_IMAGE);
            BackgroundImage.CacheOption = BitmapCacheOption.OnLoad;
            BackgroundImage.EndInit();
            BackgroundImage.Freeze();
        }

        #endregion

        #region Public Methods

        public string[] GetSystemFontNames()
        {
            return Fonts.SystemFontFamilies.Select(fontFamily =>
            {
                var nameDict = fontFamily.FamilyNames;

                if (nameDict.TryGetValue(XmlLanguage.GetLanguage(_config.Language), out string fontName))
                {
                    return fontName;
                }

                if (nameDict.TryGetValue(XmlLanguage.GetLanguage("en-us"), out fontName))
                {
                    return fontName;
                }

                return null;
            })
            .Where(fontName => !string.IsNullOrEmpty(fontName))
            .OrderBy(fontName => fontName)
            .ToArray();
        }

        public string[] GetFontWeights()
        {
            var fontWeights = new FontWeight[]
            {
                FontWeights.Light,
                FontWeights.Normal,
                FontWeights.Medium,
                FontWeights.SemiBold,
                FontWeights.Bold,
            };

            return fontWeights.Select(weight => weight.ToString()).ToArray();
        }

        #endregion
    }
}
