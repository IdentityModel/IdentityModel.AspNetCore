// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Implements basic token management logic
    /// </summary>
    public class AccessTokenManagementService : IAccessTokenManagementService
    {
        static readonly ConcurrentDictionary<string, Lazy<Task<string>>> UserRefreshDictionary =
            new ConcurrentDictionary<string, Lazy<Task<string>>>();

        static readonly ConcurrentDictionary<string, Lazy<Task<string>>> ClientTokenRequestDictionary =
            new ConcurrentDictionary<string, Lazy<Task<string>>>();

        private readonly IUserTokenStore _userTokenStore;
        private readonly ISystemClock _clock;
        private readonly AccessTokenManagementOptions _options;
        private readonly ITokenEndpointService _tokenEndpointService;
        private readonly IClientAccessTokenCache _clientAccessTokenCache;
        private readonly ILogger<AccessTokenManagementService> _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="userTokenStore"></param>
        /// <param name="clock"></param>
        /// <param name="options"></param>
        /// <param name="tokenEndpointService"></param>
        /// <param name="clientAccessTokenCache"></param>
        /// <param name="logger"></param>
        public AccessTokenManagementService(
            IUserTokenStore userTokenStore,
            ISystemClock clock,
            IOptions<AccessTokenManagementOptions> options,
            ITokenEndpointService tokenEndpointService,
            IClientAccessTokenCache clientAccessTokenCache,
            ILogger<AccessTokenManagementService> logger)
        {
            _userTokenStore = userTokenStore;
            _clock = clock;
            _options = options.Value;
            _tokenEndpointService = tokenEndpointService;
            _clientAccessTokenCache = clientAccessTokenCache;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<string> GetClientAccessTokenAsync(
            string clientName = AccessTokenManagementDefaults.DefaultTokenClientName, 
            bool forceRenewal = false,
            CancellationToken cancellationToken = default)
        {
            if (forceRenewal == false)
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
                        var response = await _tokenEndpointService.RequestClientAccessToken(clientName, cancellationToken);

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
        public Task DeleteClientAccessTokenAsync(string clientName = AccessTokenManagementDefaults.DefaultTokenClientName, CancellationToken cancellationToken = default)
        {
            return _clientAccessTokenCache.DeleteAsync(clientName, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<string> GetUserAccessTokenAsync(ClaimsPrincipal user, string resource = null, bool forceRenewal = false, CancellationToken cancellationToken = default)
        {
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return null;
            }

            var userName = user.FindFirst(JwtClaimTypes.Name)?.Value ?? user.FindFirst(JwtClaimTypes.Subject)?.Value ?? "unknown";
            var userToken = await _userTokenStore.GetTokenAsync(user, resource);

            if (userToken == null)
            {
                _logger.LogDebug("No token data found in user token store.");
                return null;
            }
            
            if (userToken.AccessToken.IsPresent() && userToken.RefreshToken.IsMissing())
            {
                _logger.LogDebug("No refresh token found in user token store for user {user} / resource {resource}. Returning current access token.", userName, resource ?? "default");
                return userToken.AccessToken;
            }

            if (userToken.AccessToken.IsMissing() && userToken.RefreshToken.IsPresent())
            {
                _logger.LogDebug(
                    "No access token found in user token store for user {user} / resource {resource}. Trying to refresh.",
                    userName, resource ?? "default");
            }

            DateTimeOffset dtRefresh = DateTimeOffset.MinValue;
            if (userToken.Expiration.HasValue)
            {
                dtRefresh = userToken.Expiration.Value.Subtract(_options.User.RefreshBeforeExpiration);
            }
            
            if (dtRefresh < _clock.UtcNow || forceRenewal == true)
            {
                _logger.LogDebug("Token for user {user} needs refreshing.", userName);

                try
                {
                    return await UserRefreshDictionary.GetOrAdd(userToken.RefreshToken, _ =>
                    {
                        return new Lazy<Task<string>>(async () =>
                        {
                            var refreshed = await RefreshUserAccessTokenAsync(user, resource, cancellationToken);
                            return refreshed.AccessToken;
                        });
                    }).Value;
                }
                finally
                {
                    UserRefreshDictionary.TryRemove(userToken.RefreshToken, out _);
                }
            }

            return userToken.AccessToken;
        }

        /// <inheritdoc/>
        public async Task RevokeRefreshTokenAsync(ClaimsPrincipal user, CancellationToken cancellationToken = default)
        {
            var userToken = await _userTokenStore.GetTokenAsync(user);

            if (!string.IsNullOrEmpty(userToken?.RefreshToken))
            {
                var response = await _tokenEndpointService.RevokeRefreshTokenAsync(userToken.RefreshToken, cancellationToken);

                if (response.IsError)
                {
                    _logger.LogError("Error revoking refresh token. Error = {error}", response.Error);
                }
            }
        }

        internal async Task<TokenResponse> RefreshUserAccessTokenAsync(ClaimsPrincipal user, string resource = null, CancellationToken cancellationToken = default)
        {
            var userToken = await _userTokenStore.GetTokenAsync(user);
            var response = await _tokenEndpointService.RefreshUserAccessTokenAsync(userToken.RefreshToken, resource, cancellationToken);

            if (!response.IsError)
            {
                var expiration = DateTime.UtcNow + TimeSpan.FromSeconds(response.ExpiresIn);

                await _userTokenStore.StoreTokenAsync(user, response.AccessToken, expiration, response.RefreshToken, resource);
            }
            else
            {
                _logger.LogError("Error refreshing access token. Error = {error}", response.Error);
            }

            return response;
        }
    }
}