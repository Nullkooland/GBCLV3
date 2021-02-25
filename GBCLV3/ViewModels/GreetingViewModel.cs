using Stylet;
using StyletIoC;
using GBCLV3.Models.Authentication;
using GBCLV3.Services.Authentication;
using GBCLV3.ViewModels.Windows;

namespace GBCLV3.ViewModels
{
    public class GreetingViewModel : Screen
    {
        #region Private Fields

        private readonly AccountService _accountService;
        private readonly AccountEditViewModel _accountEditVM;
        private readonly IWindowManager _windowManager;

        #endregion

        #region Constructor

        [Inject]
        public GreetingViewModel(
            AccountService accountService,
            AccountEditViewModel accountEditVM,
            IWindowManager windowManager)
        {
            _accountService = accountService;
            _accountEditVM = accountEditVM;
            _windowManager = windowManager;

        }

        #endregion

        #region Bindings

        public bool IsShown { get; set; }

        public Account CurrentAccount { get; private set; }

        public bool IsOfflineMode => CurrentAccount?.AuthMode == AuthMode.Offline;

        #endregion

        #region Public Method

        public void NotifyAccountChanged()
        {
            CurrentAccount = _accountService.GetSelected();
            NotifyOfPropertyChange(nameof(CurrentAccount));
        }

        public void EditCurrentAccount()
        {
            _accountEditVM.Setup(AccountEditType.EditAccount, CurrentAccount);
            _windowManager.ShowDialog(_accountEditVM);
            NotifyOfPropertyChange(nameof(CurrentAccount));
            NotifyOfPropertyChange(nameof(IsOfflineMode));
        }

        #endregion
    }
}
