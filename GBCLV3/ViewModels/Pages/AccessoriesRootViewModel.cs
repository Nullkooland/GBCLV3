using System.Threading.Tasks;
using GBCLV3.Models;
using Stylet;
using StyletIoC;

namespace GBCLV3.ViewModels
{
    class AccessoriesViewModel : Screen
    {
        #region Constructor

        [Inject]
        public AccessoriesViewModel(
            ModViewModel modVM,
            ResourcePackViewModel resourcePackVM)
        {
            ModVM = modVM;
            ResourcePackVM = resourcePackVM;
        }

        #endregion

        #region Bindings

        public ModViewModel ModVM { get; set; }

        public ResourcePackViewModel ResourcePackVM { get; set; }

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
