// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    public class ClientAccessToken
    {
        public string AccessToken { get; set; }
        public DateTimeOffset Expiration { get; set; }
    }
}