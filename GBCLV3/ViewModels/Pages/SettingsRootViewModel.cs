using Stylet;

namespace GBCLV3.ViewModels.Pages
{
    class SettingsRootViewModel : Screen
    {
        #region Private Members

        #endregion

        #region Constructor

        public SettingsRootViewModel(
            GameSettingsViewModel gameSettingsVM,
            LauncherSettingsViewModel launcherSettingsVM)
        {
            GameSettingsVM = gameSettingsVM;
            LauncherSettingsVM = launcherSettingsVM;
        }

        #endregion

        #region Bindings

        public Screen GameSettingsVM { get; private set; }

        public Screen LauncherSettingsVM { get; private set; }

        #endregion

        #region Private Methods

        #endregion
    }
}
