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

        private readonly Config _config;

        private readonly Dictionary<string, string> _availableLanguages;
        private ResourceDictionary _currentLangDict;

        #endregion

        #region Constructor

        [Inject]
        public LanguageService(ConfigService configService)
        {
            _config = configService.Entries;

            var langsResourceDict =
                Application.LoadComponent(new Uri("/Resources/Languages/AvailableLanguages.xaml", UriKind.Relative)) as
                    ResourceDictionary;

            _availableLanguages = langsResourceDict.Keys.Cast<string>()
                .OrderBy(key => key)
                .ToDictionary(
                    key => key,
                    key => langsResourceDict[key] as string
                );

            if (string.IsNullOrEmpty(_config.Language))
            {
                _config.Language = CultureInfo.CurrentCulture.Name;
            }

            if (!_availableLanguages.ContainsKey(_config.Language))
            {
                _config.Language = _availableLanguages.First().Key;
            }

            _currentLangDict =
                Application.LoadComponent(new Uri($"/Resources/Languages/{_config.Language}.xaml", UriKind.Relative)) as
                    ResourceDictionary;
        }

        #endregion

        #region Public Methods

        public void Change(string langTag)
        {
            var replaceLangDict =
                Application.LoadComponent(new Uri($"/Resources/Languages/{langTag}.xaml", UriKind.Relative)) as
                    ResourceDictionary;

            Application.Current.Resources.MergedDictionaries.Remove(_currentLangDict);
            Application.Current.Resources.MergedDictionaries.Add(replaceLangDict);

            _config.Language = langTag;
            _currentLangDict = replaceLangDict;
        }

        public Dictionary<string, string> GetAvailableLanguages() => _availableLanguages;

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