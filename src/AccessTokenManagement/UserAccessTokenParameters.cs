// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.Client;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Additional optional parameters for a user access token request
    /// </summary>
    public class UserAccessTokenParameters
    {
        /// <summary>
        /// Overrides the default sign-in scheme. This information may be used for state management.
        /// </summary>
        public string SignInScheme { get; set; }
        
        /// <summary>
        /// Overrides the default challenge scheme. This information may be used for deriving token service configuration.
        /// </summary>
        public string ChallengeScheme { get; set; }

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