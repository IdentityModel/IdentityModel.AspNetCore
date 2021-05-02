// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace IdentityModel.AspNetCore.AccessTokenManagement
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