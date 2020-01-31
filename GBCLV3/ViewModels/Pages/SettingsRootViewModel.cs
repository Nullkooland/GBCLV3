using GBCLV3.ViewModels.Tabs;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels.Pages
{
    class SettingsRootViewModel : Screen
    {

        #region Constructor

        [Inject]
        public SettingsRootViewModel(
            GameSettingsViewModel gameSettingsVM,
            AccountSettingsViewModel accountSettingsVM,
            LauncherSettingsViewModel launcherSettingsVM)
        {
            GameSettingsVM = gameSettingsVM;
            AccountSettingsVM = accountSettingsVM;
            LauncherSettingsVM = launcherSettingsVM;
        }

        #endregion

        #region Bindings

        public Screen GameSettingsVM { get; private set; }

        public Screen AccountSettingsVM { get; private set; }

        public Screen LauncherSettingsVM { get; private set; }


        #endregion

        #region Private Methods

        #endregion
    }
}
