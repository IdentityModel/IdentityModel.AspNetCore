// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Implements basic token management logic
    /// </summary>
    public class ClientAccessTokenManagementService : IClientTokenManagementService
    {
        static readonly ConcurrentDictionary<string, Lazy<Task<string>>> ClientTokenRequestDictionary =
            new ConcurrentDictionary<string, Lazy<Task<string>>>();
        
        private readonly ISystemClock _clock;
        private readonly AccessTokenManagementOptions _options;
        private readonly ITokenEndpointService _tokenEndpointService;
        private readonly IClientAccessTokenCache _clientAccessTokenCache;
        private readonly ILogger<ClientAccessTokenManagementService> _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="clock"></param>
        /// <param name="options"></param>
        /// <param name="tokenEndpointService"></param>
        /// <param name="clientAccessTokenCache"></param>
        /// <param name="logger"></param>
        public ClientAccessTokenManagementService(
            ISystemClock clock,
            IOptions<AccessTokenManagementOptions> options,
            ITokenEndpointService tokenEndpointService,
            IClientAccessTokenCache clientAccessTokenCache,
            ILogger<ClientAccessTokenManagementService> logger)
        {
            _clock = clock;
            _options = options.Value;
            _tokenEndpointService = tokenEndpointService;
            _clientAccessTokenCache = clientAccessTokenCache;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<string> GetClientAccessTokenAsync(
            string clientName = AccessTokenManagementDefaults.DefaultTokenClientName, 
            ClientAccessTokenParameters parameters = null,
            CancellationToken cancellationToken = default)
        {
            parameters ??= new ClientAccessTokenParameters();
            
            if (parameters.ForceRenewal == false)
            {
                var item = await _clientAccessTokenCache.GetAsync(clientName, cancellationToken);
                if (item != null)
                {
                    return item.AccessToken;
                }
            }

            try
            {
                return await ClientTokenRequestDictionary.GetOrAdd(clientName, _ =>
                {
                    return new Lazy<Task<string>>(async () =>
                    {
                        var response = await _tokenEndpointService.RequestClientAccessToken(clientName, parameters, cancellationToken);

                        if (response.IsError)
                        {
                            _logger.LogError("Error requesting access token for client {clientName}. Error = {error}", clientName, response.Error);
                            return null;
                        }

                        await _clientAccessTokenCache.SetAsync(clientName, response.AccessToken, response.ExpiresIn, cancellationToken);
                        return response.AccessToken;
                    });
                }).Value;
            }
            finally
            {
                ClientTokenRequestDictionary.TryRemove(clientName, out _);
            }
        }

        /// <inheritdoc/>
        public Task DeleteClientAccessTokenAsync(
            string clientName = AccessTokenManagementDefaults.DefaultTokenClientName, 
            ClientAccessTokenParameters parameters = null, 
            CancellationToken cancellationToken = default)
        {
            return _clientAccessTokenCache.DeleteAsync(clientName, cancellationToken);
        }
    }
}