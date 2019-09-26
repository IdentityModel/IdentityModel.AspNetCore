// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.AspNetCore.AccessTokenManagement;
using System;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for IServiceCollection to register the token management services
    /// </summary>
    public static class TokenManagementServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the token management services to DI
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static TokenManagementBuilder AddAccessTokenManagement(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddTransient<IAccessTokenManagementService, AccessTokenManagementService>();
            services.AddHttpClient<ITokenEndpointService, TokenEndpointService>();

            services.AddTransient<UserAccessTokenHandler>();

            services.AddTransient<IUserTokenStore, AuthenticationSessionUserTokenStore>();

            return new TokenManagementBuilder(services);
        }

        /// <summary>
        /// Adds a named HTTP client for the factory that automatically sends the current access token
        /// </summary>
        /// <param name="services"></param>
        /// <param name="name">The name of the client.</param>
        /// <param name="configureClient">Additional configuration.</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddUserAccessTokenClient(this IServiceCollection services, string name, Action<HttpClient> configureClient = null)
        {
            if (configureClient != null)
            {
                return services.AddHttpClient(name, configureClient)
                    .AddHttpMessageHandler<UserAccessTokenHandler>();
            }

            return services.AddHttpClient(name)
                .AddHttpMessageHandler<UserAccessTokenHandler>();
        }
    }
}