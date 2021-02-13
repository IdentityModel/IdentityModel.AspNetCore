using FluentAssertions;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using IdentityModel.Client;
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
                o.Client.Clients.Add("test", new ClientCredentialsTokenRequest
                {
                    Address = "https://test",
                    ClientId = "test"
                });
            }

            var service = Setup.Collection(options, handler)
                .BuildServiceProvider()
                .GetRequiredService<IClientTokenManagementService>();

            var result = await service.GetClientAccessTokenAsync();

            handler.Address.Should().Be(new Uri("https://test"));
        }

        [Fact]
        public async Task Using_default_configuration_with_multiple_client_config_should_fail()
        {
            var handler = new NetworkHandler();

            void options(AccessTokenManagementOptions o)
            {
                o.Client.Clients.Add("test1", new ClientCredentialsTokenRequest
                {
                    Address = "https://test1",
                    ClientId = "test1"
                });

                o.Client.Clients.Add("test2", new ClientCredentialsTokenRequest
                {
                    Address = "https://test2",
                    ClientId = "test2"
                });
            }

            var service = Setup.Collection(options, handler)
                .BuildServiceProvider()
                .GetRequiredService<IClientTokenManagementService>();

            
            Func<Task> act = async () => { var result = await service.GetClientAccessTokenAsync(); };

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Using_explicit_configuration_with_multiple_client_config_should_succeed()
        {
            var handler = new NetworkHandler();

            void options(AccessTokenManagementOptions o)
            {
                o.Client.Clients.Add("test1", new ClientCredentialsTokenRequest
                {
                    Address = "https://test1",
                    ClientId = "test1"
                });

                o.Client.Clients.Add("test2", new ClientCredentialsTokenRequest
                {
                    Address = "https://test2",
                    ClientId = "test2"
                });
            }

            var service = Setup.Collection(options, handler)
                .BuildServiceProvider()
                .GetRequiredService<IClientTokenManagementService>();


            var result = await service.GetClientAccessTokenAsync("test1");
            handler.Address.Should().Be(new Uri("https://test1"));

            result = await service.GetClientAccessTokenAsync("test2");
            handler.Address.Should().Be(new Uri("https://test2"));
        }

        [Fact]
        public async Task Using_default_configuration_should_pass_client_parameters()
        {
            var handler = new NetworkHandler();

            void options(AccessTokenManagementOptions o)
            {
                o.Client.Clients.Add("test", new ClientCredentialsTokenRequest
                {
                    Address = "https://test",
                    ClientId = "test",
                    Parameters = 
                    {
                        { "audience", "test123" }
                    }
                });
            }

            var service = Setup.Collection(options, handler)
                .BuildServiceProvider()
                .GetRequiredService<IClientTokenManagementService>();

            var result = await service.GetClientAccessTokenAsync();
            var requestContent = await handler.Content.ReadAsStringAsync();
            requestContent.Should().Contain("audience=test123");
        }
    }
}