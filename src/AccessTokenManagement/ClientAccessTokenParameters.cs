// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.Client;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Additional optional parameters for a client access token request
    /// </summary>
    public class ClientAccessTokenParameters
    {
        /// <summary>
        /// Force renewal of token.
        /// </summary>
        public bool ForceRenewal { get; set; }

        /// <summary>
        /// Specifies the resource parameter.
        /// </summary>
        public string Resource { get; set; }

        /// <summary>
        /// Additional context that might be relevant in the pipeline
        /// </summary>
        public Parameters Context { get; set; } = new Parameters();
    }
}