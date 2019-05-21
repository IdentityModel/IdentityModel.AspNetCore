using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore
{
    public static class TokenUtilityExtensions
    {
        public static async Task<(string accessToken, string refreshToken)> RefreshAccessTokenAsync(this HttpContext context, string refreshToken)
        {
            var service = context.RequestServices.GetRequiredService<TokenEndpointService>();
            var result = await service.RefreshTokenAsync(refreshToken);

            if (!result.IsError)
            {
                return (result.AccessToken, result.RefreshToken);
            }

            return (null, null);
        }

        public static async Task<(string accessToken, string refreshToken)> RefreshAccessTokenAsync(this HttpContext context)
        {
            var service = context.RequestServices.GetRequiredService<TokenEndpointService>();
            var store = context.RequestServices.GetRequiredService<ITokenStore>();

            var tokens = await store.GetTokenAsync(context.User);
            var result = await service.RefreshTokenAsync(tokens.refreshToken);

            if (!result.IsError)
            {
                await store.StoreTokenAsync(context.User, result.AccessToken, result.ExpiresIn, result.RefreshToken);
                return (result.AccessToken, result.RefreshToken);
            }

            return (null, null);
        }

        public static async Task RevokeRefreshTokenAsync(this HttpContext context, string refreshToken)
        {
            var service = context.RequestServices.GetRequiredService<TokenEndpointService>();
            await service.RevokeTokenAsync(refreshToken);
        }

        public static async Task<string> GetAccessTokenAsync(this HttpContext context)
        {
            var store = context.RequestServices.GetRequiredService<ITokenStore>();

            return (await store.GetTokenAsync(context.User)).accessToken;
        }
    }
}