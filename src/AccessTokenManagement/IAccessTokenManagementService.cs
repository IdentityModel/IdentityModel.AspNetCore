// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Abstraction for managing user and client access tokens
    /// </summary>
    public interface IAccessTokenManagementService
    {
        /// <summary>
        /// Returns the user access token. If the current token is expired, it will try to refresh it.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> GetUserAccessTokenAsync(ClaimsPrincipal user, UserAccessTokenParameters parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Revokes the current refresh token
        /// </summary>
        /// <returns></returns>
        Task RevokeRefreshTokenAsync(ClaimsPrincipal user, UserAccessTokenParameters parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Returns either a cached or a new access token for a given client configuration or the default client
        /// </summary>
        /// <param name="clientName">Name of the client configuration, or default is omitted.</param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
        /// <returns>The access token or null if the no token can be requested.</returns>
        Task<string> GetClientAccessTokenAsync(string clientName = AccessTokenManagementDefaults.DefaultTokenClientName, ClientAccessTokenParameters parameters = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a client access token from the cache
        /// </summary>
        /// <param name="clientName">Name of the client configuration, or default is omitted.</param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The access token or null if the no token can be requested.</returns>
        Task DeleteClientAccessTokenAsync(string clientName = AccessTokenManagementDefaults.DefaultTokenClientName, ClientAccessTokenParameters parameters = null, CancellationToken cancellationToken = default);
    }
}