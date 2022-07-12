// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Token store using the ASP.NET Core authentication session
    /// </summary>
    public class AuthenticationSessionUserAccessTokenStore : IUserAccessTokenStore
    {
        private const string TokenPrefix = ".Token.";
        private const string TokenNamesKey = ".TokenNames";

        private readonly IHttpContextAccessor _contextAccessor;
        private readonly ILogger<AuthenticationSessionUserAccessTokenStore> _logger;
        private readonly UserAccessTokenManagementOptions _options;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="contextAccessor"></param>
        /// <param name="logger"></param>
        /// <param name="options"></param>
        public AuthenticationSessionUserAccessTokenStore(
            IHttpContextAccessor contextAccessor,
            ILogger<AuthenticationSessionUserAccessTokenStore> logger, UserAccessTokenManagementOptions options)
        {
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));
            _logger = logger;
            _options = options;
        }

        /// <inheritdoc/>
        public async Task<UserAccessToken?> GetTokenAsync(
            ClaimsPrincipal user,
            UserAccessTokenParameters? parameters = null)
        {
            parameters ??= new UserAccessTokenParameters();
            var result = await _contextAccessor!.HttpContext!.AuthenticateAsync(parameters.SignInScheme);

            if (!result.Succeeded)
            {
                _logger.LogInformation("Cannot authenticate scheme: {scheme}", parameters.SignInScheme ?? "default signin scheme");

                return null;
            }

            if (result.Properties == null)
            {
                _logger.LogInformation("Authentication result properties are null for scheme: {scheme}",
                    parameters.SignInScheme ?? "default signin scheme");

                return null;
            }

            var tokens = result.Properties.Items.Where(i => i.Key.StartsWith(TokenPrefix)).ToList();
            if (!tokens.Any())
            {
                _logger.LogInformation("No tokens found in cookie properties. SaveTokens must be enabled for automatic token refresh.");

                return null;
            }

            var tokenName = $"{TokenPrefix}{OpenIdConnectParameterNames.AccessToken}";
            if (!string.IsNullOrEmpty(parameters.Resource))
            {
                tokenName += $"::{parameters.Resource}";
            }

            var expiresName = $"{TokenPrefix}expires_at"; string? refreshToken = null;
            string? accessToken = null;
            string? expiresAt = null;
            if (!string.IsNullOrEmpty(parameters.Resource))
            {
                expiresName += $"::{parameters.Resource}";
            }

            const string refreshTokenName = $"{TokenPrefix}{OpenIdConnectParameterNames.RefreshToken}";

            if (AppendChallengeSchemeToTokenNames(parameters))
            {
                refreshToken = tokens
                        .SingleOrDefault(t => t.Key == $"{refreshTokenName}||{parameters.ChallengeScheme}").Value;
                accessToken = tokens.SingleOrDefault(t => t.Key == $"{tokenName}||{parameters.ChallengeScheme}")
                    .Value;
                expiresAt = tokens.SingleOrDefault(t => t.Key == $"{expiresName}||{parameters.ChallengeScheme}")
                    .Value;
            }

            refreshToken ??= tokens.SingleOrDefault(t => t.Key == $"{refreshTokenName}").Value;
            accessToken ??= tokens.SingleOrDefault(t => t.Key == $"{tokenName}").Value;
            expiresAt ??= tokens.SingleOrDefault(t => t.Key == $"{expiresName}").Value;

            DateTimeOffset? dtExpires = null;
            if (expiresAt != null)
            {
                dtExpires = DateTimeOffset.Parse(expiresAt, CultureInfo.InvariantCulture);
            }

            return new UserAccessToken
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = dtExpires
            };
        }

        /// <inheritdoc/>
        public async Task StoreTokenAsync(
            ClaimsPrincipal user,
            string accessToken,
            DateTimeOffset expiration,
            string? refreshToken = null,
            UserAccessTokenParameters? parameters = null)
        {
            parameters ??= new UserAccessTokenParameters();
            var result = await _contextAccessor!.HttpContext!.AuthenticateAsync(parameters.SignInScheme)!;

            if (result is not { Succeeded: true })
            {
                throw new Exception("Can't store tokens. User is anonymous");
            }

            // in case you want to filter certain claims before re-issuing the authentication session
            var transformedPrincipal = await FilterPrincipalAsync(result.Principal!);

            var expiresName = "expires_at";
            if (!string.IsNullOrEmpty(parameters.Resource))
            {
                expiresName += $"::{parameters.Resource}";
            }

            var tokenName = OpenIdConnectParameterNames.AccessToken;
            if (!string.IsNullOrEmpty(parameters.Resource))
            {
                tokenName += $"::{parameters.Resource}";
            }

            var refreshTokenName = $"{OpenIdConnectParameterNames.RefreshToken}";

            if (AppendChallengeSchemeToTokenNames(parameters))
            {
                refreshTokenName += $"||{parameters.ChallengeScheme}";
                tokenName += $"||{parameters.ChallengeScheme}";
                expiresName += $"||{parameters.ChallengeScheme}";
            }

            result.Properties!.Items[$"{TokenPrefix}{tokenName}"] = accessToken;
            result.Properties!.Items[$"{TokenPrefix}{expiresName}"] = expiration.ToString("o", CultureInfo.InvariantCulture);

            if (refreshToken != null)
            {
                if (!result.Properties.UpdateTokenValue(refreshTokenName, refreshToken))
                {
                    result.Properties.Items[$"{TokenPrefix}{refreshTokenName}"] = refreshToken;
                }
            }

            var options = _contextAccessor!.HttpContext!.RequestServices.GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>();
            var schemeProvider = _contextAccessor.HttpContext.RequestServices.GetRequiredService<IAuthenticationSchemeProvider>();
            var scheme = parameters.SignInScheme ?? (await schemeProvider.GetDefaultSignInSchemeAsync())?.Name;
            var cookieOptions = options.Get(scheme);

            if (result.Properties.AllowRefresh == true ||
                (result.Properties.AllowRefresh == null && cookieOptions.SlidingExpiration))
            {
                // this will allow the cookie to be issued with a new issued (and thus a new expiration)
                result.Properties.IssuedUtc = null;
                result.Properties.ExpiresUtc = null;
            }

            result.Properties.Items.Remove(TokenNamesKey);
            result.Properties.Items.Add(new KeyValuePair<string, string?>(TokenNamesKey, string.Join(";", result.Properties.Items.Select(t => t.Key).ToList())));

            await _contextAccessor.HttpContext.SignInAsync(parameters.SignInScheme, transformedPrincipal, result.Properties);

        }

        /// <inheritdoc/>
        public Task ClearTokenAsync(
            ClaimsPrincipal user,
            UserAccessTokenParameters? parameters = null)
        {
            // todo
            return Task.CompletedTask;
        }

        /// <summary>
        /// Allows transforming the principal before re-issuing the authentication session
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        protected virtual Task<ClaimsPrincipal> FilterPrincipalAsync(ClaimsPrincipal principal)
        {
            return Task.FromResult(principal);
        }

        /// <summary>
        /// Confirm application has opted in to UseChallengeSchemeScopedTokens and a ChallengeScheme is provided upon storage and retrieval of tokens.
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        protected virtual bool AppendChallengeSchemeToTokenNames(UserAccessTokenParameters parameters)
        {
            return _options.UseChallengeSchemeScopedTokens && !string.IsNullOrEmpty(parameters!.ChallengeScheme);
        }
    }
}