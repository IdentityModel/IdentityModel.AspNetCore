using System.Collections.Generic;
using IdentityModel.Client;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Client access token options
    /// </summary>
    public class ClientAccessTokenManagementOptions
    {
        /// <summary>
        /// Options for the default client
        /// </summary>
        public DefaultClientOptions DefaultClient { get; set; } = new();
        
        /// <summary>
        /// Used to prefix the cache key
        /// </summary>
        public string CacheKeyPrefix { get; set; } = "IdentityModel.AspNetCore.AccessTokenManagement";

        /// <summary>
        /// Value to subtract from token lifetime for the cache entry lifetime (defaults to 60 seconds)
        /// </summary>
        public int CacheLifetimeBuffer { get; set; } = 60;

        /// <summary>
        /// Configures named client configurations for requesting client tokens.
        /// </summary>
        public IDictionary<string, ClientCredentialsTokenRequest> Clients { get; set; } = new Dictionary<string, ClientCredentialsTokenRequest>();

        /// <summary>
        /// Options for the default client (where the client configuration is inferred from the OpenID Connect handler)
        /// </summary>
        public class DefaultClientOptions
        {
            /// <summary>
            /// Sets the scheme name of an OpenID Connect handler, if the client configuration should be derived from it.
            /// This will be used as a default if no explicit clients are configured (and will fallback to the default challenge scheme if left empty).
            /// </summary>
            public string? Scheme { get; set; }

            /// <summary>
            /// Scope values as space separated list to use when client configuration is inferred from OpenID Connect scheme.
            /// If not set, token request will omit scope parameter.
            /// </summary>
            public string? Scope { get; set; }
            
            /// <summary>
            /// Resource value when client configuration is inferred from OpenID Connect scheme.
            /// If not set, token request will omit resource parameter.
            /// </summary>
            public string? Resource { get; set; }
        }
    }
}