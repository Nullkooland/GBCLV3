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
        private readonly AccountEditViewModel _accountEditVM;

        #endregion

        #region Constructor

        [Inject]
        public AccountSettingsViewModel(
            AccountService accountService, 
            IWindowManager windowManager,
            AccountEditViewModel addAccountVM)
        {
            _accountService = accountService;
            Accounts = new BindableCollection<Account>(_accountService.GetAll());

            _accountService.Created += account =>
            {
                if (!Accounts.Any()) account.IsSelected = true;
                Accounts.Add(account);
            };

            _windowManager = windowManager;
            _accountEditVM = addAccountVM;
        }

        #endregion

        #region Bindings

        public BindableCollection<Account> Accounts { get; private set; }

        public Account SelectedAccount { get; set; }

        public void AddNew()
        {
            _accountEditVM.Setup(EditAccountType.AddAccount);
            if (_windowManager.ShowDialog(_accountEditVM) ?? false)
            {
                
            }
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
            _accountEditVM.Setup(EditAccountType.EditAccount, account);
            _windowManager.ShowDialog(_accountEditVM);
        }

        #endregion
    }
}
