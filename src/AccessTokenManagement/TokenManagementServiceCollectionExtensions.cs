// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.AspNetCore.AccessTokenManagement;
using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        /// <param name="options"></param>
        /// <returns></returns>
        public static TokenManagementBuilder AddAccessTokenManagement(this IServiceCollection services, Action<AccessTokenManagementOptions> options = null)
        {
            if (options != null)
            {
                services.Configure(options);
            }

            services.AddHttpContextAccessor();
            services.AddAuthentication();

#if NETCOREAPP3_1
            services.AddDistributedMemoryCache();
#endif

            services.TryAddTransient<IAccessTokenManagementService, AccessTokenManagementService>();
            services.TryAddTransient<ITokenClientConfigurationService, DefaultTokenClientConfigurationService>();
            services.TryAddTransient<ITokenEndpointService, TokenEndpointService>();

            services.AddHttpClient(AccessTokenManagementDefaults.BackChannelHttpClientName);

            services.AddTransient<UserAccessTokenHandler>();
            services.AddTransient<ClientAccessTokenHandler>();

            services.TryAddTransient<IUserTokenStore, AuthenticationSessionUserTokenStore>();
            services.TryAddTransient<IClientAccessTokenCache, ClientAccessTokenCache>();

            return new TokenManagementBuilder(services);
        }

        /// <summary>
        /// Adds a named HTTP client for the factory that automatically sends the current user access token
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
                    .AddUserAccessTokenHandler();
            }

            return services.AddHttpClient(name)
                .AddUserAccessTokenHandler();
        }

        /// <summary>
        /// Adds a named HTTP client for the factory that automatically sends the a client access token
        /// </summary>
        /// <param name="services"></param>
        /// <param name="clientName">The name of the client.</param>
        /// <param name="tokenClientName">The name of the token client.</param>
        /// <param name="configureClient">Additional configuration.</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddClientAccessTokenClient(this IServiceCollection services, string clientName, string tokenClientName = AccessTokenManagementDefaults.DefaultTokenClientName, Action<HttpClient> configureClient = null)
        {
            if (configureClient != null)
            {
                return services.AddHttpClient(clientName, configureClient)
                    .AddClientAccessTokenHandler(tokenClientName);
            }

            return services.AddHttpClient(clientName)
                .AddClientAccessTokenHandler(tokenClientName);
        }

        /// <summary>
        /// Adds the client access token handler to an HttpClient
        /// </summary>
        /// <param name="httpClientBuilder"></param>
        /// <param name="tokenClientName"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddClientAccessTokenHandler(this IHttpClientBuilder httpClientBuilder, string tokenClientName = AccessTokenManagementDefaults.DefaultTokenClientName)
        {
            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var accessTokenManagementService = provider.GetRequiredService<IAccessTokenManagementService>();

                return new ClientAccessTokenHandler(accessTokenManagementService, tokenClientName);
            });
        }

        /// <summary>
        /// Adds the user access token handler to an HttpClient
        /// </summary>
        /// <param name="httpClientBuilder"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddUserAccessTokenHandler(this IHttpClientBuilder httpClientBuilder)
        {
            return httpClientBuilder.AddHttpMessageHandler<UserAccessTokenHandler>();
        }
    }
}