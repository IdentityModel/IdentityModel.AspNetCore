using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement.Azure
{
    /// <summary>
    ///     Client access token cache using Azure Key Vault SecretClient.
    /// </summary>
    public class KeyVaultClientAccessTokenCache : IClientAccessTokenCache
    {
        private readonly SecretClient _secretClient;
        private readonly ILogger<KeyVaultClientAccessTokenCache> _logger;
        private readonly ClientAccessTokenManagementOptions _options;
        private const string EntrySeparator = "___";

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="secretClient"></param>
        /// <param name="options"></param>
        public KeyVaultClientAccessTokenCache(
            ILogger<KeyVaultClientAccessTokenCache> logger,
            SecretClient secretClient,
            ClientAccessTokenManagementOptions options)
        {
            _secretClient = secretClient;
            _options = options;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<ClientAccessToken?> GetAsync(
            string clientName, 
            ClientAccessTokenParameters parameters,
            CancellationToken cancellationToken = default)
        {
            var cacheKey = GenerateCacheKey(clientName, parameters);
            Response<KeyVaultSecret>? response = null;

            try
            {
                response = await _secretClient.GetSecretAsync(cacheKey, cancellationToken: cancellationToken);
            }
            catch (RequestFailedException e) when (e.Status == StatusCodes.Status404NotFound) { };          
      
            if (response is not null 
                && response.Value is not null 
                && !string.IsNullOrEmpty(response.Value.Value))
            {
                try
                {
                    _logger.LogDebug("Cache hit for access token for client: {clientName}", clientName);
                    var entry = response.Value.Value;

                    var index = entry.LastIndexOf(EntrySeparator, StringComparison.Ordinal);

                    return new ClientAccessToken
                    {
                        AccessToken = entry.Substring(0, index),
                        Expiration = DateTimeOffset.FromUnixTimeSeconds(long.Parse(entry.AsSpan(index + EntrySeparator.Length)))
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
        public async Task DeleteAsync(string clientName, ClientAccessTokenParameters parameters, CancellationToken cancellationToken = default)
        {
            if (clientName is null) throw new ArgumentNullException(nameof(clientName));

            var cacheKey = GenerateCacheKey(clientName, parameters);

            var operation = await _secretClient.StartDeleteSecretAsync(cacheKey, cancellationToken);
            await operation.WaitForCompletionAsync(cancellationToken);
            await _secretClient.PurgeDeletedSecretAsync(cacheKey, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task SetAsync(string clientName, string accessToken, int expiresIn, ClientAccessTokenParameters parameters, CancellationToken cancellationToken = default)
        {

            if (clientName is null) throw new ArgumentNullException(nameof(clientName));

            var expiration = DateTimeOffset.UtcNow.AddSeconds(expiresIn);
            var expirationEpoch = expiration.ToUnixTimeSeconds();
            var cacheExpiration = expiration.AddSeconds(-_options.CacheLifetimeBuffer);

            var data = $"{accessToken}{EntrySeparator}{expirationEpoch}";          

            _logger.LogDebug("Caching access token for client: {clientName}. Expiration: {expiration}", clientName, cacheExpiration);

            var cacheKey = GenerateCacheKey(clientName, parameters);

            var kvSecret = new KeyVaultSecret(cacheKey, data);
            kvSecret.Properties.ExpiresOn = expiration;
            await _secretClient.SetSecretAsync(cacheKey, data, cancellationToken);
        }

        private string GenerateCacheKey(
            string clientName,
            ClientAccessTokenParameters? parameters = null) =>
                "IdentityModel--AccessTokenManagement" + "--" + clientName + "--" + parameters?.Resource ?? "";
    }
}
