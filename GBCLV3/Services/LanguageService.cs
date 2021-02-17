using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using GBCLV3.Models;
using StyletIoC;

namespace GBCLV3.Services
{
    public class LanguageService
    {
        #region Private Fields

        private const string AVAILABLE_LANGS_DICT_PATH = "/Resources/Languages/AvailableLanguages.xaml";
        private const string TRANSLATOR_DICT_PATH = "/Resources/Languages/AvailableLanguages.xaml";
        private const string STRINGS_DICTS_DIR = "/Resources/Languages/Strings";

        private readonly Config _config;

        private readonly Dictionary<string, string> _availableLanguages;
        private readonly Dictionary<string, string> _translators;
        private ResourceDictionary _currentLangDict;

        #endregion

        #region Constructor

        [Inject]
        public LanguageService(ConfigService configService)
        {
            _config = configService.Entries;

            var availableLangsDict =
                Application.LoadComponent(new Uri(AVAILABLE_LANGS_DICT_PATH, UriKind.Relative)) as
                    ResourceDictionary;

            var translatorsDict = Application.LoadComponent(new Uri(TRANSLATOR_DICT_PATH, UriKind.Relative)) as
                    ResourceDictionary;

            _availableLanguages = availableLangsDict.Keys.Cast<string>()
                .OrderBy(key => key)
                .ToDictionary(
                    key => key,
                    key => availableLangsDict[key] as string
                );

            _translators = _availableLanguages.Keys
                .ToDictionary(key => key, key => translatorsDict[key] as string);

            if (string.IsNullOrEmpty(_config.Language))
            {
                _config.Language = CultureInfo.CurrentCulture.Name;
            }

            if (!_availableLanguages.ContainsKey(_config.Language))
            {
                _config.Language = _availableLanguages.First().Key;
            }

            _currentLangDict =
                Application.LoadComponent(new Uri($"{STRINGS_DICTS_DIR}/{_config.Language}.xaml", UriKind.Relative)) as
                    ResourceDictionary;

            Application.Current.Resources.MergedDictionaries.Add(_currentLangDict);
        }

        #endregion

        #region Public Methods

        public void Change(string langTag)
        {
            var replaceLangDict =
                Application.LoadComponent(new Uri($"{STRINGS_DICTS_DIR}/{langTag}.xaml", UriKind.Relative)) as
                    ResourceDictionary;

            Application.Current.Resources.MergedDictionaries.Remove(_currentLangDict);
            Application.Current.Resources.MergedDictionaries.Add(replaceLangDict);

            _config.Language = langTag;
            _currentLangDict = replaceLangDict;
        }

        public Dictionary<string, string> GetAvailableLanguages() => _availableLanguages;

        public Dictionary<string, string> GetTranslators() => _translators;

        public string GetEntry(string key)
        {
            return !string.IsNullOrEmpty(key) ? _currentLangDict[key] as string : null;
        }

        public string ReplaceKeyToEntry(string src)
        {
            return Regex.Replace(src ?? string.Empty, "\\${.*?}", match =>
            {
                string key = match.Value[2..^1];
                return GetEntry(key);
            });
        }

        #endregion
    }
}