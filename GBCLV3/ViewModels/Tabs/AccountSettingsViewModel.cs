using GBCLV3.Models.Authentication;
using GBCLV3.Services.Authentication;
using GBCLV3.ViewModels.Windows;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GBCLV3.ViewModels.Tabs
{
    class AccountSettingsViewModel : Screen
    {
        #region Private Fields

        // IoC
        private readonly AccountService _accountService;
        private readonly IWindowManager _windowManager;
        private readonly AddAccountViewModel _addAccountVM;

        #endregion

        #region Constructor

        [Inject]
        public AccountSettingsViewModel(
            AccountService accountService, 
            IWindowManager windowManager,
            AddAccountViewModel addAccountVM)
        {
            _accountService = accountService;
            Accounts = new BindableCollection<Account>(_accountService.GetAll());

            _accountService.Created += account =>
            {
                if (!Accounts.Any()) account.IsSelected = true;
                Accounts.Add(account);
            };

            _windowManager = windowManager;
            _addAccountVM = addAccountVM;
        }

        #endregion

        #region Bindings

        public BindableCollection<Account> Accounts { get; private set; }

        public Account SelectedAccount { get; set; }

        public void AddNew()
        {
            if (_windowManager.ShowDialog(_addAccountVM) ?? false)
            {
                
            }
        }

        public void Delete(Account account)
        {
            _accountService.Delete(account);
            Accounts.Remove(account);
            SelectedAccount ??= Accounts.FirstOrDefault();
        }

        #endregion
    }
}
