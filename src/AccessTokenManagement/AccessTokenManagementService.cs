using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    public class AccessTokenManagementService : IAccessTokenManagementService
    {
        static readonly ConcurrentDictionary<string, Lazy<Task<string>>> _dictionary =
            new ConcurrentDictionary<string, Lazy<Task<string>>>();

        private readonly IUserTokenStore _userTokenStore;
        private readonly ISystemClock _clock;
        private readonly AccessTokenManagementOptions _options;
        private readonly TokenEndpointService _tokenEndpointService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AccessTokenManagementService> _logger;

        public AccessTokenManagementService(
            IUserTokenStore userTokenStore,
            ISystemClock clock,
            IOptions<AccessTokenManagementOptions> options,
            TokenEndpointService tokenEndpointService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AccessTokenManagementService> logger)
        {
            _userTokenStore = userTokenStore;
            _clock = clock;
            _options = options.Value;
            _tokenEndpointService = tokenEndpointService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <summary>
        /// Returns either a cached of a new access token for a given client configuration or the default client
        /// </summary>
        /// <param name="name">Name of the client configuration, of default</param>
        /// <returns>The access token.</returns>
        public async Task<string> GetClientAccessTokenAsync(string name = null)
        {
            var response = await _tokenEndpointService.RequestClientAccessToken(name);

            if (response.IsError)
            {
                var configName = name ?? "default";
                _logger.LogError("Error requesting access token for client {configName}. Error = {error}", configName, response.Error);
            }

            return response.AccessToken;
        }

        public async Task<string> GetUserAccessTokenAsync()
        {
            var userToken = await _userTokenStore.GetTokenAsync(_httpContextAccessor.HttpContext.User);

            var dtRefresh = userToken.Expiration.Subtract(_options.User.RefreshBeforeExpiration);
            if (dtRefresh < _clock.UtcNow)
            {
                _logger.LogDebug("Token {token} needs refreshing.", userToken.AccessToken);

                try
                {
                    return await _dictionary.GetOrAdd(userToken.RefreshToken, (string refreshToken) =>
                    {
                        return new Lazy<Task<string>>(async () =>
                        {
                            var refreshed = await RefreshUserAccessTokenAsync();
                            return refreshed.AccessToken;
                        });
                    }).Value;
                }
                finally
                {
                    _dictionary.TryRemove(userToken.RefreshToken, out _);
                }
            }

            return userToken.AccessToken;
        }

        public async Task<TokenResponse> RefreshUserAccessTokenAsync()
        {
            var userToken = await _userTokenStore.GetTokenAsync(_httpContextAccessor.HttpContext.User);
            var response = await _tokenEndpointService.RefreshUserAccessTokenAsync(userToken.RefreshToken);

            if (!response.IsError)
            {
                await _userTokenStore.StoreTokenAsync(_httpContextAccessor.HttpContext.User, response.AccessToken, response.ExpiresIn, response.RefreshToken);
            }

            return response;
        }

        public async Task RevokeRefreshTokenAsync()
        {
            var userToken = await _userTokenStore.GetTokenAsync(_httpContextAccessor.HttpContext.User);

            if (!string.IsNullOrEmpty(userToken.RefreshToken))
            {
                await _tokenEndpointService.RevokeRefreshTokenAsync(userToken.RefreshToken);
            }
        }
    }
}