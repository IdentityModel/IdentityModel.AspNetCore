// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.Client;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Implements token endpoint operations using IdentityModel
    /// </summary>
    public class TokenEndpointService : ITokenEndpointService
    {
        private readonly ITokenClientConfigurationService _configService;
        private readonly HttpClient _httpClient;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="configService"></param>
        /// <param name="httpClientFactory"></param>
        public TokenEndpointService(
            ITokenClientConfigurationService configService,
            IHttpClientFactory httpClientFactory)
        {
            _configService = configService;
            _httpClient = httpClientFactory.CreateClient(AccessTokenManagementDefaults.BackChannelHttpClientName);
        }

        /// <inheritdoc/>
        public async Task<TokenResponse> RequestClientAccessToken(
            string clientName = AccessTokenManagementDefaults.DefaultTokenClientName, 
            CancellationToken cancellationToken = default)
        {
            var requestDetails = await _configService.GetClientCredentialsRequestAsync(clientName);

            return await _httpClient.RequestClientCredentialsTokenAsync(requestDetails, cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TokenResponse> RefreshUserAccessTokenAsync(string refreshToken, UserAccessTokenParameters parameters = null, CancellationToken cancellationToken = default)
        {
            var requestDetails = await _configService.GetRefreshTokenRequestAsync();
            requestDetails.RefreshToken = refreshToken;

            if (!string.IsNullOrEmpty(parameters?.Resource))
            {
                requestDetails.Resource.Add(parameters.Resource);
            }

            return await _httpClient.RequestRefreshTokenAsync(requestDetails, cancellationToken: cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TokenRevocationResponse> RevokeRefreshTokenAsync(string refreshToken, UserAccessTokenParameters parameters = null, CancellationToken cancellationToken = default)
        {
            var requestDetails = await _configService.GetTokenRevocationRequestAsync();
            requestDetails.Token = refreshToken;
            requestDetails.TokenTypeHint = OidcConstants.TokenTypes.RefreshToken;

            return await _httpClient.RevokeTokenAsync(requestDetails, cancellationToken: cancellationToken);
        }
    }
}