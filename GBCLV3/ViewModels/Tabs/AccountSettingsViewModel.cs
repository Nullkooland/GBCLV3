using GBCLV3.Models.Authentication;
using GBCLV3.Services.Authentication;
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

        #endregion

        #region Constructor

        [Inject]
        public AccountSettingsViewModel(AccountService accountService)
        {
            _accountService = accountService;
            Accounts = new BindableCollection<Account>(_accountService.GetAll());
        }

        #endregion

        #region Bindings

        public BindableCollection<Account> Accounts { get; private set; }

        #endregion
    }
}
