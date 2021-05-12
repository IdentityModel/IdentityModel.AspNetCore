using System;
using System.Collections.Concurrent;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Implements basic token management logic
    /// </summary>
    public class UserAccessAccessTokenManagementService : IUserAccessTokenManagementService
    {
        private static readonly ConcurrentDictionary<string, Lazy<Task<string>>> UserRefreshDictionary = new();
        
        private readonly IUserAccessTokenStore _userAccessTokenStore;
        private readonly ISystemClock _clock;
        private readonly UserAccessTokenManagementOptions _options;
        private readonly ITokenEndpointService _tokenEndpointService;
        private readonly ILogger<UserAccessAccessTokenManagementService> _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="userAccessTokenStore"></param>
        /// <param name="clock"></param>
        /// <param name="options"></param>
        /// <param name="tokenEndpointService"></param>
        /// <param name="logger"></param>
        public UserAccessAccessTokenManagementService(
            IUserAccessTokenStore userAccessTokenStore,
            ISystemClock clock,
            UserAccessTokenManagementOptions options,
            ITokenEndpointService tokenEndpointService,
            ILogger<UserAccessAccessTokenManagementService> logger)
        {
            _userAccessTokenStore = userAccessTokenStore;
            _clock = clock;
            _options = options;
            _tokenEndpointService = tokenEndpointService;
            _logger = logger;
        }
        
        /// <inheritdoc/>
        public async Task<string> GetUserAccessTokenAsync(
            ClaimsPrincipal user, 
            UserAccessTokenParameters parameters = null, 
            CancellationToken cancellationToken = default)
        {
            parameters ??= new UserAccessTokenParameters();
            
            if (user == null || !user.Identity.IsAuthenticated)
            {
                return null;
            }

            var userName = user.FindFirst(JwtClaimTypes.Name)?.Value ?? user.FindFirst(JwtClaimTypes.Subject)?.Value ?? "unknown";
            var userToken = await _userAccessTokenStore.GetTokenAsync(user, parameters);

            if (userToken == null)
            {
                _logger.LogDebug("No token data found in user token store for user {user}.", userName);
                return null;
            }
            
            if (userToken.AccessToken.IsPresent() && userToken.RefreshToken.IsMissing())
            {
                _logger.LogDebug("No refresh token found in user token store for user {user} / resource {resource}. Returning current access token.", userName, parameters.Resource ?? "default");
                return userToken.AccessToken;
            }

            if (userToken.AccessToken.IsMissing() && userToken.RefreshToken.IsPresent())
            {
                _logger.LogDebug(
                    "No access token found in user token store for user {user} / resource {resource}. Trying to refresh.",
                    userName, parameters.Resource ?? "default");
            }

            var dtRefresh = DateTimeOffset.MinValue;
            if (userToken.Expiration.HasValue)
            {
                dtRefresh = userToken.Expiration.Value.Subtract(_options.RefreshBeforeExpiration);
            }
            
            if (dtRefresh < _clock.UtcNow || parameters.ForceRenewal)
            {
                _logger.LogDebug("Token for user {user} needs refreshing.", userName);

                try
                {
                    return await UserRefreshDictionary.GetOrAdd(userToken.RefreshToken, _ =>
                    {
                        return new Lazy<Task<string>>(async () =>
                        {
                            var refreshed = await RefreshUserAccessTokenAsync(user, parameters, cancellationToken);
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
        public async Task RevokeRefreshTokenAsync(
            ClaimsPrincipal user, 
            UserAccessTokenParameters parameters = null, 
            CancellationToken cancellationToken = default)
        {
            parameters ??= new UserAccessTokenParameters();
            var userToken = await _userAccessTokenStore.GetTokenAsync(user, parameters);

            if (!string.IsNullOrEmpty(userToken?.RefreshToken))
            {
                var response = await _tokenEndpointService.RevokeRefreshTokenAsync(userToken.RefreshToken, parameters, cancellationToken);

                if (response.IsError)
                {
                    _logger.LogError("Error revoking refresh token. Error = {error}", response.Error);
                }
            }
        }

        private async Task<TokenResponse> RefreshUserAccessTokenAsync(ClaimsPrincipal user, UserAccessTokenParameters parameters, CancellationToken cancellationToken = default)
        {
            var userToken = await _userAccessTokenStore.GetTokenAsync(user, parameters);
            var response = await _tokenEndpointService.RefreshUserAccessTokenAsync(userToken.RefreshToken, parameters, cancellationToken);

            if (!response.IsError)
            {
                var expiration = DateTime.UtcNow + TimeSpan.FromSeconds(response.ExpiresIn);

                await _userAccessTokenStore.StoreTokenAsync(user, response.AccessToken, expiration, response.RefreshToken, parameters);
            }
            else
            {
                _logger.LogError("Error refreshing access token. Error = {error}", response.Error);
            }

            return response;
        }
    }
}