// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.Client;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Abstraction for managing user and client accesss tokens
    /// </summary>
    public interface IAccessTokenManagementService
    {
        Task<string> GetUserAccessTokenAsync();
        Task RevokeRefreshTokenAsync();
        Task<TokenResponse> RefreshUserAccessTokenAsync();

        /// <summary>
        /// Returns either a cached of a new access token for a given client configuration or the default client
        /// </summary>
        /// <param name="clientName">Name of the client configuration, or default is omitted.</param>
        /// <returns>The access token.</returns>
        Task<string> GetClientAccessTokenAsync(string clientName = null);
    }
}