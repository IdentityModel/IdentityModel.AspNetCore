// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using IdentityModel.Client;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Additional optional parameters for a user access token request
    /// </summary>
    public class UserAccessTokenParameters
    {
        public string SchemeName { get; set; }

        public bool ForceRenewal { get; set; }

        public string Resource { get; set; }

        public Parameters Context { get; set; } = new Parameters();
    }
}