﻿using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SparkyStudios.TrakLibrary.Model.Login;
using SparkyStudios.TrakLibrary.Model.Response;
using SparkyStudios.TrakLibrary.Model.Settings;
using SparkyStudios.TrakLibrary.Service.Exception;

namespace SparkyStudios.TrakLibrary.Service.Impl
{
    internal class AuthServiceHttpClientImpl : IAuthService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConnectionService _connectionService;
        private readonly JsonSerializerSettings _serializerSettings;
        private readonly JsonSerializerSettings _deserializerSettings;

        public AuthServiceHttpClientImpl(IHttpClientFactory httpClientFactory, IConnectionService connectionService)
        {
            _httpClientFactory = httpClientFactory;
            _connectionService = connectionService;
            
            // Ensure we use the correct TLS version before making the request.
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
            // Serialization settings.
            _serializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Objects,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
            // Deserialization settings.
            _deserializerSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                TypeNameHandling = TypeNameHandling.Objects,
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead,
                DateParseHandling = DateParseHandling.None,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            };
        }

        public async Task<string> GetTokenAsync(LoginRequest loginRequest)
        {
            if (!_connectionService.IsConnected())
            {
                throw new TaskCanceledException();
            }
            
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));

            var content = new StringContent(JsonConvert.SerializeObject(loginRequest, _serializerSettings),
                Encoding.UTF8, "application/json");
            using var response = await client.PostAsync("auth/token", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = string.Empty
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TokenResponse>(json, _deserializerSettings)?.AccessToken;
        }

        public async Task<CheckedResponse<bool>> VerifyAsync(long userId, string verificationCode)
        {
            if (!_connectionService.IsConnected())
            {
                throw new TaskCanceledException();
            }
            
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));

            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            using var response = await client.PutAsync($"auth/users/{userId}/verify?verification-code={verificationCode}", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = string.Empty
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CheckedResponse<bool>>(json, _deserializerSettings);
        }

        public async Task ReVerifyAsync(long userId)
        {
            if (!_connectionService.IsConnected())
            {
                throw new TaskCanceledException();
            }
            
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));
            
            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            using var response = await client.PutAsync($"auth/users/{userId}/reverify", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = string.Empty
                };
            }
        }

        public async Task RequestRecoveryAsync(string emailAddress)
        {
            if (!_connectionService.IsConnected())
            {
                throw new TaskCanceledException();
            }
            
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));

            var content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
            using var response = await client.PutAsync($"auth/users/recover?email-address={emailAddress}", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = string.Empty
                };
            }
        }

        public async Task<CheckedResponse<RegistrationResponse>> RegisterAsync(RegistrationRequest registrationRequest)
        {
            if (!_connectionService.IsConnected())
            {
                throw new TaskCanceledException();
            }
            
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));

            var content = new StringContent(JsonConvert.SerializeObject(registrationRequest, _serializerSettings),
                Encoding.UTF8, "application/json");
            using var response = await client.PostAsync("auth/users", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = string.Empty
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CheckedResponse<RegistrationResponse>>(json, _deserializerSettings);
        }

        public async Task<CheckedResponse<UserResponse>> RecoverAsync(RecoveryRequest recoveryRequest)
        {
            if (!_connectionService.IsConnected())
            {
                throw new TaskCanceledException();
            }
            
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));

            var content = new StringContent(JsonConvert.SerializeObject(recoveryRequest, _serializerSettings),
                Encoding.UTF8, "application/json");
            using var response = await client.PutAsync("auth/users", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = string.Empty
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CheckedResponse<UserResponse>>(json, _deserializerSettings);
        }

        public async Task<CheckedResponse<bool>> ChangePasswordAsync(long userId, ChangePasswordRequest changePasswordRequest)
        {
            if (!_connectionService.IsConnected())
            {
                throw new TaskCanceledException();
            }
            
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));

            var content = new StringContent(JsonConvert.SerializeObject(changePasswordRequest, _serializerSettings),
                Encoding.UTF8, "application/json");
            using var response = await client.PutAsync($"auth/users/{userId}/change-password", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = string.Empty
                };
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CheckedResponse<bool>>(json, _deserializerSettings);
        }

        public async Task<CheckedResponse<bool>> ChangeEmailAddressAsync(long userId, ChangeEmailAddressRequest changeEmailAddressRequest)
        {
            if (!_connectionService.IsConnected())
            {
                throw new TaskCanceledException();
            }
            
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));

            var content = new StringContent(JsonConvert.SerializeObject(changeEmailAddressRequest, _serializerSettings),
                Encoding.UTF8, "application/json");
            using var response = await client.PutAsync($"auth/users/{userId}/change-email-address", content);

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = string.Empty
                };
            }
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<CheckedResponse<bool>>(json, _deserializerSettings);
        }

        public async Task DeleteByIdAsync(long userId)
        {
            if (!_connectionService.IsConnected())
            {
                throw new TaskCanceledException();
            }
            
            using var client = _httpClientFactory.CreateClient("TrakAuth");
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/vnd.traklibrary.v1+json"));
            
            using var response = await client.DeleteAsync($"auth/users/{userId}");

            if (!response.IsSuccessStatusCode)
            {
                throw new ApiException
                {
                    StatusCode = response.StatusCode,
                    Content = string.Empty
                };
            }
        }
    }
}