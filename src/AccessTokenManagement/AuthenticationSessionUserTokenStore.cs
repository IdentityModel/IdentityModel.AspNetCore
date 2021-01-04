// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Token store using the ASP.NET Core authentication session
    /// </summary>
    public class AuthenticationSessionUserTokenStore : IUserTokenStore
    {
        private readonly IHttpContextAccessor _contextAccessor;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="contextAccessor"></param>
        public AuthenticationSessionUserTokenStore(
            IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        /// <inheritdoc/>
        public async Task<UserAccessToken> GetTokenAsync(ClaimsPrincipal user, string resource)
        {
            var result = await _contextAccessor.HttpContext.AuthenticateAsync();

            if (!result.Succeeded)
            {
                return null;
            }

            var tokens = result.Properties.GetTokens().ToList();
            if (tokens == null || !tokens.Any())
            {
                throw new InvalidOperationException("No tokens found in cookie properties. SaveTokens must be enabled for automatic token refresh.");
            }
            
            string tokenName = OpenIdConnectParameterNames.AccessToken;
            if (!string.IsNullOrEmpty(resource))
            {
                tokenName += $"::{resource}";
            }

            string expiresName = "expires_at";
            if (!string.IsNullOrEmpty(resource))
            {
                expiresName += $"::{resource}";
            }

            var accessToken = tokens.SingleOrDefault(t => t.Name == tokenName);
            if (accessToken == null)
            {
                throw new InvalidOperationException("No access token found in cookie properties. An access token must be requested and SaveTokens must be enabled.");
            }
            
            var expiresAt = tokens.SingleOrDefault(t => t.Name == expiresName);
            if (expiresAt == null)
            {
                throw new InvalidOperationException("No expires_at value found in cookie properties.");
            }
            
            var dtExpires = DateTimeOffset.Parse(expiresAt.Value, CultureInfo.InvariantCulture);
            var refreshToken = tokens.SingleOrDefault(t => t.Name == OpenIdConnectParameterNames.RefreshToken);
            
            return new UserAccessToken
            {
                AccessToken = accessToken.Value,
                RefreshToken = refreshToken?.Value,
                Expiration = dtExpires
            };
            
        }

        /// <inheritdoc/>
        public async Task StoreTokenAsync(ClaimsPrincipal user, string accessToken, DateTimeOffset expiration, string refreshToken = null, string resource = null)
        {
            var result = await _contextAccessor.HttpContext.AuthenticateAsync();

            if (!result.Succeeded)
            {
                throw new Exception("Can't store tokens. User is anonymous");
            }

            string tokenName = OpenIdConnectParameterNames.AccessToken;
            if (!string.IsNullOrEmpty(resource))
            {
                tokenName += $"::{resource}";
            }

            string expiresName = "expires_at";
            if (!string.IsNullOrEmpty(resource))
            {
                expiresName += $"::{resource}";
            }
            
            result.Properties.UpdateTokenValue(tokenName, accessToken);
            result.Properties.UpdateTokenValue(expiresName, expiration.ToString("o", CultureInfo.InvariantCulture));
            
            if (refreshToken != null)
            {
                result.Properties.UpdateTokenValue(OpenIdConnectParameterNames.RefreshToken, refreshToken);
            }

            await _contextAccessor.HttpContext.SignInAsync(result.Principal, result.Properties);
        }

        /// <inheritdoc/>
        public Task ClearTokenAsync(ClaimsPrincipal user)
        {
            return Task.CompletedTask;
        }
    }
}