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

        public async Task<string> GetClientAccessTokenAsync(string name = null)
        {
            var response = await _tokenEndpointService.RequestClientAccessToken(name);

            // todo: error handling

            return response.AccessToken;
        }

        public async Task<string> GetUserAccessTokenAsync()
        {
            var tokens = await _userTokenStore.GetTokenAsync(_httpContextAccessor.HttpContext.User);

            var dtRefresh = tokens.expiration.Subtract(_options.User.RefreshBeforeExpiration);
            if (dtRefresh < _clock.UtcNow)
            {
                _logger.LogDebug("Token {token} needs refreshing.", tokens.accessToken);

                try
                {
                    return await _dictionary.GetOrAdd(tokens.refreshToken, (string refreshToken) =>
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
                    _dictionary.TryRemove(tokens.refreshToken, out _);
                }
            }

            return tokens.accessToken;
        }

        public async Task<TokenResponse> RefreshUserAccessTokenAsync()
        {
            var tokens = await _userTokenStore.GetTokenAsync(_httpContextAccessor.HttpContext.User);
            var response = await _tokenEndpointService.RefreshUserAccessTokenAsync(tokens.refreshToken);

            if (!response.IsError)
            {
                await _userTokenStore.StoreTokenAsync(_httpContextAccessor.HttpContext.User, response.AccessToken, response.ExpiresIn, response.RefreshToken);
            }

            return response;
        }

        public async Task RevokeRefreshTokenAsync()
        {
            var tokens = await _userTokenStore.GetTokenAsync(_httpContextAccessor.HttpContext.User);

            if (!string.IsNullOrEmpty(tokens.refreshToken))
            {
                await _tokenEndpointService.RevokeRefreshTokenAsync(tokens.refreshToken);
            }
        }
    }
}