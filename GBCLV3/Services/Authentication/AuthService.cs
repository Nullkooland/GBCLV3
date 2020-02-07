using GBCLV3.Models;
using GBCLV3.Models.Authentication;
using GBCLV3.Utils;
using StyletIoC;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GBCLV3.Services.Authentication
{
    class AuthService
    {
        #region Events

        public event Action<string> UsernameChanged;

        #endregion

        #region Private Fields

        private const string MOJANG_AUTH_SERVER = "https://authserver.mojang.com";

        private static readonly HttpClient _client = new HttpClient() { Timeout = TimeSpan.FromSeconds(15) };

        private static readonly JsonSerializerOptions _jsonOptions
            = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        #endregion

        #region Public Methods

        public async ValueTask<AuthResult> LoginAsync(Account account)
        {
            var authResult = account.AuthMode switch
            {
                AuthMode.Offline => BuildOfflineResult(account.Username),
                AuthMode.Yggdrasil => await RefreshAsync(account.ClientToken, account.AccessToken),
                AuthMode.AuthLibInjector => await RefreshAsync(account.ClientToken, account.AccessToken, account.AuthServer),
                _ => null,
            };

            // Refresh local tokens
            if (authResult.IsSuccessful)
            {
                account.Username = authResult.Username;
                account.ClientToken = authResult.ClientToken;
                account.AccessToken = authResult.AccessToken;
                account.UUID = authResult.UUID;
                UsernameChanged?.Invoke(authResult.Username);
            }

            return authResult;
        }

        public async ValueTask<AuthResult> AuthenticateAsync(string email, string password, string authServer = null)
        {
            var request = new AuthRequest
            {
                Username = email,
                Password = password,
                ClientToken = CryptUtil.Guid,
                RequestUser = false,
            };

            string requestJson = JsonSerializer.Serialize(request, _jsonOptions);
            return await RequestAsync(requestJson, false, authServer ?? MOJANG_AUTH_SERVER);
        }

        public async ValueTask<AuthResult> RefreshAsync(string clientToken, string accessToken, string authServer = null)
        {
            var request = new RefreshRequest
            {
                ClientToken = clientToken,
                AccessToken = accessToken,
            };

            var requestJson = JsonSerializer.Serialize(request, _jsonOptions);
            return await RequestAsync(requestJson, true, authServer ?? MOJANG_AUTH_SERVER);
        }

        public async ValueTask<AuthServerInfo> GetAuthServerInfo(string authServer)
        {
            try
            {
                var responseJson = await _client.GetStringAsync(authServer);
                return JsonSerializer.Deserialize<AuthServerInfo>(responseJson, _jsonOptions);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return null;
            }
        }

        public async ValueTask<bool> IsAuthServerValid(string authServer)
        {
            var info = await GetAuthServerInfo(authServer);
            return info?.Meta != null;
        }

        #endregion

        #region Helper Methods

        private static AuthResult BuildOfflineResult(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new AuthResult
                {
                    IsSuccessful = false,
                    ErrorType = AuthErrorType.EmptyUsername,
                    ErrorMessage = "${EmptyUsernameError}",
                };
            }

            return new AuthResult
            {
                Username = username,
                UUID = CryptUtil.GetStringMD5(username),
                ClientToken = CryptUtil.Guid,
                AccessToken = CryptUtil.Guid,
                UserType = "mojang",
                IsSuccessful = true,
            };
        }

        private static async ValueTask<AuthResult> RequestAsync(string requestJson, bool isRefresh, string authServer)
        {
            var result = new AuthResult();

            try
            {
                var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var uri = new Uri(authServer + (isRefresh ? "/refresh" : "/authenticate"));
                var msg = await _client.PostAsync(uri, content);
                string responseJson = await msg.Content.ReadAsStringAsync();

                content.Dispose();
                msg.Dispose();

                if (msg.IsSuccessStatusCode)
                {
                    var response = JsonSerializer.Deserialize<AuthResponse>(responseJson, _jsonOptions);

                    result.IsSuccessful = true;
                    result.Username = response.SelectedProfile.Name;
                    result.UUID = response.SelectedProfile.Id;
                    result.AvailableProfiles = response.AvailableProfiles;
                    result.ClientToken = response.ClientToken;
                    result.AccessToken = response.AccessToken;
                    result.UserType = response.SelectedProfile.Legacy ? "legacy" : "mojang";
                }
                else
                {
                    var error = JsonSerializer.Deserialize<AuthErrorResponse>(responseJson, _jsonOptions);
                    if (error.ErrorMessage.ToLower().Contains("token"))
                    {
                        result.ErrorType = AuthErrorType.InvalidToken;
                        result.ErrorMessage = "${InvalidTokenError}";
                    }
                    else if (error.ErrorMessage.ToLower().Contains("credentials"))
                    {
                        result.ErrorType = AuthErrorType.InvalidCredentials;
                        result.ErrorMessage = "${InvalidCredentialsError}";
                    }
                    else
                    {
                        result.ErrorType = AuthErrorType.Unknown;
                        result.ErrorMessage = "${UnknownError}" + '\n' + "${ErrorMessage}" + error.ErrorMessage;
                    }

                    result.IsSuccessful = false;
                }

            }
            catch (HttpRequestException ex)
            {
                result.IsSuccessful = false;
                result.ErrorType = AuthErrorType.NoInternetConnection;
                result.ErrorMessage = ex.Message + '\n' + "${NoInternetConnectionError}";
            }
            catch (OperationCanceledException)
            {
                result.IsSuccessful = false;
                result.ErrorType = AuthErrorType.Timeout;
                result.ErrorMessage = "${AuthTimeoutError}";
            }

            return result;
        }

        #endregion
    }
}
