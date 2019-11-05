using FluentAssertions;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class ClientAccessTokenTests
    {
        private ServiceProvider SetupContainer(Action<AccessTokenManagementOptions> options = null)
        {
            var services = new ServiceCollection();

            services.AddAuthentication();
            services.AddAccessTokenManagement(options);

            return services.BuildServiceProvider();
        }

        [Fact]
        public async Task Using_default_configuration_with_no_scheme_or_explicit_client_config_should_fail()
        {
            var service = SetupContainer().GetRequiredService<IAccessTokenManagementService>();

            Func<Task> act = async () => { var token = await service.GetClientAccessTokenAsync(); };
            
            await act.Should().ThrowAsync<InvalidOperationException>();            
        }

        [Fact]
        public async Task Using_default_configuration_with_wrong_scheme_or_explicit_client_config_should_fail()
        {
            Action<AccessTokenManagementOptions> options = (o) =>
            {
                o.Client.OidcSchemeClient = "invalid";
            };

            var service = SetupContainer(options).GetRequiredService<IAccessTokenManagementService>();

            Func<Task> act = async () => { var token = await service.GetClientAccessTokenAsync(); };

            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
