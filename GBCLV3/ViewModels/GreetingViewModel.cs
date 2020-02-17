using Stylet;
using StyletIoC;
using GBCLV3.Models.Authentication;
using GBCLV3.Services.Authentication;

namespace GBCLV3.ViewModels
{
    public class GreetingViewModel : Screen
    {

        #region Constructor

        public GreetingViewModel(Account currentAccount)
        {
            CurrentAccount = currentAccount;
            IsReady = true;
        }

        #endregion

        #region Bindings

        public bool IsReady { get; set; }

        public Account CurrentAccount { get; private set; }


        public void OnAnimationCompleted()
        {
            if (this.IsActive) this.RequestClose();
        }

        #endregion
    }
}
