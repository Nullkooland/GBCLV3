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
            ShaderPackViewModel shaderPackVM)
        {
            ModVM = modVM;
            ResourcePackVM = resourcePackVM;
            ShaderPackVM = shaderPackVM;
        }

        #endregion

        #region Bindings

        public ModViewModel ModVM { get; set; }

        public ResourcePackViewModel ResourcePackVM { get; set; }

        public ShaderPackViewModel ShaderPackVM { get; set; }

        #endregion

        #region Override Methods

        protected override void OnActivate()
        {
            ModVM.Reload();
            ResourcePackVM.Reload();
            ShaderPackVM.Reload();
        }

        protected override void OnDeactivate()
        {
            ResourcePackVM.SaveToOptions();
            ShaderPackVM.SaveToOptions();
        }

        #endregion
    }
}
