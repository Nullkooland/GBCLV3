using GBCLV3.Services;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels
{
    class VersionManagementViewModel : Screen
    {
        #region Private Members

        private readonly LanguageService _languageService;

        #endregion

        #region Constructor

        [Inject]
        public VersionManagementViewModel(LanguageService languageService)
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
