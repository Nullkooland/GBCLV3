using GBCLV3.ViewModels.Tabs;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels
{
    public class AuxiliariesRootViewModel : Screen
    {
        #region Constructor

        [Inject]
        public AuxiliariesRootViewModel(
            ModViewModel modVM,
            ResourcePackViewModel resourcePackVM,
            SkinViewModel skinVM)
        {
            ModVM = modVM;
            ResourcePackVM = resourcePackVM;
            SkinVM = skinVM;
        }

        #endregion

        #region Bindings

        public ModViewModel ModVM { get; set; }

        public ResourcePackViewModel ResourcePackVM { get; set; }

        public SkinViewModel SkinVM { get; set; }

        #endregion

        #region Override Methods

        protected override void OnActivate()
        {
            ModVM.Reload();
            ResourcePackVM.Reload();
        }

        protected override void OnDeactivate() => ResourcePackVM.SaveToOptions();

        #endregion
    }
}
