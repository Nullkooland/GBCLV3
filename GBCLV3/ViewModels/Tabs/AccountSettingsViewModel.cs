using GBCLV3.Models.Authentication;
using GBCLV3.Services.Authentication;
using GBCLV3.ViewModels.Windows;
using Stylet;
using StyletIoC;
using System.Linq;
using System.Windows;

namespace GBCLV3.ViewModels.Tabs
{
    class AccountSettingsViewModel : Screen
    {
        #region Private Fields

        // IoC
        private readonly AccountService _accountService;
        private readonly IWindowManager _windowManager;
        private readonly AccountEditViewModel _accountEditEditVm;

        #endregion

        #region Constructor

        [Inject]
        public AccountSettingsViewModel(
            AccountService accountService, 
            IWindowManager windowManager,
            AccountEditViewModel accountEditVM)
        {
            _accountService = accountService;
            Accounts = new BindableCollection<Account>(_accountService.GetAll());
            SelectedAccount = _accountService.GetSelected();

            _accountService.Created += account => Accounts.Add(account);

            _windowManager = windowManager;
            _accountEditEditVm = accountEditVM;
        }

        #endregion

        #region Bindings

        public BindableCollection<Account> Accounts { get; private set; }

        public Account SelectedAccount { get; set; }

        public void AddNew()
        {
            _accountEditEditVm.Setup(AccountEditType.AddAccount);
            _windowManager.ShowDialog(_accountEditEditVm);
        }
        public void Delete(Account account)
        {
            if (_windowManager.ShowMessageBox("${WhetherDeleteAccount}", "${DeleteAccount}",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                _accountService.Delete(account);
                Accounts.Remove(account);
                SelectedAccount ??= Accounts.FirstOrDefault();
            }
        }

        public void Edit(Account account)
        {
            _accountEditEditVm.Setup(AccountEditType.EditAccount, account);
            _windowManager.ShowDialog(_accountEditEditVm);
            Accounts.Refresh();
        }

        #endregion
    }
}
