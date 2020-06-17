// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

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
        /// <param name="context"></param>
        /// <param name="forceRenewal">If set to true, the cached user token is ignored, and a new one gets requested. Default to false.</param>
        /// <returns></returns>
        public static async Task<string> GetUserAccessTokenAsync(this HttpContext context, bool forceRenewal = false)
        {
            var service = context.RequestServices.GetRequiredService<IAccessTokenManagementService>();

            return await service.GetUserAccessTokenAsync(context.User, forceRenewal);
        }

        /// <summary>
        /// Returns an access token for the standard client or a named client
        /// </summary>
        /// <param name="context"></param>
        /// <param name="clientName">Name of the client configuration (or null to use the standard client).</param>
        /// <param name="forceRenewal">If set to true, the cached access token is ignored, and a new one gets requested. Default to false.</param>
        /// <returns></returns>
        public static async Task<string> GetClientAccessTokenAsync(this HttpContext context, string clientName = AccessTokenManagementDefaults.DefaultTokenClientName, bool forceRenewal = false)
        {
            var service = context.RequestServices.GetRequiredService<IAccessTokenManagementService>();

            return await service.GetClientAccessTokenAsync(clientName, forceRenewal);
        }

        /// <summary>
        /// Revokes the current user refresh token
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static async Task RevokeUserRefreshTokenAsync(this HttpContext context)
        {
            var service = context.RequestServices.GetRequiredService<IAccessTokenManagementService>();
            var store = context.RequestServices.GetRequiredService<IUserTokenStore>();

            await service.RevokeRefreshTokenAsync(context.User);
            await store.ClearTokenAsync(context.User);
        }
    }
}