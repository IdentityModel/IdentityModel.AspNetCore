// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.Client;
using System;
using System.Collections.Generic;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Options for the token management services
    /// </summary>
    public class AccessTokenManagementOptions
    {
        /// <summary>
        /// Options for user access tokens
        /// </summary>
        public UserOptions User { get; set; } = new UserOptions();
        
        /// <summary>
        /// Options for client acccess tokens
        /// </summary>
        public ClientOptions Client { get; set; } = new ClientOptions();

        /// <summary>
        /// User access token options
        /// </summary>
        public class UserOptions
        {
            /// <summary>
            /// Name of the authentication scheme to use for the token operations
            /// </summary>
            public string Scheme { get; set; }

            /// <summary>
            /// Timespan that specifies how long before expiration, the token should be refreshed (defaults to 1 minute)
            /// </summary>
            public TimeSpan RefreshBeforeExpiration { get; set; } = TimeSpan.FromMinutes(1);
        }

        /// <summary>
        /// Client access token options
        /// </summary>
        public class ClientOptions
        {
            /// <summary>
            /// Sets the scheme name of an OpenID Connect handler, if the client configuration should be derived from it.
            /// This will be used as a default if no explicit clients are configured (and will fallback to the default challenge scheme if left empty).
            /// </summary>
            public string Scheme { get; set; }

            /// <summary>
            /// Scope values as space separated list to use when client configuration is inferred from OpenID Connect scheme.
            /// If not set, token request will omit scope parameter.
            /// </summary>
            public string Scope { get; set; }

            /// <summary>
            /// Used to prefix the cache key
            /// </summary>
            public string CacheKeyPrefix { get; set; } = "IdentityModel.AspNetCore.AccessTokenManagement:";

            /// <summary>
            /// Value to subtract from token lifetime for the cache entry lifetime (defaults to 60 seconds)
            /// </summary>
            public int CacheLifetimeBuffer { get; set; } = 60;

            /// <summary>
            /// Configures named client configurations for requesting client tokens.
            /// </summary>
            public IDictionary<string, ClientCredentialsTokenRequest> Clients { get; set; } = new Dictionary<string, ClientCredentialsTokenRequest>();
        }
    }
}