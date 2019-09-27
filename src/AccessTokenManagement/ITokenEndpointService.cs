// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using IdentityModel.Client;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    public interface ITokenEndpointService
    {
        Task<TokenResponse> RefreshUserAccessTokenAsync(string refreshToken);
        Task<TokenResponse> RequestClientAccessToken(string clientName = null);
        Task<TokenRevocationResponse> RevokeRefreshTokenAsync(string refreshToken);
    }
}