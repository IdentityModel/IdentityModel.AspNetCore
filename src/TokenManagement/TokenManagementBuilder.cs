// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace IdentityModel.AspNetCore
{
    /// <summary>
    /// Builder object for the token management services
    /// </summary>
    public class TokenManagementBuilder
    {
        /// <summary>
        /// The underlying service collection
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="services"></param>
        public TokenManagementBuilder(IServiceCollection services)
        {
            Services = services;
        }

        /// <summary>
        /// Configures the back-channel client
        /// </summary>
        /// <returns></returns>
        public IHttpClientBuilder ConfigureBackchannelHttpClient()
        {
            return Services.AddHttpClient<TokenEndpointService>();
        }

        /// <summary>
        /// Configures the back-channel client
        /// </summary>
        /// <returns></returns>
        public IHttpClientBuilder ConfigureBackchannelHttpClient(Action<HttpClient> configureClient)
        {
            return Services.AddHttpClient<TokenEndpointService>(configureClient);
        }
    }
}
