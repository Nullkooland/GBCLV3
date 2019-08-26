using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;

namespace GBCLV3.Services
{
    class LanguageService
    {
        #region Properties

        public IEnumerable<string> Keys => _langSelections.Keys;

        #endregion

        #region Private Members

        private readonly Dictionary<string, string> _langSelections = new Dictionary<string, string>
        {
            { "zh-cn", "/GBCL;component/Resources/Languages/zh-cn.xaml" },
            //{ "zh-tw", "/GBCL;component/Resources/Languages/zh-tw.xaml" },
            { "en-us", "/GBCL;component/Resources/Languages/en-us.xaml" },
        };

        private string _currentTag;
        private ResourceDictionary _currentDict;

        #endregion

        #region Constructor

        public LanguageService()
        {
            _currentTag = "zh-cn"; // Default language: zh-cn
        }

        #endregion

        #region Public Methods

        public void Change(string langName)
        {
            if (_currentDict == null)
            {
                foreach (var dict in Application.Current.Resources.MergedDictionaries)
                {
                    if (dict.Source?.ToString() == _langSelections[langName])
                    {
                        _currentDict = dict;
                        break;
                    }
                }
            }

            if (langName == _currentTag || !_langSelections.ContainsKey(langName))
            {
                return;
            }

            Application.Current.Resources.MergedDictionaries.Remove(_currentDict);

            _currentTag = langName;
            _currentDict = Application.LoadComponent(new Uri(_langSelections[langName], UriKind.Relative)) as ResourceDictionary;
            Application.Current.Resources.MergedDictionaries.Add(_currentDict);
        }

        public string GetEntry(string key)
        {
            return !string.IsNullOrEmpty(key) ? _currentDict[key] as string : null;
        }

        public string ReplaceKeyToEntry(string src)
        {
            return Regex.Replace(src ?? string.Empty, "\\${.*?}", match =>
            {
                string key = match.Value.Substring(2, match.Length - 3);
                return this.GetEntry(key);
            });
        }

        #endregion
    }
}
