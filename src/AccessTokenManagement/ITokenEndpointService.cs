// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using IdentityModel.Client;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Abstraction for token endpoint operations
    /// </summary>
    public interface ITokenEndpointService
    {
        /// <summary>
        /// Refreshes a user access token.
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        Task<TokenResponse> RefreshUserAccessTokenAsync(string refreshToken);
        
        /// <summary>
        /// Requests a client access token.
        /// </summary>
        /// <param name="clientName"></param>
        /// <returns></returns>
        Task<TokenResponse> RequestClientAccessToken(string clientName = null);
        
        /// <summary>
        /// Revokes a refresh token.
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        Task<TokenRevocationResponse> RevokeRefreshTokenAsync(string refreshToken);
    }
}