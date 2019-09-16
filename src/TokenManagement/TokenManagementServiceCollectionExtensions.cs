// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.AspNetCore;

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
        public static TokenManagementBuilder AddTokenManagement(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddTransient<AccessTokenHandler>();
            services.AddTransient<ITokenStore, AuthenticationSessionTokenStore>();
            
            services.AddHttpClient<TokenEndpointService>();

            return new TokenManagementBuilder(services);
        }
    }
}