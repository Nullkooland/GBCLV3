using GBCLV3.Models;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GBCLV3.Services
{
    class ThemeService : PropertyChangedBase
    {
        #region Binding Properties

        public BitmapImage BackgroundImage { get; private set; }

        public StreamGeometry BackgroundIcon { get; private set; }

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

        #region Private Fields

        private const string ICONS_SOURCE = "/GBCL;component/Resources/Styles/Icons.xaml";
        private const string DEFAULT_BACKGROUND_IMAGE = "pack://application:,,,/Resources/Images/default_background.png";

        private static readonly Color REF_COLOR_SPIKE = Color.FromRgb(15, 105, 200);
        private static readonly Color REF_COLOR_BULLZEYE = Color.FromRgb(210, 50, 55);
        private static readonly Color REF_COLOR_TBONE = Color.FromRgb(165, 125, 10);
        private static readonly Color REF_COLOR_STEGZ = Color.FromRgb(105, 175, 5);

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

        public void LoadBackgroundIcon(Color accentColor)
        {
            ResourceDictionary iconsDict = null;
            foreach (var dict in Application.Current.Resources.MergedDictionaries)
            {
                if (dict.Source?.ToString() == ICONS_SOURCE)
                {
                    iconsDict = dict;
                    break;
                }
            }

#if DEBUG
            float l2_norm_0 = ColorUtil.CalcL2Norm(accentColor, REF_COLOR_SPIKE);
            float l2_norm_1 = ColorUtil.CalcL2Norm(accentColor, REF_COLOR_BULLZEYE);
            float l2_norm_2 = ColorUtil.CalcL2Norm(accentColor, REF_COLOR_TBONE);
            float l2_norm_3 = ColorUtil.CalcL2Norm(accentColor, REF_COLOR_STEGZ);

            Debug.WriteLine("[REF COLOR L2 NORM]");
            Debug.WriteLine(l2_norm_0);
            Debug.WriteLine(l2_norm_1);
            Debug.WriteLine(l2_norm_2);
            Debug.WriteLine(l2_norm_3);
#endif

            if (ColorUtil.CalcL2Norm(accentColor, REF_COLOR_SPIKE) < 0.0075f)
            {
                BackgroundIcon = iconsDict["Spike"] as StreamGeometry;
            }
            else if (ColorUtil.CalcL2Norm(accentColor, REF_COLOR_BULLZEYE) < 0.0005f)
            {
                BackgroundIcon = iconsDict["Bullzeye"] as StreamGeometry;
            }
            else if (ColorUtil.CalcL2Norm(accentColor, REF_COLOR_TBONE) < 0.0082f)
            {
                BackgroundIcon = iconsDict["T-Bone"] as StreamGeometry;
            }
            //else if (ColorUtil.CalcL2Norm(accentColor, REF_COLOR_STEGZ) < COLOR_L2_THRESHOLD)
            //{
            //    BackgroundIcon = iconsDict["Stegz"] as StreamGeometry;
            //}
            else
            {
                BackgroundIcon = iconsDict["DragonIcon"] as StreamGeometry;
            }
        }

        #endregion
    }
}
