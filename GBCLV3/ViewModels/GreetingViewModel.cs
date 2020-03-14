using Stylet;
using StyletIoC;
using GBCLV3.Models.Authentication;
using GBCLV3.Services.Authentication;
using GBCLV3.ViewModels.Tabs;

namespace GBCLV3.ViewModels
{
    public class GreetingViewModel : Screen
    {
        #region Private Fields

        private readonly AccountService _accountService;

        #endregion

        #region Constructor

        [Inject]
        public GreetingViewModel(AccountService accountService)
        {
            _accountService = accountService;
        }

        #endregion

        #region Bindings

        public bool IsShown { get; set; }

        public Account SelectedAccount { get; private set; }

        public bool IsOfflineMode => SelectedAccount?.AuthMode == AuthMode.Offline;

        #endregion

        #region Public Method

        public void NotifyAccountChanged()
        {
            SelectedAccount = _accountService.GetSelected();
            NotifyOfPropertyChange(nameof(SelectedAccount));
        }

        #endregion
    }
}
