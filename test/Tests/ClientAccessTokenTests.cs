using FluentAssertions;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Tests.Infrastructure;
using Xunit;

namespace Tests
{
    public class ClientAccessTokenTests
    {
        [Fact]
        public async Task Using_default_configuration_with_single_client_config_should_succeed()
        {
            var handler = new NetworkHandler();

            void options(AccessTokenManagementOptions o)
            {
                o.Client.Clients.Add("test", new IdentityModel.Client.TokenClientOptions
                {
                    Address = "https://test",
                    ClientId = "test"
                });
            }

            var service = Setup.Collection(options, handler)
                .BuildServiceProvider()
                .GetRequiredService<IAccessTokenManagementService>();

            var result = await service.GetClientAccessTokenAsync();

            handler.Address.Should().Be(new Uri("https://test"));
        }

        [Fact]
        public async Task Using_default_configuration_with_multiple_client_config_should_fail()
        {
            var handler = new NetworkHandler();

            void options(AccessTokenManagementOptions o)
            {
                o.Client.Clients.Add("test1", new IdentityModel.Client.TokenClientOptions
                {
                    Address = "https://test1",
                    ClientId = "test1"
                });

                o.Client.Clients.Add("test2", new IdentityModel.Client.TokenClientOptions
                {
                    Address = "https://test2",
                    ClientId = "test2"
                });
            }

            var service = Setup.Collection(options, handler)
                .BuildServiceProvider()
                .GetRequiredService<IAccessTokenManagementService>();

            
            Func<Task> act = async () => { var result = await service.GetClientAccessTokenAsync(); };

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Using_explicit_configuration_with_multiple_client_config_should_succeed()
        {
            var handler = new NetworkHandler();

            void options(AccessTokenManagementOptions o)
            {
                o.Client.Clients.Add("test1", new IdentityModel.Client.TokenClientOptions
                {
                    Address = "https://test1",
                    ClientId = "test1"
                });

                o.Client.Clients.Add("test2", new IdentityModel.Client.TokenClientOptions
                {
                    Address = "https://test2",
                    ClientId = "test2"
                });
            }

            var service = Setup.Collection(options, handler)
                .BuildServiceProvider()
                .GetRequiredService<IAccessTokenManagementService>();


            var result = await service.GetClientAccessTokenAsync("test1");
            handler.Address.Should().Be(new Uri("https://test1"));

            result = await service.GetClientAccessTokenAsync("test2");
            handler.Address.Should().Be(new Uri("https://test2"));
        }
    }
}