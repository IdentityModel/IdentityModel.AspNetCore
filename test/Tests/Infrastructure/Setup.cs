using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Tests.Infrastructure
{
    static class Setup
    {
        public static IServiceCollection ClientCollection(Action<ClientAccessTokenManagementOptions> options = null, HttpMessageHandler networkHandler = null)
        {
            var services = new ServiceCollection();
            
            var builder = services.AddClientAccessTokenManagement(options);

            if (networkHandler != null)
            {
                builder.ConfigureBackchannelHttpClient()
                    .ConfigurePrimaryHttpMessageHandler(s => networkHandler);
            }

            return services;
        }

        public static ServiceProvider ClientContainer(Action<ClientAccessTokenManagementOptions> options = null)
        {
            return ClientCollection(options).BuildServiceProvider();
        }
    }
}