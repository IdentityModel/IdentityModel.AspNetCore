// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    public class TokenEndpointService : ITokenEndpointService
    {
        private readonly AccessTokenManagementOptions _accessTokenManagementOptions;
        
        private readonly IOptionsSnapshot<OpenIdConnectOptions> _oidcOptions;
        private readonly IAuthenticationSchemeProvider _schemeProvider;
        private readonly HttpClient _httpClient;
        private readonly ILogger<TokenEndpointService> _logger;

        public TokenEndpointService(
            IOptions<AccessTokenManagementOptions> accessTokenManagementOptions,
            IOptionsSnapshot<OpenIdConnectOptions> oidcOptions,
            IAuthenticationSchemeProvider schemeProvider,
            HttpClient httpClient,
            ILogger<TokenEndpointService> logger)
        {
            _accessTokenManagementOptions = accessTokenManagementOptions.Value;

            _oidcOptions = oidcOptions;
            _schemeProvider = schemeProvider;
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<TokenResponse> RefreshUserAccessTokenAsync(string refreshToken)
        {
            var oidcOptions = await GetOidcOptionsAsync(_accessTokenManagementOptions.User.Scheme);
            var configuration = await oidcOptions.ConfigurationManager.GetConfigurationAsync(default);

            var response = await _httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
            {
                Address = configuration.TokenEndpoint,

                ClientId = oidcOptions.ClientId,
                ClientSecret = oidcOptions.ClientSecret,
                RefreshToken = refreshToken
            });

            if (response.IsError)
            {
                _logger.LogError("Error refreshing access token. Error = {error}", response.Error);
            }

            return response;
        }

        public async Task<TokenRevocationResponse> RevokeRefreshTokenAsync(string refreshToken)
        {
            var oidcOptions = await GetOidcOptionsAsync(_accessTokenManagementOptions.User.Scheme);
            var configuration = await oidcOptions.ConfigurationManager.GetConfigurationAsync(default);

            var response = await _httpClient.RevokeTokenAsync(new TokenRevocationRequest
            {
                Address = configuration.AdditionalData[OidcConstants.Discovery.RevocationEndpoint].ToString(),
                ClientId = oidcOptions.ClientId,
                ClientSecret = oidcOptions.ClientSecret,
                Token = refreshToken,
                TokenTypeHint = OidcConstants.TokenTypes.RefreshToken
            });

            if (response.IsError)
            {
                _logger.LogError("Error revoking refresh token. Error = {error}", response.Error);
            }

            return response;
        }

        public async Task<TokenResponse> RequestClientAccessToken(string clientName = null)
        {
            TokenClientOptions tokenClientOptions = null;

            if (!string.IsNullOrEmpty(clientName))
            {
                tokenClientOptions = _accessTokenManagementOptions.Client.Clients[clientName];
            }
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

            var response = await _httpClient.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest
            {
                Address = tokenClientOptions.Address,

                ClientId = tokenClientOptions.ClientId,
                ClientSecret = tokenClientOptions.ClientSecret
            });

            if (response.IsError)
            {
                _logger.LogError("Error request client access token. Error = {error}", response.Error);
            }

            return response;
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