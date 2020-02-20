using GBCLV3.ViewModels.Tabs;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels.Pages
{
    public class SettingsRootViewModel : Screen
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

        public Screen GameSettingsVM { get; }

        public Screen AccountSettingsVM { get; }

        public Screen LauncherSettingsVM { get; }


        #endregion

        #region Private Methods

        #endregion
    }
}
