// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.AspNetCore.AccessTokenManagement;
using System;
using System.Linq;
using System.Net.Http;
using IdentityModel.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for IServiceCollection to register the token management services
    /// </summary>
    public static class TokenManagementServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the token management services to DI using all default values
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns></returns>
        public static TokenManagementBuilder AddAccessTokenManagement(this IServiceCollection services)
        {
            CheckConfigMarker(services);
            
            var options = new AccessTokenManagementOptions();
            
            services.AddSingleton(options.Client);
            services.AddSingleton(options.User);

            services.AddUserAccessTokenManagementInternal();
            services.AddClientAccessTokenManagementInternal();
            
            return new TokenManagementBuilder(services);
        }
        
        /// <summary>
        /// Adds the token management services to DI
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configureAction">A delegate that is used to configure an <see cref="AccessTokenManagementOptions"/>.</param>
        /// <returns></returns>
        public static TokenManagementBuilder AddAccessTokenManagement(
            this IServiceCollection services,
            Action<AccessTokenManagementOptions> configureAction)
        {
            CheckConfigMarker(services);
            
            var options = new AccessTokenManagementOptions();
            configureAction?.Invoke(options);
            
            services.AddSingleton(options.Client);
            services.AddSingleton(options.User);

            services.AddUserAccessTokenManagementInternal();
            services.AddClientAccessTokenManagementInternal();
            
            return new TokenManagementBuilder(services);
        }

        /// <summary>
        /// Adds the token management services to DI
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configureAction">A delegate that is used to configure an <see cref="AccessTokenManagementOptions"/>.</param>
        /// <returns></returns>
        /// <remarks>
        /// The <see cref="IServiceProvider"/> provided to <paramref name="configureAction"/> will be the
        /// same application's root service provider instance.
        /// </remarks>
        public static TokenManagementBuilder AddAccessTokenManagement(
            this IServiceCollection services,
            Action<IServiceProvider, AccessTokenManagementOptions> configureAction)
        {
            CheckConfigMarker(services);

            var options = new AccessTokenManagementOptions();

            services.AddSingleton(provider =>
            {
                configureAction?.Invoke(provider, options);
                return options.Client;
            });

            services.AddSingleton(options.User);

            services.AddUserAccessTokenManagementInternal();
            services.AddClientAccessTokenManagementInternal();

            return new TokenManagementBuilder(services);
        }
        
        /// <summary>
        /// Adds the services required for client access token management using all default values
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns></returns>
        public static TokenManagementBuilder AddClientAccessTokenManagement(this IServiceCollection services)
        {
            CheckConfigMarker(services);
            
            var clientOptions = new ClientAccessTokenManagementOptions();
            
            services.AddSingleton(clientOptions);
            services.AddSingleton(new UserAccessTokenManagementOptions());

            return services.AddClientAccessTokenManagementInternal();
        }
        
        /// <summary>
        /// Adds the services required for client access token management
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configureAction">A delegate that is used to configure a <see cref="ClientAccessTokenManagementOptions"/>.</param>
        /// <returns></returns>
        public static TokenManagementBuilder AddClientAccessTokenManagement(
            this IServiceCollection services,
            Action<ClientAccessTokenManagementOptions> configureAction)
        {
            CheckConfigMarker(services);
            
            var clientOptions = new ClientAccessTokenManagementOptions();
            configureAction?.Invoke(clientOptions);
            
            services.AddSingleton(clientOptions);
            services.AddSingleton(new UserAccessTokenManagementOptions());

            return services.AddClientAccessTokenManagementInternal();
        }

        /// <summary>
        /// Adds the services required for client access token management
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configureAction">A delegate that is used to configure a <see cref="ClientAccessTokenManagementOptions"/>.</param>
        /// <returns></returns>
        /// <remarks>
        /// The <see cref="IServiceProvider"/> provided to <paramref name="configureAction"/> will be the
        /// same application's root service provider instance.
        /// </remarks>
        public static TokenManagementBuilder AddClientAccessTokenManagement(
            this IServiceCollection services,
            Action<IServiceProvider, ClientAccessTokenManagementOptions> configureAction)
        {
            CheckConfigMarker(services);

            services.AddSingleton(provider =>
            {
                var clientOptions = new ClientAccessTokenManagementOptions();
                configureAction?.Invoke(provider, clientOptions);

                return clientOptions;
            });

            services.AddSingleton(new UserAccessTokenManagementOptions());

            return services.AddClientAccessTokenManagementInternal();
        }
        
        /// <summary>
        /// Adds the services required for user access token management using all default values
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <returns></returns>
        public static TokenManagementBuilder AddUserAccessTokenManagement(this IServiceCollection services)
        {
            CheckConfigMarker(services);
            
            var userOptions = new UserAccessTokenManagementOptions();
            
            services.AddSingleton(userOptions);
            services.AddSingleton(new ClientAccessTokenManagementOptions());

            return services.AddUserAccessTokenManagementInternal();
        }

        /// <summary>
        /// Adds the services required for user access token management
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configureAction">A delegate that is used to configure an <see cref="UserAccessTokenManagementOptions"/>.</param>
        /// <returns></returns>
        public static TokenManagementBuilder AddUserAccessTokenManagement(
            this IServiceCollection services,
            Action<UserAccessTokenManagementOptions> configureAction)
        {
            CheckConfigMarker(services);
            
            var userOptions = new UserAccessTokenManagementOptions();
            configureAction?.Invoke(userOptions);
            
            services.AddSingleton(userOptions);
            services.AddSingleton(new ClientAccessTokenManagementOptions());

            return services.AddUserAccessTokenManagementInternal();
        }

        /// <summary>
        /// Adds the services required for user access token management
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configureAction">A delegate that is used to configure an <see cref="UserAccessTokenManagementOptions"/>.</param>
        /// <returns></returns>
        /// <remarks>
        /// The <see cref="IServiceProvider"/> provided to <paramref name="configureAction"/> will be the
        /// same application's root service provider instance.
        /// </remarks>
        public static TokenManagementBuilder AddUserAccessTokenManagement(
            this IServiceCollection services,
            Action<IServiceProvider, UserAccessTokenManagementOptions> configureAction)
        {
            CheckConfigMarker(services);

            services.AddSingleton(provider =>
            {
                var userOptions = new UserAccessTokenManagementOptions();
                configureAction?.Invoke(provider, userOptions);
                return userOptions;
            });

            
            services.AddSingleton(new ClientAccessTokenManagementOptions());

            return services.AddUserAccessTokenManagementInternal();
        }
        
        private static TokenManagementBuilder AddUserAccessTokenManagementInternal(this IServiceCollection services)
        {
            // necessary ASP.NET plumbing
            services.AddHttpContextAccessor();
            services.AddAuthentication();
            
            services.AddSharedServices();
            
            services.TryAddTransient<IUserAccessTokenManagementService, UserAccessTokenManagementService>();
            services.TryAddTransient<IUserAccessTokenStore, AuthenticationSessionUserAccessTokenStore>();
            services.TryAddSingleton<IUserAccessTokenRequestSynchronization, AccessTokenRequestSynchronization>();
            
            return new TokenManagementBuilder(services);
        }

        private static void AddSharedServices(this IServiceCollection services)
        {
            services.TryAddTransient<ITokenClientConfigurationService, DefaultTokenClientConfigurationService>();
            services.TryAddTransient<ITokenEndpointService, TokenEndpointService>();
            
            services.AddHttpClient(AccessTokenManagementDefaults.BackChannelHttpClientName);
        }

        /// <summary>
        /// Adds a named HTTP client for the factory that automatically sends the current user access token
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the client.</param>
        /// <param name="parameters"></param>
        /// <param name="configureClient">A delegate that is used to configure a <see cref="HttpClient"/>.</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddUserAccessTokenHttpClient(this IServiceCollection services, 
            string name,
            UserAccessTokenParameters? parameters = null, 
            Action<HttpClient>? configureClient = null)
        {
            if (configureClient != null)
            {
                return services.AddHttpClient(name, configureClient)
                    .AddUserAccessTokenHandler(parameters);
            }

            return services.AddHttpClient(name)
                .AddUserAccessTokenHandler(parameters);
        }

        /// <summary>
        /// Adds a named HTTP client for the factory that automatically sends the current user access token
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="name">The name of the client.</param>
        /// <param name="parameters"></param>
        /// <param name="configureClient">Additional configuration with service provider instance.</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddUserAccessTokenHttpClient(this IServiceCollection services,
            string name,
            UserAccessTokenParameters? parameters = null,
            Action<IServiceProvider, HttpClient>? configureClient = null)
        {
            if (configureClient != null)
            {
                return services.AddHttpClient(name, configureClient)
                    .AddUserAccessTokenHandler(parameters);
            }

            return services.AddHttpClient(name)
                .AddUserAccessTokenHandler(parameters);
        }

        /// <summary>
        /// Adds a named HTTP client for the factory that automatically sends the a client access token
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="clientName">The name of the client.</param>
        /// <param name="tokenClientName">The name of the token client.</param>
        /// <param name="configureClient">A delegate that is used to configure a <see cref="HttpClient"/>.</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddClientAccessTokenHttpClient(
            this IServiceCollection services, string clientName,
            string tokenClientName = AccessTokenManagementDefaults.DefaultTokenClientName,
            Action<HttpClient>? configureClient = null)
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
        /// Adds a named HTTP client for the factory that automatically sends the a client access token
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="clientName">The name of the client.</param>
        /// <param name="tokenClientName">The name of the token client.</param>
        /// <param name="configureClient">Additional configuration with service provider instance.</param>
        /// <returns></returns>
        public static IHttpClientBuilder AddClientAccessTokenHttpClient(
            this IServiceCollection services, 
            string clientName,
            string tokenClientName = AccessTokenManagementDefaults.DefaultTokenClientName,
            Action<IServiceProvider, HttpClient>? configureClient = null)
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
        public static IHttpClientBuilder AddClientAccessTokenHandler(
            this IHttpClientBuilder httpClientBuilder,
            string tokenClientName = AccessTokenManagementDefaults.DefaultTokenClientName)
        {
            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var accessTokenManagementService = provider.GetRequiredService<IClientAccessTokenManagementService>();

                return new ClientAccessTokenHandler(accessTokenManagementService, tokenClientName);
            });
        }

        /// <summary>
        /// Adds the user access token handler to an HttpClient
        /// </summary>
        /// <param name="httpClientBuilder"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static IHttpClientBuilder AddUserAccessTokenHandler(
            this IHttpClientBuilder httpClientBuilder,
            UserAccessTokenParameters? parameters = null)
        {
            return httpClientBuilder.AddHttpMessageHandler(provider =>
            {
                var contextAccessor = provider.GetRequiredService<IHttpContextAccessor>();

                return new UserAccessTokenHandler(contextAccessor, parameters);
            });
        }
        
        private static TokenManagementBuilder AddClientAccessTokenManagementInternal(this IServiceCollection services)
        {
            // necessary ASP.NET plumbing
            services.AddDistributedMemoryCache();
            services.TryAddSingleton<ISystemClock, SystemClock>();
            services.TryAddSingleton<IAuthenticationSchemeProvider, AuthenticationSchemeProvider>();
            
            services.AddSharedServices();
            
            services.TryAddTransient<IClientAccessTokenManagementService, ClientAccessTokenManagementService>();
            services.TryAddTransient<IClientAccessTokenCache, ClientAccessTokenCache>();
            services.TryAddSingleton<IClientAccessTokenRequestSynchronization, AccessTokenRequestSynchronization>();
            
            return new TokenManagementBuilder(services);
        }
        
        private static void CheckConfigMarker(IServiceCollection services)
        {
            var marker = services.FirstOrDefault(s => s.ServiceType == typeof(ConfigMarker));
            if (marker == null)
            {
                services.AddSingleton(new ConfigMarker());
                return;
            }

            throw new InvalidOperationException(
                "Call 'AddAccessTokenManagement' to add support for both client and user access tokens. Or call 'AddUserAccessTokenManagement' or 'AddClientAccessTokenManagement' respectively. You cannot mix them. Nor can you call them multiple times.");
        }
    }
}