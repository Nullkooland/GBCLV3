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
            bool isDuplicate = _accounts.Where(account => account.AuthMode == AuthMode.Offline)
                                        .Any(account => account.Username == username);

            if (isDuplicate) return null;

            var account = new Account
            {
                AuthMode = AuthMode.Offline,
                Username = username,
            };

            _accounts.Add(account);
            Created?.Invoke(account);
            return account;
        }

        public async ValueTask<Account> AddOnlineAccount(string email,
                                                         AuthResult authResult,
                                                         AuthMode authMode,
                                                         string authServer = null)
        {

            bool isDuplicate = _accounts.Where(account => account.AuthMode == authMode)
                                        .Any(account => account.Email == email);

            if (isDuplicate) return null;

            var account = new Account
            {
                AuthMode = authMode,
                Email = email,
                Username = authResult.Username,
                ClientToken = authResult.ClientToken,
                AccessToken = authResult.AccessToken,
                UUID = authResult.UUID,
                AuthServerBase = authMode == AuthMode.AuthLibInjector ? authServer : null,
            };

            await LoadSkinAsync(account);

            _accounts.Add(account);
            Created?.Invoke(account);
            return account;
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
                account.SkinProfile ??= await _skinService.GetProfileAsync(account.UUID, account.ProfileServer);
                account.Skin = await _skinService.GetSkinAsync(account.SkinProfile);
            }
        }

        #endregion
    }
}
