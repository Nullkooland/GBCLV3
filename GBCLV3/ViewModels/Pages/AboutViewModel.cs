using GBCLV3.Utils;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels.Pages
{
    class AboutViewModel : Screen
    {
        #region Private Fields

        // IoC
        private readonly IWindowManager _windowManager;

        #endregion

        #region Constructor

        [Inject]
        public AboutViewModel(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        #endregion

        #region Bindings

        public string VersionCode => AssemblyUtil.Version;
        public string Copyright => "MIT License, " + AssemblyUtil.Copyright;
        public string GBCLV3Page => "https://github.com/Goose-Bomb/GBCLV3";

        public string Stylet => "Stylet MVVM Framework";
        public string StyletPage => "https://github.com/canton7/Stylet";

        public string FodyPropertyChanged => "Fody.PropertyChanged";
        public string FodyPropertyChangedPage => "https://github.com/Fody/PropertyChanged";

        public string AdonisUI => "Adonis UI Toolkit";
        public string AdonisUIPage => "https://github.com/benruehl/adonis-ui";

        public string OokiiDialogs => "Ookii Dialogs";
        public string OokiiDialogsPage => "http://www.ookii.org/software/dialogs";

        public string BMCLAPI => "BMCLAPI Download Mirror";
        public string BMCLAPIPage => "https://bmclapidoc.bangbang93.com";

        public string Forge => "Support Minecraft Forge";
        public string ForgePage => "https://www.patreon.com/LexManos";

        public void OpenLink(string url) => SystemUtil.OpenLink(url);

        public void DontStop() => _windowManager.ShowMessageBox("${DontStop}", "${FlowerOfHope}");

        #endregion

        #region Override Methods


        #endregion
    }
}
