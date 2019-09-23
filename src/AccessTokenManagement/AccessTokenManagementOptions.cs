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
        public UserOptions User { get; set; } = new UserOptions();
        public ClientOptions Client { get; set; } = new ClientOptions();

        public class UserOptions
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

        public class ClientOptions
        {
            /// <summary>
            /// Set the scheme name of an OpenID Connect handler, if the client configuration can be derived from it.
            /// This will be used as a default if no explicit clients are configured (and will fallback to the default challenge scheme if left empty).
            /// </summary>
            public string OidcSchemeClient { get; set; }

            /// <summary>
            /// Configures named client configurations for requesting client tokens.
            /// </summary>
            public IDictionary<string, TokenClientOptions> Clients { get; set; } = new Dictionary<string, TokenClientOptions>();
        }
    }
}