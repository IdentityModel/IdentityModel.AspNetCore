using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Tests.Infrastructure
{
    static class Setup
    {
        public static IServiceCollection Collection(Action<AccessTokenManagementOptions> options = null)
        {
            var services = new ServiceCollection();

            services.AddAuthentication();
            services.AddAccessTokenManagement(options);

            return services;
        }

        public static ServiceProvider Container(Action<AccessTokenManagementOptions> options = null)
        {
            return Collection(options).BuildServiceProvider();
        }
    }
}