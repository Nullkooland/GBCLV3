using GBCLV3.Models;
using GBCLV3.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using StyletIoC;
using GBCLV3.Services.Auxiliary;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace GBCLV3.Services.Authentication
{
    class AccountService
    {
        #region Events

        public event Action<Account> SelectedAccountChanged;

        #endregion

        #region Private Fields

        // IoC
        private readonly Config _config;
        private readonly SkinService _skinService;

        private readonly List<Account> _accounts;

        #endregion

        #region Constructor

        [Inject]
        public AccountService(ConfigService configService, SkinService skinService)
        {
            _config = configService.Entries;
            _skinService = skinService;
            _accounts = _config.Accounts;
        }

        #endregion

        #region Public Methods

        public bool Load()
        {
            foreach (var account in _accounts)
            {
                LoadSkinAsync(account).ConfigureAwait(false);
            }

            return _accounts.Any();
        }

        public IEnumerable<Account> GetAll() => _accounts;

        public Account GetSelected() => _accounts.Find(account => account.IsSelected);

        public Account AddOfflineAccount(string username)
        {
            var account = new Account
            {
                AuthMode = AuthMode.Offline,
                Username = username,
            };

            _accounts.Add(account);
            return account;
        }

        public async ValueTask<Account> AddOnlineAccount(string email, AuthResult authResult, AuthMode mode, string authServer = null)
        {
            var account = new Account
            {
                AuthMode = mode,
                Email = email,
                Username = authResult.Username,
                ClientToken = authResult.ClientToken,
                AccessToken = authResult.AccessToken,
                UUID = authResult.UUID,
                AuthServer = mode == AuthMode.AuthLibInjector ? authServer : null,
            };

            await LoadSkinAsync(account);

            _accounts.Add(account);
            return account;
        }

        #endregion

        #region Private Methods

        private async Task LoadSkinAsync(Account account)
        {
            if (account.AuthMode != AuthMode.Offline)
            {
                string profileServer = (account.AuthMode == AuthMode.AuthLibInjector) ?
                    account.AuthServer + "/sessionserver/session/minecraft/profile/" : null;

                account.SkinProfile ??= await _skinService.GetProfileAsync(account.UUID, profileServer);
                account.Skin = await _skinService.GetSkinAsync(account.SkinProfile);
            }
        }

        #endregion
    }
}
