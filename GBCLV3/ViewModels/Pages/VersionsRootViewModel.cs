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

        #endregion

        #region Constructor

        [Inject]
        public VersionsRootViewModel(
            VersionsManagementViewModel versionsManagementVM,
            GameInstallViewModel gameInstallVM,
            ForgeInstallViewModel forgeInstallVM)
        {
            _versionsManagementVM = versionsManagementVM;
            _gameInstallVM = gameInstallVM;
            _forgeInstallVM = forgeInstallVM;

            this.ActivateItem(_versionsManagementVM);
            _versionsManagementVM.NavigateView += OnNavigateView;
        }

        #endregion

        #region Public Methods

        public void OnNavigateView(string versionID)
        {
            if (versionID == null) this.ActivateItem(_gameInstallVM);
            else
            {
                _forgeInstallVM.GameVersion = versionID;
                this.ActivateItem(_forgeInstallVM);
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
