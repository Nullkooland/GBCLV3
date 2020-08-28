using GBCLV3.Models.Authentication;
using GBCLV3.Services.Authentication;
using GBCLV3.ViewModels.Windows;
using Stylet;
using StyletIoC;
using System.Linq;
using System.Windows;

namespace GBCLV3.ViewModels.Tabs
{
    public class AccountSettingsViewModel : Screen
    {
        #region Private Fields

        // IoC
        private readonly AccountService _accountService;
        private readonly IWindowManager _windowManager;
        private readonly AccountEditViewModel _accountEditVM;
        private readonly GreetingViewModel _greetingVM;

        #endregion

        #region Constructor

        [Inject]
        public AccountSettingsViewModel(
            AccountService accountService, 
            IWindowManager windowManager,
            AccountEditViewModel accountEditVM,
            GreetingViewModel greetingVM)
        {
            _windowManager = windowManager;
            _accountEditVM = accountEditVM;
            _greetingVM = greetingVM;

            _accountService = accountService;
            Accounts = new BindableCollection<Account>(_accountService.GetAll());
            SelectedAccount = _accountService.GetSelected();

            _accountService.Created += account => Accounts.Add(account);
        }

        #endregion

        #region Bindings

        public BindableCollection<Account> Accounts { get; }

        public Account SelectedAccount { get; set; }

        public void AddNew()
        {
            _accountEditVM.Setup(AccountEditType.AddAccount);
            _windowManager.ShowDialog(_accountEditVM);

            _greetingVM.NotifyAccountChanged();
            _greetingVM.IsShown = true;
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

            if (!_accountService.Any())
            {
                _greetingVM.IsShown = false;
            }
        }

        public void Edit(Account account)
        {
            _accountEditVM.Setup(AccountEditType.EditAccount, account);
            _windowManager.ShowDialog(_accountEditVM);
            Accounts.Refresh();
            _greetingVM.NotifyAccountChanged();
        }

        public void OnSelectedAccountChanged() => _greetingVM.NotifyAccountChanged();
        

        #endregion
    }
}
