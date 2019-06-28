using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore
{
    public class AuthenticationSessionTokenStore : ITokenStore
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<AuthenticationSessionTokenStore> _logger;

        public AuthenticationSessionTokenStore(
            IHttpContextAccessor contextAccessor,
            ILogger<AuthenticationSessionTokenStore> logger)
        {
            _contextAccessor = contextAccessor;
            _logger = logger;
        }

        public async Task<(string accessToken, string refreshToken, DateTimeOffset expiration)> GetTokenAsync(ClaimsPrincipal user)
        {
            var result = await _contextAccessor.HttpContext.AuthenticateAsync();

            var tokens = result.Properties.GetTokens();
            if (tokens == null || !tokens.Any())
            {
                throw new InvalidOperationException("No tokens found in cookie properties. SaveTokens must be enabled for automatic token refresh.");
            }

            var accessToken = tokens.SingleOrDefault(t => t.Name == OpenIdConnectParameterNames.AccessToken);
            if (accessToken == null)
            {
                throw new InvalidOperationException("No access token found in cookie properties. An access token must be requested and SaveTokens must be enabled.");
            }

            var refreshToken = tokens.SingleOrDefault(t => t.Name == OpenIdConnectParameterNames.RefreshToken);
            if (refreshToken == null)
            {
                throw new InvalidOperationException("No refresh token found in cookie properties. A refresh token must be requested and SaveTokens must be enabled.");
            }

            var expiresAt = tokens.SingleOrDefault(t => t.Name == "expires_at");
            if (expiresAt == null)
            {
                throw new InvalidOperationException("No expires_at value found in cookie properties.");
            }

            var dtExpires = DateTimeOffset.Parse(expiresAt.Value, CultureInfo.InvariantCulture);

            return (accessToken.Value, refreshToken.Value, dtExpires);
        }

        public async Task StoreTokenAsync(ClaimsPrincipal user, string accessToken, int expiresIn, string refreshToken)
        {
            var result = await _contextAccessor.HttpContext.AuthenticateAsync();

            result.Properties.UpdateTokenValue("access_token", accessToken);
            result.Properties.UpdateTokenValue("refresh_token", refreshToken);

            var newExpiresAt = DateTime.UtcNow + TimeSpan.FromSeconds(expiresIn);
            result.Properties.UpdateTokenValue("expires_at", newExpiresAt.ToString("o", CultureInfo.InvariantCulture));

            await _contextAccessor.HttpContext.SignInAsync(result.Principal, result.Properties);
        }
    }
}