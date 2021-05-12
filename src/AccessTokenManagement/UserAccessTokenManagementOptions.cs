using System;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    public class UserAccessTokenManagementOptions
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
}