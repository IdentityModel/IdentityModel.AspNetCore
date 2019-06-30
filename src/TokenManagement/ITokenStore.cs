using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore
{
    public interface ITokenStore
    {
        Task StoreTokenAsync(ClaimsPrincipal user, string accessToken, int expiresIn, string refreshToken);
        Task<(string accessToken, string refreshToken, DateTimeOffset expiration)> GetTokenAsync(ClaimsPrincipal user);
    }
}