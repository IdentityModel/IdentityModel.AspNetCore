using FluentAssertions;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Tests.Infrastructure;
using Xunit;

namespace Tests
{
    public class OptionsConfigServicesTests
    {
        [Fact]
        public async Task Using_default_configuration_with_no_scheme_or_explicit_client_config_should_fail()
        {
            var service = Setup.Collection()
                .AddTransient(p => p.GetRequiredService<ITokenClientConfigurationService>() as OptionsTokenClientConfigurationService)
                .BuildServiceProvider()
                .GetRequiredService<OptionsTokenClientConfigurationService>();

            Func<Task> act = async () => { var settings = await service.GetOpenIdConnectSettingsAsync(null); };

            await act.Should().ThrowAsync<InvalidOperationException>();            
        }

        [Fact]
        public async Task Using_default_configuration_with_wrong_scheme_or_explicit_client_config_should_fail()
        {
            var service = Setup.Collection()
                .AddTransient(p => p.GetRequiredService<ITokenClientConfigurationService>() as OptionsTokenClientConfigurationService)
                .BuildServiceProvider()
                .GetRequiredService<OptionsTokenClientConfigurationService>();

            Func<Task> act = async () => { var settings = await service.GetOpenIdConnectSettingsAsync("invalid"); };

            await act.Should().ThrowAsync<InvalidOperationException>();
        }
    }
}
