using GBCLV3.Models.Installation;
using GBCLV3.ViewModels.Tabs;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels.Pages
{
    class VersionsRootViewModel : Conductor<IScreen>.StackNavigation
    {
        #region Private Members

        //IoC
        private readonly VersionsManagementViewModel _versionsManagementVM;
        private readonly GameInstallViewModel _gameInstallVM;
        private readonly ForgeInstallViewModel _forgeInstallVM;
        private readonly FabricInstallViewModel _fabricInstallVM;

        #endregion

        #region Constructor

        [Inject]
        public VersionsRootViewModel(
            VersionsManagementViewModel versionsManagementVM,
            GameInstallViewModel gameInstallVM,
            ForgeInstallViewModel forgeInstallVM,
            FabricInstallViewModel fabricInstallVM)
        {
            _versionsManagementVM = versionsManagementVM;
            _gameInstallVM = gameInstallVM;
            _forgeInstallVM = forgeInstallVM;
            _fabricInstallVM = fabricInstallVM;

            this.ActivateItem(_versionsManagementVM);
            _versionsManagementVM.NavigateInstallView += OnNavigateInstallView;
        }

        #endregion

        #region Public Methods

        public void OnNavigateInstallView(string versionID, InstallType type)
        {
            switch (type)
            {
                case InstallType.Version:
                    this.ActivateItem(_gameInstallVM);
                    return;

                case InstallType.Forge:
                    _forgeInstallVM.GameVersion = versionID;
                    this.ActivateItem(_forgeInstallVM);
                    return;

                case InstallType.Fabric:
                    _fabricInstallVM.GameVersion = versionID;
                    this.ActivateItem(_fabricInstallVM);
                    return;

                default: return;
            }
        }

        #endregion

        #region Override Methods

        protected override void OnActivate()
        {
            if (this.ActiveItem is GameInstallViewModel gameInstallVM && !gameInstallVM.IsLoading)
            {
                gameInstallVM.GoBack();
            }

            if (this.ActiveItem is ForgeInstallViewModel forgeInstallVM && !forgeInstallVM.IsLoading)
            {
                forgeInstallVM.GoBack();
            }
        }

        #endregion
    }
}
