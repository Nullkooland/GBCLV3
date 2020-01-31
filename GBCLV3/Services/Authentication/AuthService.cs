using GBCLV3.Models;
using GBCLV3.Models.Authentication;
using GBCLV3.Utils;
using StyletIoC;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GBCLV3.Services.Authentication
{
    class AuthService
    {
        #region Private Members

        private const string MOJANG_AUTH_SERVER = "https://authserver.mojang.com/";

        private static readonly HttpClient _client = new HttpClient() { Timeout = TimeSpan.FromSeconds(15) };

        private static readonly JsonSerializerOptions _jsonOptions
            = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private readonly Config _config;

        #endregion

        #region Constructor

        [Inject]
        public AuthService(ConfigService configService)
        {
            _config = configService.Entries;
        }

        #endregion

        #region Public Methods

        public async ValueTask<AuthResult> LoginAsync()
        {
            return _config.AuthMode switch
            {
                AuthMode.Offline => BuildOfflineResult(_config.Username),
                AuthMode.Yggdrasil => await RefreshAsync(_config.ClientToken, _config.AccessToken, MOJANG_AUTH_SERVER),
                AuthMode.AuthLibInjector => await RefreshAsync(_config.ClientToken, _config.AccessToken, _config.AuthServer),
                _ => null,
            };
        }

        #endregion

        #region Helper Method

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

        private static async ValueTask<AuthResult> LoginAsync(string email, string password, string authServer)
        {
            if (!IsValidEmailAddress(email))
            {
                return new AuthResult
                {
                    IsSuccessful = false,
                    ErrorType = AuthErrorType.InvalidEmail,
                    ErrorMessage = "${InvalidEmailError}",
                };
            }

            var request = new AuthRequest
            {
                Username = email,
                Password = password,
                ClientToken = CryptUtil.Guid,
                RequestUser = false,
            };

            return await AuthenticateAsync(JsonSerializer.Serialize(request, _jsonOptions), false, authServer);
        }

        internal static object GetOfflineProfile(string v)
        {
            throw new NotImplementedException();
        }

        private static async ValueTask<AuthResult> RefreshAsync(string clientToken, string accessToken, string authServer)
        {
            var request = new RefreshRequest
            {
                ClientToken = clientToken,
                AccessToken = accessToken,
            };

            return await AuthenticateAsync(JsonSerializer.Serialize(request, _jsonOptions), true, authServer);
        }

        private static async ValueTask<AuthResult> AuthenticateAsync(string requestJson, bool isRefresh, string authServer)
        {
            var result = new AuthResult();

            try
            {
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var requestUri = new Uri(authServer + (isRefresh ? "/refresh" : "/authenticate"));
                var responseMsg = await _client.PostAsync(requestUri, requestContent);
                string responseJson = await responseMsg.Content.ReadAsStringAsync();

                requestContent.Dispose();
                responseMsg.Dispose();

                if (responseMsg.IsSuccessStatusCode)
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

                    result.IsSuccessful = false;

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

        public static bool IsValidEmailAddress(string emailAddress)
        {
            var regex = new Regex("^\\s*([A-Za-z0-9_-]+(\\.\\w+)*@(\\w+\\.)+\\w{2,5})\\s*$");
            return !string.IsNullOrEmpty(emailAddress) && regex.IsMatch(emailAddress);
        }

        #endregion
    }
}
