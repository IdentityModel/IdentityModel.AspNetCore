// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Implements token endpoint operations using IdentityModel
    /// </summary>
    public class TokenEndpointService : ITokenEndpointService
    {
        private readonly AccessTokenManagementOptions _accessTokenManagementOptions;
        
        private readonly IOptionsSnapshot<OpenIdConnectOptions> _oidcOptions;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="accessTokenManagementOptions"></param>
        /// <param name="oidcOptions"></param>
        /// <param name="schemeProvider"></param>
        /// <param name="httpClient"></param>
        public TokenEndpointService(
            IOptions<AccessTokenManagementOptions> accessTokenManagementOptions,
            IOptionsSnapshot<OpenIdConnectOptions> oidcOptions,
            IAuthenticationSchemeProvider schemeProvider,
            HttpClient httpClient)
        {
            _accessTokenManagementOptions = accessTokenManagementOptions.Value;

            _oidcOptions = oidcOptions;
            _schemeProvider = schemeProvider;
            _httpClient = httpClient;
        }

        /// <inheritdoc/>
        public async Task<TokenResponse> RequestClientAccessToken(string clientName = null)
        {
            TokenClientOptions tokenClientOptions;

            // if a named client configuration was passed in, try to load it
            if (string.IsNullOrEmpty(clientName) || string.Equals(clientName, AccessTokenManagementOptions.DefaultClientName))
            {
                // if only one client configuration exists, load that
                if (_accessTokenManagementOptions.Client.Clients.Count == 1)
                {
                    tokenClientOptions = _accessTokenManagementOptions.Client.Clients.First().Value;
                }
                // otherwise fall back to the scheme configuration
                else
                {
                    var oidcOptions = await GetOidcOptionsAsync(_accessTokenManagementOptions.Client.OidcSchemeClient);
                    var configuration = await oidcOptions.ConfigurationManager.GetConfigurationAsync(default);

                    tokenClientOptions = new TokenClientOptions
                    {
                        Address = configuration.TokenEndpoint,

                        ClientId = oidcOptions.ClientId,
                        ClientSecret = oidcOptions.ClientSecret
                    };
                }
            }
            else
            {
                if (!_accessTokenManagementOptions.Client.Clients.TryGetValue(clientName, out tokenClientOptions))
                {
                    throw new InvalidOperationException($"No access token client configuration found for client: {clientName}");
                }
            }

            return await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = tokenClientOptions.Address,

                ClientId = tokenClientOptions.ClientId,
                ClientSecret = tokenClientOptions.ClientSecret
            });
        }

        /// <inheritdoc/>
        public async Task<TokenResponse> RefreshUserAccessTokenAsync(string refreshToken)
        {
            var oidcOptions = await GetOidcOptionsAsync(_accessTokenManagementOptions.User.Scheme);
            var configuration = await oidcOptions.ConfigurationManager.GetConfigurationAsync(default);

            return await _httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = configuration.TokenEndpoint,

                ClientId = oidcOptions.ClientId,
                ClientSecret = oidcOptions.ClientSecret,
                RefreshToken = refreshToken
            });
        }

        /// <inheritdoc/>
        public async Task<TokenRevocationResponse> RevokeRefreshTokenAsync(string refreshToken)
        {
            var oidcOptions = await GetOidcOptionsAsync(_accessTokenManagementOptions.User.Scheme);
            var configuration = await oidcOptions.ConfigurationManager.GetConfigurationAsync(default);

            return await _httpClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = configuration.AdditionalData[OidcConstants.Discovery.RevocationEndpoint].ToString(),
                ClientId = oidcOptions.ClientId,
                ClientSecret = oidcOptions.ClientSecret,
                Token = refreshToken,
                TokenTypeHint = OidcConstants.TokenTypes.RefreshToken
            });
        }

        private async Task<OpenIdConnectOptions> GetOidcOptionsAsync(string schemeName)
        {
            if (string.IsNullOrEmpty(schemeName))
            {
                var scheme = await _schemeProvider.GetDefaultChallengeSchemeAsync();
                return _oidcOptions.Get(scheme.Name);
            }
            else
            {
                return _oidcOptions.Get(schemeName);
            }
        }
    }
}