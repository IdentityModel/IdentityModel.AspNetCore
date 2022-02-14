// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Implements basic token management logic
    /// </summary>
    public class ClientAccessTokenManagementService : IClientAccessTokenManagementService
    {
        private readonly IClientAccessTokenRequestSynchronization _sync;
        private readonly ITokenEndpointService _tokenEndpointService;
        private readonly IClientAccessTokenCache _clientAccessTokenCache;
        private readonly ILogger<ClientAccessTokenManagementService> _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="sync"></param>
        /// <param name="tokenEndpointService"></param>
        /// <param name="clientAccessTokenCache"></param>
        /// <param name="logger"></param>
        public ClientAccessTokenManagementService(
            IClientAccessTokenRequestSynchronization sync,
            ITokenEndpointService tokenEndpointService,
            IClientAccessTokenCache clientAccessTokenCache,
            ILogger<ClientAccessTokenManagementService> logger)
        {
            _sync = sync;
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
                var item = await _clientAccessTokenCache.GetAsync(clientName, parameters, cancellationToken);
                if (item != null)
                {
                    return item.AccessToken;
                }
            }

            try
            {
                return await _sync.Dictionary.GetOrAdd(clientName, _ =>
                {
                    return new Lazy<Task<string>>(async () =>
                    {
                        var response = await _tokenEndpointService.RequestClientAccessToken(clientName, parameters, cancellationToken);

                        if (response.IsError)
                        {
                            _logger.LogError("Error requesting access token for client {clientName}. Error = {error}. Error description = {errorDescription}", clientName, response.Error, response.ErrorDescription);
                            return null;
                        }

                        await _clientAccessTokenCache.SetAsync(clientName, response.AccessToken, response.ExpiresIn, parameters, cancellationToken);
                        return response.AccessToken;
                    });
                }).Value;
            }
            finally
            {
                _sync.Dictionary.TryRemove(clientName, out _);
            }
        }

        /// <inheritdoc/>
        public Task DeleteClientAccessTokenAsync(
            string clientName = AccessTokenManagementDefaults.DefaultTokenClientName, 
            ClientAccessTokenParameters parameters = null, 
            CancellationToken cancellationToken = default)
        {
            return _clientAccessTokenCache.DeleteAsync(clientName, parameters, cancellationToken);
        }
    }
}