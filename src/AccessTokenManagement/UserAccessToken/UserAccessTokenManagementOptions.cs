using System;
using IdentityModel.Client;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Options for user access token management
    /// </summary>
    public class UserAccessTokenManagementOptions
    {
        /// <summary>
        /// Default client credential style to use when requesting tokens
        /// </summary>
        public ClientCredentialStyle ClientCredentialStyle { get; set; } =
            ClientCredentialStyle.PostBody;
        
        /// <summary>
        /// Name of the authentication scheme to use for deriving token service configuration
        /// (will fall back to configured default challenge scheme if not set)
        /// </summary>
        public string? SchemeName { get; set; }

        /// <summary>
        /// Boolean to set whether tokens added to a session should be challenge-scheme-specific.
        /// The default is 'false'.
        /// </summary>
        public bool UseChallengeSchemeScopedTokens { get; set; }

        /// <summary>
        /// Timespan that specifies how long before expiration, the token should be refreshed (defaults to 1 minute)
        /// </summary>
        public TimeSpan RefreshBeforeExpiration { get; set; } = TimeSpan.FromMinutes(1);
    }
}