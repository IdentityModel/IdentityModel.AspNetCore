using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.AspNetCore.AccessTokenManagement.Azure;
using Microsoft.Extensions.DependencyInjection;
using System;
using Xunit;

namespace Tests.Azure
{
    public class KeyVaultExtensionsTests
    {
        [Fact]
        public void WithAzureKeyVault_Register_ShouldAddExpectedServices()
        {
            // Arrange
            var services = new ServiceCollection();

            //Act
            services
                .AddClientAccessTokenManagement()
                .WithAzureKeyVault(opts =>
                {
                    opts.Credential = new DefaultAzureCredential();
                    opts.Url = new Uri("http://demo.com");
                });


            var secretClient = services.BuildServiceProvider()
                .GetRequiredService<SecretClient>();

            var clientAccessCache = services.BuildServiceProvider()
                .GetRequiredService<IClientAccessTokenCache>();

            // Assert
            Assert.NotNull(secretClient);
            Assert.NotNull(clientAccessCache);
            Assert.IsType<KeyVaultClientAccessTokenCache>(clientAccessCache);
        }
    }
}
