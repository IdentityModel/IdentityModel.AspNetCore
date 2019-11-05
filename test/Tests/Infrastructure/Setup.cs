using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Tests.Infrastructure
{
    static class Setup
    {
        public static IServiceCollection Collection(Action<AccessTokenManagementOptions> options = null, HttpMessageHandler networkHandler = null)
        {
            var services = new ServiceCollection();

            services.AddAuthentication();
            var builder = services.AddAccessTokenManagement(options);

            if (networkHandler != null)
            {
                builder.ConfigureBackchannelHttpClient()
                    .ConfigurePrimaryHttpMessageHandler(s => networkHandler);
            }

            return services;
        }

        public static ServiceProvider Container(Action<AccessTokenManagementOptions> options = null)
        {
            return Collection(options).BuildServiceProvider();
        }
    }
}