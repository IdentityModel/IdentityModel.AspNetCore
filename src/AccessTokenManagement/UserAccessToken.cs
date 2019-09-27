// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Models a user access token
    /// </summary>
    public class UserAccessToken
    {
        /// <summary>
        /// The access token
        /// </summary>
        public string AccessToken { get; set; }
        
        /// <summary>
        /// The access token expiration
        /// </summary>
        public DateTimeOffset Expiration { get; set; }
        
        /// <summary>
        /// The refresh token
        /// </summary>
        public string RefreshToken { get; set; }
    }
}