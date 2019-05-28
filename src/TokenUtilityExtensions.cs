using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore
{
    public static class TokenUtilityHttpContextExtensions
    {
        static readonly ConcurrentDictionary<string, Task<string>> _dictionary = 
            new ConcurrentDictionary<string, Task<string>>();

        public static async Task<string> GetAccessTokenAsync(this HttpContext context)
        {
            var store = context.RequestServices.GetRequiredService<ITokenStore>();
            var clock = context.RequestServices.GetRequiredService<ISystemClock>();
            var options = context.RequestServices.GetRequiredService<IOptions<TokenUtilityOptions>>();

            var tokens = await store.GetTokenAsync(context.User);

            var dtRefresh = tokens.expiration.Subtract(options.Value.RefreshBeforeExpiration);
            if (dtRefresh < clock.UtcNow)
            {
                string accessToken = null;

                try
                {
                    var source = new TaskCompletionSource<string>();
                    var accessTokenTask = _dictionary.GetOrAdd(tokens.refreshToken, source.Task);

                    if (accessTokenTask == source.Task)
                    {
                        var refreshed = await context.RefreshAccessTokenAsync();
                        accessToken = refreshed.accessToken;

                        source.SetResult(accessToken);
                    }
                    else
                    {
                        accessToken = await accessTokenTask;
                    }
                }
                finally
                {
                    _dictionary.TryRemove(tokens.refreshToken, out _);
                }

                return accessToken;
            }

            return tokens.accessToken;
        }

        public static async Task<(string accessToken, int expiresIn, string refreshToken)> RefreshAccessTokenAsync(this HttpContext context, string refreshToken)
        {
            var service = context.RequestServices.GetRequiredService<TokenEndpointService>();
            var result = await service.RefreshTokenAsync(refreshToken);

            if (!result.IsError)
            {
                return (result.AccessToken, result.ExpiresIn, result.RefreshToken);
            }

            throw new System.Exception(result.Error);
        }

        public static async Task<(string accessToken, string refreshToken)> RefreshAccessTokenAsync(this HttpContext context)
        {
            var store = context.RequestServices.GetRequiredService<ITokenStore>();

            var tokens = await store.GetTokenAsync(context.User);
            var result = await context.RefreshAccessTokenAsync(tokens.refreshToken);

            await store.StoreTokenAsync(context.User, result.accessToken, result.expiresIn, result.refreshToken);
            return (result.accessToken, result.refreshToken);
        }

        public static async Task RevokeRefreshTokenAsync(this HttpContext context, string refreshToken)
        {
            var service = context.RequestServices.GetRequiredService<TokenEndpointService>();
            await service.RevokeTokenAsync(refreshToken);
        }
    }
}