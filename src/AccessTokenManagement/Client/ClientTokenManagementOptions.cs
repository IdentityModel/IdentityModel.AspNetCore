using IdentityModel.Client;
using System.Collections.Generic;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    public class ClientTokenManagementOptions
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
