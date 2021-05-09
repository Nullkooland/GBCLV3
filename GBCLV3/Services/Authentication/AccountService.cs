using GBCLV3.Models;
using GBCLV3.Models.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using StyletIoC;
using GBCLV3.Services.Auxiliary;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

namespace GBCLV3.Services.Authentication
{
    public class AccountService
    {
        #region Events

        public event Action<Account> Created;

        #endregion

        #region Private Fields

        private const string MOJANG_PROFILE_SERVER = "https://sessionserver.mojang.com/session/minecraft/profile";

        // IoC
        private readonly Config _config;
        private readonly SkinService _skinService;
        private readonly LogService _logService;
        private readonly HttpClient _client;

        private readonly List<Account> _accounts;

        #endregion

        #region Constructor

        [Inject]
        public AccountService(
            ConfigService configService,
            SkinService skinService,
            LogService logService,
            HttpClient client)
        {
            _config = configService.Entries;
            _skinService = skinService;
            _logService = logService;
            _client = client;

            _accounts = _config.Accounts;
        }

        #endregion

        #region Public Methods

        public Task LoadSkinsForAllAsync()
        {
            var tasks = _accounts.Select(async account =>
                await LoadSkinAsync(account).ConfigureAwait(false));

            return Task.WhenAll(tasks);
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

            if (!_accounts.Any()) account.IsSelected = true;

            _accounts.Add(account);
            Created?.Invoke(account);

            _logService.Info(nameof(AccountService), $"Offline account \"{account.Username}\" added");

            return account;
        }

        public async ValueTask<Account> AddOnlineAccountAsync(string email, AuthMode authMode, AuthResult authResult,
            string authServer = null)
        {
            var account = new Account();
            await UpdateAccountAsync(account, null, authMode, email, authResult, authServer);

            if (!_accounts.Any()) account.IsSelected = true;

            _accounts.Add(account);
            Created?.Invoke(account);

            _logService.Info(nameof(AccountService), $"{account.AuthMode} account \"{account.Username}\" added");

            return account;
        }

        public async ValueTask UpdateAccountAsync(
            Account account,
            string username = null,
            AuthMode authMode = AuthMode.Offline,
            string email = null,
            AuthResult authResult = null,
            string authServer = null)
        {
            account.AuthMode = authMode;
            account.Email = email;
            account.Username = authResult?.SelectedProfile.Name ?? username;
            account.UUID = authResult?.SelectedProfile.Id;
            account.ClientToken = authResult?.ClientToken;
            account.AccessToken = authResult?.AccessToken;
            account.AuthServerBase = (authMode == AuthMode.AuthLibInjector) ? authServer : null;

            _logService.Info(nameof(AccountService), $"{account.AuthMode} account \"{account.Username}\" updated");

            await LoadSkinAsync(account);
        }

        public void Delete(Account account)
        {
            _accounts.Remove(account);
            _logService.Info(nameof(AccountService), $"{account.AuthMode} account \"{account.Username}\" deleted");
        }

        public async ValueTask<string> GetProfileAsync(string uuid, string profileServer = null)
        {
            string profileUrl = $"{profileServer ?? MOJANG_PROFILE_SERVER}/{uuid}";

            _logService.Info(nameof(AccountService), $"Fetching account profile from \"{profileUrl}\"");

            try
            {
                var profileJson = await _client.GetByteArrayAsync(profileUrl);
                using var profile = JsonDocument.Parse(profileJson);

                return profile.RootElement
                    .GetProperty("properties")[0]
                    .GetProperty("value")
                    .GetString();
            }
            catch (HttpRequestException ex)
            {
                _logService.Error(nameof(AccountService), $"Failed to fetch account profile: HTTP error\n{ex.Message}");
            }
            catch (OperationCanceledException)
            {
                _logService.Error(nameof(AccountService), $"Failed to fetch account profile: Timeout");
            }
            catch (Exception ex)
            {
                _logService.Error(nameof(AccountService), $"Failed to fetch account profile: Unkown error\n{ex.Message}");
            }

            return null;
        }

        public async ValueTask LoadSkinAsync(Account account)
        {
            if (account.AuthMode == AuthMode.Offline) return;

            _logService.Info(nameof(AccountService), $"Loading skin for {account.AuthMode} account \"{account.Username}\"");

            account.Skin = await _skinService.GetAsync(account.Profile);
            if (account.Profile == null)
            {
                await RefreshSkinAsync(account);
            }
            else
            {
                RefreshSkinAsync(account).ConfigureAwait(false);
            }
        }

        public async ValueTask RefreshSkinAsync(Account account)
        {
            var latestProfile = await GetProfileAsync(account.UUID, account.ProfileServer);
            account.Profile = latestProfile ?? account.Profile;
            account.Skin = await _skinService.GetAsync(account.Profile);
        }

        #endregion
    }
}