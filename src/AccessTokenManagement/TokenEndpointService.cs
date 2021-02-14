// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
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
            ClientAccessTokenParameters parameters = null,
            CancellationToken cancellationToken = default)
        {
            parameters ??= new ClientAccessTokenParameters();
            
            var requestDetails = await _configService.GetClientCredentialsRequestAsync(clientName, parameters);
            
#if NET5_0
            requestDetails.Options.TryAdd(AccessTokenManagementOptions.AccessTokenParametersOptionsName, parameters);
#elif NETCOREAPP3_1
            requestDetails.Properties.Add(AccessTokenManagementOptions.AccessTokenParametersOptionsName, parameters);
#endif
            
            if (!string.IsNullOrWhiteSpace(parameters.Resource))
            {
                requestDetails.Resource.Add(parameters.Resource);
            }
            
            return await _httpClient.RequestClientCredentialsTokenAsync(requestDetails, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TokenResponse> RefreshUserAccessTokenAsync(
            string refreshToken, 
            UserAccessTokenParameters parameters = null, 
            CancellationToken cancellationToken = default)
        {
            parameters ??= new UserAccessTokenParameters();
            
            var requestDetails = await _configService.GetRefreshTokenRequestAsync(parameters);
            requestDetails.RefreshToken = refreshToken;
            
#if NET5_0
            requestDetails.Options.TryAdd(AccessTokenManagementOptions.AccessTokenParametersOptionsName, parameters);
#elif NETCOREAPP3_1
            requestDetails.Properties.Add(AccessTokenManagementOptions.AccessTokenParametersOptionsName, parameters);
#endif

            if (!string.IsNullOrEmpty(parameters.Resource))
            {
                requestDetails.Resource.Add(parameters.Resource);
            }

            return await _httpClient.RequestRefreshTokenAsync(requestDetails, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<TokenRevocationResponse> RevokeRefreshTokenAsync(
            string refreshToken, 
            UserAccessTokenParameters parameters = null, 
            CancellationToken cancellationToken = default)
        {
            parameters ??= new UserAccessTokenParameters();
            
            var requestDetails = await _configService.GetTokenRevocationRequestAsync(parameters);
            requestDetails.Token = refreshToken;
            requestDetails.TokenTypeHint = OidcConstants.TokenTypes.RefreshToken;
            
#if NET5_0
            requestDetails.Options.TryAdd(AccessTokenManagementOptions.AccessTokenParametersOptionsName, parameters);
#elif NETCOREAPP3_1
            requestDetails.Properties.Add(AccessTokenManagementOptions.AccessTokenParametersOptionsName, parameters);
#endif

            return await _httpClient.RevokeTokenAsync(requestDetails, cancellationToken);
        }
    }
}