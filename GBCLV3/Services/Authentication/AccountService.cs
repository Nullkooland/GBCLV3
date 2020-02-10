using GBCLV3.Models;
using GBCLV3.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using StyletIoC;
using GBCLV3.Services.Auxiliary;
using System.Threading.Tasks;

namespace GBCLV3.Services.Authentication
{
    class AccountService
    {
        #region Events

        public event Action<Account> Created;

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

        public async Task LoadSkinsAsync()
        {
            var task = _accounts.Select(async account =>
                await LoadSkinAsync(account).ConfigureAwait(false));

            await Task.WhenAll(task);
        }

        public bool Any() => _accounts.Any();

        public IEnumerable<Account> GetAll() => _accounts;

        public bool HasOfflineAccount(string username) =>
            _accounts.Where(account => account.AuthMode == AuthMode.Offline)
                .Any(account => account.Username == username);

        public bool HasOnlineAccount(AuthMode authMode, string email) =>
            _accounts.Where(account => account.AuthMode == authMode)
                .Any(account => account.Email == email);

        public Account GetSelected() => _accounts.Find(account => account.IsSelected);

        public Account AddOfflineAccount(string username)
        {
            var account = new Account
            {
                AuthMode = AuthMode.Offline,
                Username = username,
            };

            _accounts.Add(account);
            Created?.Invoke(account);
            return account;
        }

        public async ValueTask<Account> AddOnlineAccountAsync(string email, AuthMode authMode, AuthResult authResult,
            string authServer = null)
        {
            var account = new Account();
            await UpdateOnlineAccountAsync(account, authMode, email, authResult, authServer);

            _accounts.Add(account);
            Created?.Invoke(account);
            return account;
        }

        public async Task UpdateOnlineAccountAsync(Account account, AuthMode authMode, string email,
            AuthResult authResult,
            string authServer = null)
        {
            account.AuthMode = authMode;
            account.Email = email;
            account.Username = authResult.SelectedProfile.Name;
            account.UUID = authResult.SelectedProfile.Id;
            account.ClientToken = authResult.ClientToken;
            account.AccessToken = authResult.AccessToken;
            account.AuthServerBase = authMode == AuthMode.AuthLibInjector ? authServer : null;

            await LoadSkinAsync(account);
        }

        public void Delete(Account account)
        {
            _accounts.Remove(account);
        }

        #endregion

        #region Private Methods

        private async Task LoadSkinAsync(Account account)
        {
            if (account.AuthMode != AuthMode.Offline)
            {
                account.Skin = await _skinService.GetSkinAsync(account.Profile);
                // Refresh latest profile and skin later
                RefreshSkinAsync(account).ConfigureAwait(false);
            }
        }

        private async Task RefreshSkinAsync(Account account)
        {
            var latestProfile = await _skinService.GetProfileAsync(account.UUID, account.ProfileServer);
            account.Profile = latestProfile ?? account.Profile;
            account.Skin = await _skinService.GetSkinAsync(account.Profile);
        }

        #endregion
    }
}