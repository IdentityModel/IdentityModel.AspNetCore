using FluentAssertions;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
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

            void options(ClientAccessTokenManagementOptions o)
            {
                o.Clients.Add("test", new ClientCredentialsTokenRequest
                {
                    Address = "https://test",
                    ClientId = "test"
                });
            }

            var service = Setup.ClientCollection(options, handler)
                .BuildServiceProvider()
                .GetRequiredService<IClientAccessTokenManagementService>();

            var result = await service.GetClientAccessTokenAsync();

            handler.Address.Should().Be(new Uri("https://test"));
        }

        [Fact]
        public async Task Using_default_configuration_with_multiple_client_config_should_fail()
        {
            var handler = new NetworkHandler();

            void options(ClientAccessTokenManagementOptions o)
            {
                o.Clients.Add("test1", new ClientCredentialsTokenRequest
                {
                    Address = "https://test1",
                    ClientId = "test1"
                });

                o.Clients.Add("test2", new ClientCredentialsTokenRequest
                {
                    Address = "https://test2",
                    ClientId = "test2"
                });
            }

            var service = Setup.ClientCollection(options, handler)
                .BuildServiceProvider()
                .GetRequiredService<IClientAccessTokenManagementService>();

            
            Func<Task> act = async () => { var result = await service.GetClientAccessTokenAsync(); };

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Using_explicit_configuration_with_multiple_client_config_should_succeed()
        {
            var handler = new NetworkHandler();

            void options(ClientAccessTokenManagementOptions o)
            {
                o.Clients.Add("test1", new ClientCredentialsTokenRequest
                {
                    Address = "https://test1",
                    ClientId = "test1"
                });

                o.Clients.Add("test2", new ClientCredentialsTokenRequest
                {
                    Address = "https://test2",
                    ClientId = "test2"
                });
            }

            var service = Setup.ClientCollection(options, handler)
                .BuildServiceProvider()
                .GetRequiredService<IClientAccessTokenManagementService>();


            var result = await service.GetClientAccessTokenAsync("test1");
            handler.Address.Should().Be(new Uri("https://test1"));

            result = await service.GetClientAccessTokenAsync("test2");
            handler.Address.Should().Be(new Uri("https://test2"));
        }

        [Fact]
        public async Task Using_default_configuration_should_pass_client_parameters()
        {
            var handler = new NetworkHandler();

            void options(ClientAccessTokenManagementOptions o)
            {
                o.Clients.Add("test", new ClientCredentialsTokenRequest
                {
                    Address = "https://test",
                    ClientId = "test",
                    Resource = { "urn:resource" },
                    Parameters = 
                    {
                        { "audience", "test123" }
                    }
                });
            }

            var service = Setup.ClientCollection(options, handler)
                .BuildServiceProvider()
                .GetRequiredService<IClientAccessTokenManagementService>();

            var result = await service.GetClientAccessTokenAsync();
            var requestContent = await handler.Content.ReadAsStringAsync();
            requestContent.Should().Contain("audience=test123");
            requestContent.Should().Contain("resource=urn%3Aresource");
        }
        
        [Fact(Skip = "Weird x-plat behavior")]
        public async Task ClientAccessTokenParameters_should_propagate()
        {
            var handler = new NetworkHandler();

            void options(ClientAccessTokenManagementOptions o)
            {
                o.Clients.Add("test", new ClientCredentialsTokenRequest
                {
                    Address = "https://test",
                    ClientId = "test"
                });
            }

            var service = Setup.ClientCollection(options, handler)
                .BuildServiceProvider()
                .GetRequiredService<IClientAccessTokenManagementService>();
            
            var parameters = new ClientAccessTokenParameters
            {
                Resource = "urn:resource",
                Context =
                {
                    { "context_item", "context_value" }
                }
            };
            
            var result = await service.GetClientAccessTokenAsync(parameters: parameters);
            var requestContent = await handler.Content.ReadAsStringAsync();
            requestContent.Should().Contain("resource=urn%3Aresource");
            
            #if NET5_0_OR_GREATER
            parameters = handler.Options.SingleOrDefault(o => o.Key == AccessTokenManagementDefaults.AccessTokenParametersOptionsName).Value 
                as ClientAccessTokenParameters;
            #else
            
            var properties = handler.Properties;
            parameters =
                properties[AccessTokenManagementDefaults.AccessTokenParametersOptionsName] as
                    ClientAccessTokenParameters;
            #endif
            
            parameters.Should().NotBeNull();
            parameters.Context["context_item"].First().Should().Be("context_value");
        }
    }
}