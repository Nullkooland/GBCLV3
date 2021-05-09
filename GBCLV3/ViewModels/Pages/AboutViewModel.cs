using System.Collections.Generic;
using GBCLV3.Services;
using GBCLV3.Utils;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels.Pages
{
    public class AboutViewModel : Screen
    {
        #region Private Fields

        // IoC
        private readonly IWindowManager _windowManager;

        #endregion

        #region Constructor

        [Inject]
        public AboutViewModel(IWindowManager windowManager, LanguageService languageService)
        {
            _windowManager = windowManager;

            Dependencies = new Dictionary<string, string>
            {
                { "Stylet MVVM Framework", "https://github.com/canton7/Stylet"},
                { "AdonisUI", "https://github.com/benruehl/adonis-ui"},
                { "Fody.PropertyChanged", "https://github.com/Fody/PropertyChanged"},
                { "OokiiDialogs", "https://github.com/ookii-dialogs/ookii-dialogs-wpf"},
            };

            Credits = new Dictionary<string, string>
            {
                { "BMCLAPI", "https://bmclapidoc.bangbang93.com"},
                { "MCBBS", "https://www.mcbbs.net"},
                { "Fabric", "https://fabricmc.net"},
                { "Forge", "http://files.minecraftforge.net/"},
            };

            Translators = languageService.GetTranslators();
        }

        #endregion

        #region Bindings

        public string VersionCode => AssemblyUtil.Version;
        public string Copyright => "MIT License, " + AssemblyUtil.Copyright;
        public string GBCLPage => "https://github.com/Goose-Bomb/GBCLV3";

        public IReadOnlyDictionary<string, string> Dependencies { get; }

        public IReadOnlyDictionary<string, string> Credits { get; }

        public IReadOnlyDictionary<string, string> Translators { get; }

        public void OpenLink(string url) => SystemUtil.OpenLink(url);

        public void DontStop() => _windowManager.ShowMessageBox("${DontStop}", "${FlowerOfHope}");

        #endregion

        #region Override Methods


        #endregion
    }
}
