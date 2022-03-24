using System.Threading.Tasks;
using Bff.InMemoryTests.TestFramework;
using IdentityModel.Client;

namespace Bff.InMemoryTests.TestHosts
{
    public class FakeIDPTenantlessClient
    {
        private readonly TestBrowserClient _httpClient;

        public FakeIDPTenantlessClient(TestBrowserClient client) => _httpClient = client;

        public async Task<UserInfoResponse> GetUserInfoAsync(string userToken, AuthSettings authSettings)
        {
            return await _httpClient.GetUserInfoAsync(new UserInfoRequest
            {
                Address = _httpClient.BaseAddress + "connect/userinfo",
                ClientId = authSettings.TenantedClientId,
                ClientSecret = authSettings.TenantedClientSecret,
                Token = userToken
            });

            return new UserInfoResponse() { };
            // send custom grant to token endpoint, return response
        }

        /// <summary>
        /// Requests token exchange
        /// </summary>
        /// <param name="userToken"></param>
        /// <param name="authSettings"></param>
        /// <returns></returns>
        public async Task<TokenResponse> DelegateAsync(string userToken, AuthSettings authSettings)
        {
            return await _httpClient.RequestTokenAsync(new TokenRequest
                {
                    Address = _httpClient.BaseAddress+ "connect/token",
                    GrantType = "urn:ietf:params:oauth:grant-type:token-exchange",

                    ClientId = authSettings.TenantlessClientId,
                    ClientSecret = authSettings.TenantlessClientSecret,
                    
                    Parameters =
                    {
                        {ProtocolRequestParameters.Scope, $"scope1 { "offline_access"}  {"an.api"}"},
                        {ProtocolRequestParameters.SubjectToken, userToken},
                        {ProtocolRequestParameters.SubjectTokenType, TokenTypes.AccessTokenFullUrn}
                    }
                });

            return new TokenResponse() { };
            // send custom grant to token endpoint, return response
        }
    }

    public static class ProtocolRequestParameters
    {
        public const string Scope = "scope";
        public const string SubjectToken = "subject_token";
        public const string SubjectTokenType = "subject_token_type";
    }
    public static class TokenTypes
    {
        public const string IdentityToken = "urn:ietf:params:oauth:token-type:id_token";
        public const string IdentityTokenFullUrn = "id_token";
        public const string AccessToken = "access_token";
        public const string AccessTokenFullUrn = "urn:ietf:params:oauth:token-type:access_token";
    }
}