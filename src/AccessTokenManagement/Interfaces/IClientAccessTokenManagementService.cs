// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Abstraction for managing client access tokens
    /// </summary>
    public interface IClientAccessTokenManagementService
    {
        /// <summary>
        /// Returns either a cached or a new access token for a given client configuration or the default client
        /// </summary>
        /// <param name="clientName">Name of the client configuration, or default is omitted.</param>
        /// <param name="parameters">Optional parameters.</param>
        /// <param name="cancellationToken">A cancellation token to cancel the operation</param>
        /// <returns>The access token or null if the no token can be requested.</returns>
        Task<string> GetClientAccessTokenAsync(
            string clientName = AccessTokenManagementDefaults.DefaultTokenClientName, 
            ClientAccessTokenParameters parameters = null, 
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a client access token from the cache
        /// </summary>
        /// <param name="clientName">Name of the client configuration, or default is omitted.</param>
        /// <param name="parameters">Optional parameters.</param>
        /// <param name="cancellationToken">A cancellation token to cancel operation.</param>
        /// <returns>The access token or null if the no token can be requested.</returns>
        Task DeleteClientAccessTokenAsync(
            string clientName = AccessTokenManagementDefaults.DefaultTokenClientName, 
            ClientAccessTokenParameters parameters = null, 
            CancellationToken cancellationToken = default);
    }
}