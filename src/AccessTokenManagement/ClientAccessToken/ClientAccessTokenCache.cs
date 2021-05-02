// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Client access token cache using IDistributedCache
    /// </summary>
    public class ClientAccessTokenCache : IClientAccessTokenCache
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<ClientAccessTokenCache> _logger;
        private readonly AccessTokenManagementOptions _options;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="cache"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public ClientAccessTokenCache(IDistributedCache cache, IOptions<AccessTokenManagementOptions> options, ILogger<ClientAccessTokenCache> logger)
        {
            _cache = cache;
            _logger = logger;
            _options = options.Value;
        }

        /// <inheritdoc/>
        public async Task<ClientAccessToken> GetAsync(string clientName, ClientAccessTokenParameters parameters, CancellationToken cancellationToken = default)
        {
            if (clientName is null) throw new ArgumentNullException(nameof(clientName));
            
            var cacheKey = GenerateCacheKey(_options, clientName, parameters);
            var entry = await _cache.GetStringAsync(cacheKey, token: cancellationToken);

            if (entry != null)
            {
                try
                {
                    _logger.LogDebug("Cache hit for access token for client: {clientName}", clientName);

                    var values = entry.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);

                    return new ClientAccessToken
                    {
                        AccessToken = values[0],
                        Expiration = DateTimeOffset.FromUnixTimeSeconds(long.Parse(values[1]))
                    };
                }
                catch (Exception ex)
                {
                    _logger.LogCritical(ex, "Error parsing cached access token for client {clientName}", clientName);
                    return null;
                }
            }

            _logger.LogDebug("Cache miss for access token for client: {clientName}", clientName);
            return null;
        }

        /// <inheritdoc/>
        public async Task SetAsync(string clientName, string accessToken, int expiresIn, ClientAccessTokenParameters parameters, CancellationToken cancellationToken = default)
        {
            if (clientName is null) throw new ArgumentNullException(nameof(clientName));

            var expiration = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
            var expirationEpoch = expiration.ToUnixTimeSeconds();
            var cacheExpiration = expiration.AddSeconds(-_options.Client.CacheLifetimeBuffer);

            var data = $"{accessToken}___{expirationEpoch.ToString()}";

            var entryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = cacheExpiration
            };

            _logger.LogDebug("Caching access token for client: {clientName}. Expiration: {expiration}", clientName, cacheExpiration);
            
            var cacheKey = GenerateCacheKey(_options, clientName, parameters);
            await _cache.SetStringAsync(cacheKey, data, entryOptions, token: cancellationToken);
        }

        /// <inheritdoc/>
        public Task DeleteAsync(string clientName, ClientAccessTokenParameters parameters, CancellationToken cancellationToken = default)
        {
            if (clientName is null) throw new ArgumentNullException(nameof(clientName));

            var cacheKey = GenerateCacheKey(_options, clientName, parameters);
            return _cache.RemoveAsync(cacheKey, cancellationToken);
        }

        /// <summary>
        /// Generates the cache key based on various inputs
        /// </summary>
        /// <param name="options"></param>
        /// <param name="clientName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected virtual string GenerateCacheKey(AccessTokenManagementOptions options, string clientName,
            ClientAccessTokenParameters parameters)
        {
            return options.Client.CacheKeyPrefix + clientName + parameters?.Resource ?? "";
        }
    }
}