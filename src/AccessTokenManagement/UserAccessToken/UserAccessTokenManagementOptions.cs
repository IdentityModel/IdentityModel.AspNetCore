using System;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Options for user access token management
    /// </summary>
    public class UserAccessTokenManagementOptions
    {
        /// <summary>
        /// Name of the authentication scheme to use for deriving token service configuration
        /// (will fall back to configured default challenge scheme if not set)
        /// </summary>
        public string? SchemeName { get; set; }

        /// <summary>
        /// Timespan that specifies how long before expiration, the token should be refreshed (defaults to 1 minute)
        /// </summary>
        public TimeSpan RefreshBeforeExpiration { get; set; } = TimeSpan.FromMinutes(1);
    }
}