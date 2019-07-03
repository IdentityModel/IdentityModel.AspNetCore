using IdentityModel.AspNetCore;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication
{
    public static class TokenManagementHttpContextExtensions
    {
        static readonly ConcurrentDictionary<string, Lazy<Task<string>>> _dictionary = 
            new ConcurrentDictionary<string, Lazy<Task<string>>>();

        public static async Task<string> GetAccessTokenAsync(this HttpContext context)
        {
            var store = context.RequestServices.GetRequiredService<ITokenStore>();
            var clock = context.RequestServices.GetRequiredService<ISystemClock>();
            var options = context.RequestServices.GetRequiredService<IOptions<TokenManagementOptions>>();
            var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("IdentityModel.AspNetCore.TokenManagement");

            var tokens = await store.GetTokenAsync(context.User);

            var dtRefresh = tokens.expiration.Subtract(options.Value.RefreshBeforeExpiration);
            if (dtRefresh < clock.UtcNow)
            {
                logger.LogDebug("Token {token} needs refreshing.", tokens.accessToken);

                try
                {
                    return await _dictionary.GetOrAdd(tokens.refreshToken, (string refreshToken) =>
                    {
                        return new Lazy<Task<string>>(async () => {
                            var refreshed = await context.RefreshAccessTokenAsync();
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

        public static async Task<TokenResponse> RefreshAccessTokenAsync(this HttpContext context, string refreshToken)
        {
            var service = context.RequestServices.GetRequiredService<TokenEndpointService>();
            var response = await service.RefreshAccessTokenAsync(refreshToken);

            return response;
        }

        public static async Task<TokenResponse> RefreshAccessTokenAsync(this HttpContext context)
        {
            var store = context.RequestServices.GetRequiredService<ITokenStore>();

            var tokens = await store.GetTokenAsync(context.User);
            var response = await context.RefreshAccessTokenAsync(tokens.refreshToken);

            if (!response.IsError)
            {
                await store.StoreTokenAsync(context.User, response.AccessToken, response.ExpiresIn, response.RefreshToken);
            }

            return response;
        }

        public static async Task RevokeRefreshTokenAsync(this HttpContext context, string refreshToken)
        {
            var service = context.RequestServices.GetRequiredService<TokenEndpointService>();
            await service.RevokeTokenAsync(refreshToken);
        }
    }
}