﻿using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    public class ClientAccessTokenCache : IClientAccessTokenCache
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<ClientAccessTokenCache> _logger;
        private readonly AccessTokenManagementOptions _options;

        public ClientAccessTokenCache(IDistributedCache cache, IOptions<AccessTokenManagementOptions> options, ILogger<ClientAccessTokenCache> logger)
        {
            _cache = cache;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<ClientAccessToken> GetAsync(string clientName)
        {
            if (clientName is null) throw new ArgumentNullException(nameof(clientName));
            
            var entry = await _cache.GetStringAsync(_options.Client.CacheKeyPrefix + clientName);

            if (entry != null)
            {
                _logger.LogDebug("Cache hit for access token for client: {clientName}", clientName);

                var values = entry.Split(new[] { "___" }, StringSplitOptions.RemoveEmptyEntries);

                return new ClientAccessToken
                {
                    AccessToken = values[0],
                    Expiration = DateTimeOffset.FromUnixTimeSeconds(long.Parse(values[1]))
                };
            }

            _logger.LogDebug("Cache miss for access token for client: {clientName}", clientName);
            return null;
        }

        public async Task SetAsync(string clientName, string accessToken, int expiresIn)
        {
            if (clientName is null) throw new ArgumentNullException(nameof(clientName));

            var expiration = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
            var expirationEpoch = expiration.ToUnixTimeSeconds();
            var cacheExpiration = expiration.AddSeconds(-(_options.Client.CacheLifetimeBuffer));

            var data = $"{accessToken}___{expirationEpoch.ToString()}";

            var entryOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = cacheExpiration
            };

            _logger.LogDebug("Caching access token for client: {clientName}. Expiration: {expiration}", clientName, cacheExpiration);
            await _cache.SetStringAsync(_options.Client.CacheKeyPrefix + clientName, data, entryOptions);
        }
    }
}