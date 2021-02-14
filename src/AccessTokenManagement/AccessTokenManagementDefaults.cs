﻿namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Default values
    /// </summary>
    public static class AccessTokenManagementDefaults
    {
        /// <summary>
        /// Name of the default client access token configuration
        /// </summary>
        public const string DefaultTokenClientName = "default";

        /// <summary>
        /// Name of the back-channel HTTP client
        /// </summary>
        public const string BackChannelHttpClientName = "IdentityModel.AspNetCore.AccessTokenManagement.TokenEndpointService";
        
        /// <summary>
        /// Name used to propagate access token parameters to HttpRequestMessage
        /// </summary>
        public const string AccessTokenParametersOptionsName = "IdentityModel.AspNetCore.AccessTokenParameters";
    }
}