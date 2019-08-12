using GBCLV3.Services;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels
{
    class GameInstallViewModel : Screen
    {
        #region Private Members

        private readonly LanguageService _languageService;

        #endregion

        #region Constructor

        [Inject]
        public GameInstallViewModel(LanguageService languageService)
        {
            _languageService = languageService;
        }

        #endregion

        #region Bindings

        #endregion

        #region Private Methods

        #endregion
    }
}
