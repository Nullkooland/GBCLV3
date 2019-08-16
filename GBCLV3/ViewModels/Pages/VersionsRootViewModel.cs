using GBCLV3.Views;
using Stylet;

namespace GBCLV3.ViewModels.Pages
{
    class VersionsRootViewModel : Screen
    {
        #region Constructor

        public VersionsRootViewModel(
            VersionManagementViewModel versionManagementVM,
            GameInstallViewModel gameInstallVM,
            ForgeInstallViewModel forgeInstallVM)
        {
            VersionManagementVM = versionManagementVM;
            GameInstallVM = gameInstallVM;
            ForgeInstallVM = forgeInstallVM;
        }

        #endregion

        #region Bindings

        public Screen VersionManagementVM { get; private set; }

        public Screen GameInstallVM { get; private set; }

        public Screen ForgeInstallVM { get; private set; }

        #endregion
    }
}
