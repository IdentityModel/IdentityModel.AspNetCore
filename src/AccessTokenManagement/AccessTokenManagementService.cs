﻿using IdentityModel.Client;
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
        static readonly ConcurrentDictionary<string, Lazy<Task<string>>> _userRefreshDictionary =
            new ConcurrentDictionary<string, Lazy<Task<string>>>();

        private readonly IUserTokenStore _userTokenStore;
        private readonly ISystemClock _clock;
        private readonly AccessTokenManagementOptions _options;
        private readonly TokenEndpointService _tokenEndpointService;
        private readonly IClientAccessTokenCache _clientAccessTokenCache;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AccessTokenManagementService> _logger;

        public AccessTokenManagementService(
            IUserTokenStore userTokenStore,
            ISystemClock clock,
            IOptions<AccessTokenManagementOptions> options,
            TokenEndpointService tokenEndpointService,
            IClientAccessTokenCache clientAccessTokenCache,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AccessTokenManagementService> logger)
        {
            _userTokenStore = userTokenStore;
            _clock = clock;
            _options = options.Value;
            _tokenEndpointService = tokenEndpointService;
            _clientAccessTokenCache = clientAccessTokenCache;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<string> GetClientAccessTokenAsync(string clientName = null)
        {
            if (clientName is null) clientName = "default";

            var item = await _clientAccessTokenCache.GetAsync(clientName);
            if (item != null)
            {
                return item.AccessToken;
            }

            var response = await _tokenEndpointService.RequestClientAccessToken(clientName);

            if (response.IsError)
            {
                _logger.LogError("Error requesting access token for client {clientName}. Error = {error}", clientName, response.Error);
            }

            await _clientAccessTokenCache.SetAsync(clientName, response.AccessToken, response.ExpiresIn);
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
                    return await _userRefreshDictionary.GetOrAdd(userToken.RefreshToken, (string refreshToken) =>
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
                    _userRefreshDictionary.TryRemove(userToken.RefreshToken, out _);
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
            else
            {
                _logger.LogError("Error refreshing access token. Error = {error}", response.Error);
            }

            return response;
        }

        public async Task RevokeRefreshTokenAsync()
        {
            var userToken = await _userTokenStore.GetTokenAsync(_httpContextAccessor.HttpContext.User);

            if (!string.IsNullOrEmpty(userToken.RefreshToken))
            {
                var response = await _tokenEndpointService.RevokeRefreshTokenAsync(userToken.RefreshToken);

                if (response.IsError)
                {
                    _logger.LogError("Error revoking refresh token. Error = {error}", response.Error);
                }
            }
        }
    }
}