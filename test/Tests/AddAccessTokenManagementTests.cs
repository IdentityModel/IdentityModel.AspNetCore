using System.Reflection.Metadata.Ecma335;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace Tests
{
    public class AddAccessTokenManagementTests
    {
        public class CustomOptions
        {
            public string Thing { get; set; }
        }

        [Fact]
        public void Using_overload_with_serviceprovider_should_succeed()
        {
            var services = new ServiceCollection();
            var prefix = "Blah";
            services.AddOptions<CustomOptions>()
                .Configure(options =>
                {
                    options.Thing = prefix;
                });

            services.AddSingleton(provider => provider.GetRequiredService<IOptions<CustomOptions>>().Value);

            services.AddAccessTokenManagement((provider, options) =>
            {
                var custom = provider.GetRequiredService<CustomOptions>();

                options.Client.CacheKeyPrefix = custom.Thing;
            });

            var clientAccessTokenManagementOptions = services.BuildServiceProvider().GetRequiredService<ClientAccessTokenManagementOptions>();

            Assert.Equal(prefix, clientAccessTokenManagementOptions.CacheKeyPrefix);
        }
    }
}