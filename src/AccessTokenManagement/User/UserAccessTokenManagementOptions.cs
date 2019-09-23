// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Options for the token management services
    /// </summary>
    public class UserAccessTokenManagementOptions
    {
        /// <summary>
        /// Name of the authentication scheme to use for the token operations
        /// </summary>
        public string Scheme { get; set; }

        /// <summary>
        /// Timespan that specifies how long before expiration, the token should be refreshed
        /// </summary>
        public TimeSpan RefreshBeforeExpiration { get; set; } = TimeSpan.FromMinutes(1);
    }
}