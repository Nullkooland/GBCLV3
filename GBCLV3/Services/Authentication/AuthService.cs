using GBCLV3.Models.Authentication;
using GBCLV3.Utils;
using StyletIoC;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace GBCLV3.Services.Authentication
{
    public class AuthService
    {
        #region Private Fields

        private const string MOJANG_AUTH_SERVER = "https://authserver.mojang.com";

        private readonly HttpClient _client;
        private readonly LogService _logService;

        private static readonly JsonSerializerOptions _jsonOptions
            = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        #endregion

        #region Constructor

        [Inject]
        public AuthService(HttpClient client, LogService logService)
        {
            _client = client;
            _logService = logService;
        }

        #endregion

        #region Public Methods

        public async ValueTask<AuthResult> LoginAsync(Account account)
        {
            var authResult = account.AuthMode switch
            {
                AuthMode.Offline => BuildOfflineResult(account.Username),
                AuthMode.Yggdrasil => await RefreshAsync(account.ClientToken, account.AccessToken),
                AuthMode.AuthLibInjector => await RefreshAsync(account.ClientToken, account.AccessToken,
                    account.AuthServer),
                _ => null,
            };

            // Refresh local tokens
            if (authResult.IsSuccessful)
            {
                account.Username = authResult.SelectedProfile?.Name;
                account.UUID = authResult.SelectedProfile?.Id;
                account.ClientToken = authResult.ClientToken;
                account.AccessToken = authResult.AccessToken;
            }

            return authResult;
        }

        public ValueTask<AuthResult> AuthenticateAsync(string email, string password, string authServer = null)
        {
            var request = new AuthRequest
            {
                Username = email,
                Password = password,
                ClientToken = Guid.NewGuid().ToString("N"),
                RequestUser = false,
            };

            string requestJson = JsonSerializer.Serialize(request, _jsonOptions);
            return RequestAsync(requestJson, false, authServer ?? MOJANG_AUTH_SERVER);
        }

        public ValueTask<AuthResult> RefreshAsync(string clientToken, string accessToken,
            string authServer = null, AuthUserProfile selectedProfile = null)
        {
            var request = new RefreshRequest
            {
                ClientToken = clientToken,
                AccessToken = accessToken,
                SelectedProfile = selectedProfile,
            };

            var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
            return RequestAsync(requestJson, true, authServer ?? MOJANG_AUTH_SERVER);
        }

        public async ValueTask<string> PrefetchAuthServerInfo(string authServer)
        {
            try
            {
                var responseJson = await _client.GetByteArrayAsync(authServer);
                return Convert.ToBase64String(responseJson);
            }
            catch (HttpRequestException ex)
            {
                _logService.Error(nameof(AuthService), $"Failed to prefetch external authserver info\n{ex.Message}");
                return null;
            }
        }

        public async ValueTask<AuthServerInfo> GetAuthServerInfo(string authServer)
        {
            try
            {
                var responseJson = await _client.GetByteArrayAsync(authServer);
                return JsonSerializer.Deserialize<AuthServerInfo>(responseJson, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logService.Error(nameof(AuthService), $"Failed to get external authserver info\n{ex.Message}");
                return null;
            }
        }

        public async ValueTask<bool> IsValidAuthServer(string authServer)
        {
            var info = await GetAuthServerInfo(authServer);
            return info?.Meta != null;
        }

        #endregion

        #region Helper Methods

        private static AuthResult BuildOfflineResult(string username)
        {
            return new AuthResult
            {
                SelectedProfile
                    = new AuthUserProfile { Name = username, Id = CryptoUtil.GetStringMD5(username) },

                ClientToken = Guid.NewGuid().ToString("N"),
                AccessToken = Guid.NewGuid().ToString("N"),
                UserType = "mojang",
                IsSuccessful = true,
            };
        }

        private async ValueTask<AuthResult> RequestAsync(string requestJson, bool isRefresh, string authServer)
        {
            var result = new AuthResult();

            try
            {
                using var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                using var msg = await _client.PostAsync(
                    authServer + (isRefresh ? "/refresh" : "/authenticate"), content);

                var responseJson = await msg.Content.ReadAsByteArrayAsync();

                if (msg.IsSuccessStatusCode)
                {
                    var response = JsonSerializer.Deserialize<AuthResponse>(responseJson, _jsonOptions);

                    result.IsSuccessful = true;
                    result.SelectedProfile = response.SelectedProfile;
                    result.AvailableProfiles = response.AvailableProfiles;
                    result.ClientToken = response.ClientToken;
                    result.AccessToken = response.AccessToken;
                    result.UserType = (response.SelectedProfile?.Legacy ?? false) ? "legacy" : "mojang";
                }
                else
                {
                    var error = JsonSerializer.Deserialize<AuthErrorResponse>(responseJson, _jsonOptions);

                    _logService.Warn(nameof(AuthService), $"Auth failed. Error: \"{error.Error}\" Message:\"{error.ErrorMessage}\"");


                    if (error.ErrorMessage?.ToLower().Contains("token") ?? false)
                    {
                        result.ErrorType = AuthErrorType.InvalidToken;
                    }
                    else if (error.ErrorMessage?.ToLower().Contains("credentials") ?? false)
                    {
                        result.ErrorType = AuthErrorType.InvalidCredentials;
                    }
                    else
                    {
                        result.ErrorType = AuthErrorType.Unknown;
                        result.ErrorMessage = error.ErrorMessage;
                    }

                    result.IsSuccessful = false;
                }
            }
            catch (HttpRequestException ex)
            {
                result.IsSuccessful = false;
                result.ErrorType = AuthErrorType.NoInternetConnection;
                result.ErrorMessage = ex.Message;

                _logService.Error(nameof(AuthService), $"Failed to send auth request: HTTP error\n{ex.Message}");
            }
            catch (OperationCanceledException)
            {
                result.IsSuccessful = false;
                result.ErrorType = AuthErrorType.AuthTimeout;

                _logService.Error(nameof(AuthService), "Failed to send auth request: Timeout");
            }
            catch (Exception ex)
            {
                result.IsSuccessful = false;
                result.ErrorType = AuthErrorType.Unknown;
                result.ErrorMessage = ex.Message;

                _logService.Error(nameof(AuthService), $"Failed to send auth request: Unkown error\n{ex.Message}");
            }

            return result;
        }

        #endregion
    }
}