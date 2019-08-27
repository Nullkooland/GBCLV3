using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GBCLV3.Models.Launcher;
using GBCLV3.Utils;

namespace GBCLV3.Services.Launcher
{
    static class AuthService
    {
        #region Private Members

        private const string _authServer = "https://authserver.mojang.com/";

        private static readonly HttpClient _client = new HttpClient() { Timeout = TimeSpan.FromSeconds(15) };

        private static readonly JsonSerializerOptions _jsonOptions
            = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        private static readonly string _guid = Guid.NewGuid().ToString("N");

        #endregion

        #region Public Methods

        public static async Task<AuthResult> LoginAsync(string email, string password)
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
                ClientToken = _guid,
                RequestUser = false,
            };

            return await OnlineAuthenticateAsync(JsonSerializer.Serialize(request, _jsonOptions), false);
        }

        public static async Task<AuthResult> RefreshAsync(string clientToken, string accessToken)
        {
            var request = new RefreshRequest
            {
                ClientToken = clientToken,
                AccessToken = accessToken,
            };

            return await OnlineAuthenticateAsync(JsonSerializer.Serialize(request, _jsonOptions), true);
        }

        public static AuthResult GetOfflineProfile(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return new AuthResult
                {
                    IsSuccessful = false,
                    ErrorType = AuthErrorType.UsernameEmpty,
                    ErrorMessage = "${UsernameEmptyError}",
                };
            }

            return new AuthResult
            {
                Username = username,
                UUID = CryptUtil.GetStringMD5(username),
                ClientToken = _guid,
                AccessToken = _guid,
                UserType = "mojang",
                IsSuccessful = true,
            };
        }

        #endregion

        #region Helper Method

        private static async Task<AuthResult> OnlineAuthenticateAsync(string requestJson, bool isRefresh)
        {
            var result = new AuthResult();

            try
            {
                var requestContent = new StringContent(requestJson, Encoding.UTF8, "application/json");
                var requestUri = new Uri(_authServer + (isRefresh ? "/refresh" : "/authenticate"));
                var responseMsg = await _client.PostAsync(requestUri, requestContent);
                string responseJson = await responseMsg.Content.ReadAsStringAsync();

                requestContent.Dispose();
                responseMsg.Dispose();

                if (responseMsg.StatusCode == HttpStatusCode.OK)
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
            return (!string.IsNullOrEmpty(emailAddress) && regex.IsMatch(emailAddress));
        }

        #endregion
    }
}
