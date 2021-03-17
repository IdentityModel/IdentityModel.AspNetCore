// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Authentication
{
    /// <summary>
    /// Extensions methods for HttpContext for token management
    /// </summary>
    public static class TokenManagementHttpContextExtensions
    {
        /// <summary>
        /// Returns (and refreshes if needed) the current access token for the logged on user
        /// </summary>
        /// <param name="httpContext">The HTTP context</param>
        /// <param name="parameters">Extra optional parameters</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns></returns>
        public static async Task<string> GetUserAccessTokenAsync(this HttpContext httpContext, UserAccessTokenParameters parameters = null, CancellationToken cancellationToken = default)
        {
            var service = httpContext.RequestServices.GetRequiredService<IUserAccessTokenManagementService>();

            return await service.GetUserAccessTokenAsync(httpContext.User, parameters, cancellationToken);
        }

        /// <summary>
        /// Returns an access token for the standard client or a named client
        /// </summary>
        /// <param name="httpContext">The HTTP context</param>
        /// <param name="clientName">Name of the client configuration (or null to use the standard client).</param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns></returns>
        public static async Task<string> GetClientAccessTokenAsync(
            this HttpContext httpContext, 
            string clientName = AccessTokenManagementDefaults.DefaultTokenClientName, 
            ClientAccessTokenParameters parameters = null,
            CancellationToken cancellationToken = default)
        {
            var service = httpContext.RequestServices.GetRequiredService<IClientAccessTokenManagementService>();

            return await service.GetClientAccessTokenAsync(clientName, parameters, cancellationToken);
        }

        /// <summary>
        /// Revokes the current user refresh token
        /// </summary>
        /// <param name="httpContext">The HTTP context</param>
        /// <param name="parameters">Extra optional parameters</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns></returns>
        public static async Task RevokeUserRefreshTokenAsync(
            this HttpContext httpContext, 
            UserAccessTokenParameters parameters = null, 
            CancellationToken cancellationToken = default)
        {
            var service = httpContext.RequestServices.GetRequiredService<IUserAccessTokenManagementService>();
            var store = httpContext.RequestServices.GetRequiredService<IUserTokenStore>();

            await service.RevokeRefreshTokenAsync(httpContext.User, parameters, cancellationToken);
            await store.ClearTokenAsync(httpContext.User, parameters);
        }
    }
}