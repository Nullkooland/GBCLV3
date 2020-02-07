using GBCLV3.Models.Authentication;
using GBCLV3.Services.Authentication;
using GBCLV3.ViewModels.Windows;
using Stylet;
using StyletIoC;
using System;
using System.Collections.Generic;
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

            _windowManager = windowManager;
            _addAccountVM = addAccountVM;
        }

        #endregion

        #region Bindings

        public BindableCollection<Account> Accounts { get; private set; }

        public void AddNew()
        {
            if (_windowManager.ShowDialog(_addAccountVM) ?? false)
            {
                
            }
        }

        #endregion
    }
}
