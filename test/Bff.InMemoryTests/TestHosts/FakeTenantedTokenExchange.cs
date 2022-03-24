using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Bff.InMemoryTests.TestHosts
{
    public static class FakeTenantedTokenExchange
    {
        private const string IdentityServerApiTokenName = "urn:IdentityServerApi";
        private const string AnApiTokenName = "urn:an.api";
        private const string TokenNamesKey = ".TokenNames";
        private const string ExpiresAtKey = "expires_at";

        /// <summary>
        /// Pass-through to DelegateAsync and stores exchanged token, along with other AuthenticationTokens, and AuthenticationProperties items.
        /// </summary>
        /// <param name="userTokens"></param>
        /// <param name="authenticationProperties"></param>
        /// <param name="idpUri"></param>
        /// <param name="authSettings"></param>
        /// <param name="identityToken"></param>
        /// <returns></returns>
        public static async Task ExchangeTokenAsync((string accessToken, string refreshToken) userTokens, AuthenticationProperties authenticationProperties, AuthSettings authSettings, string identityToken = null, FakeIDPTenantlessClient idpTenantlessClient = null)
        {
            var tokens = new List<AuthenticationToken>();

            var accessToken = userTokens.accessToken;
            var idToken = identityToken;
            var refreshToken = userTokens.refreshToken;
            var tokenType = "Bearer";

            // Perform token-exchange if Claim Name "amr" (Authentication Methods References) in AccessToken is not token-exchange.
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(accessToken);

            if (!token.Claims.Contains(new Claim("amr", "token-exchange")))
            {
                var response = await idpTenantlessClient.DelegateAsync(accessToken, authSettings);
                var exchangedAccessToken = response.AccessToken;
                authenticationProperties.Items.Clear();

                var stringList = new List<string>();

                if (!string.IsNullOrEmpty(idToken))
                {
                    tokens.Add(new AuthenticationToken { Name = $"{OpenIdConnectParameterNames.IdToken}", Value = idToken });
                }

                if (!string.IsNullOrEmpty(accessToken))
                {
                    tokens.Add(new AuthenticationToken { Name = $"{OpenIdConnectParameterNames.AccessToken}::{IdentityServerApiTokenName}", Value = accessToken });
                }

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    tokens.Add(new AuthenticationToken { Name = $"{OpenIdConnectParameterNames.RefreshToken}::oidc", Value = refreshToken });
                }

                if (!string.IsNullOrEmpty(response.ExpiresIn.ToString()))
                {
                    if (int.TryParse(response.ExpiresIn.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture,
                            out int value))
                    {
                        var expiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(value);
                        tokens.Add(new AuthenticationToken { Name = ExpiresAtKey, Value = expiresAt.ToString("o", CultureInfo.InvariantCulture) });
                        tokens.Add(new AuthenticationToken { Name = $"{ExpiresAtKey}::{IdentityServerApiTokenName}", Value = expiresAt.ToString("o", CultureInfo.InvariantCulture) });
                        tokens.Add(new AuthenticationToken { Name = $"{ExpiresAtKey}::{AnApiTokenName}", Value = expiresAt.ToString("o", CultureInfo.InvariantCulture) });
                    }
                }

                if (!string.IsNullOrEmpty(exchangedAccessToken))
                {
                    tokens.Add(new AuthenticationToken { Name = $"{OpenIdConnectParameterNames.AccessToken}::{AnApiTokenName}", Value = exchangedAccessToken });
                }

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    tokens.Add(new AuthenticationToken { Name = $"{OpenIdConnectParameterNames.RefreshToken}::achallengescheme", Value = response.RefreshToken });
                }

                if (!string.IsNullOrEmpty(tokenType))
                {
                    tokens.Add(new AuthenticationToken { Name = OpenIdConnectParameterNames.TokenType, Value = tokenType });
                }

                authenticationProperties.Items.Add(new KeyValuePair<string, string?>(TokenNamesKey, string.Join(";", tokens.Select(t => t.Name).ToList())));

                // With Token exchange completed we can go ahead and store them. 
                authenticationProperties.StoreTokens(tokens);
            }
        }
    }
}