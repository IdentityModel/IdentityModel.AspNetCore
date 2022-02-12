using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class ClientAccessTokenCacheTests
    {
        private readonly ClientAccessTokenCache _cache;

        public ClientAccessTokenCacheTests()
        {
            var services = new ServiceCollection()
                .AddDistributedMemoryCache()
                .AddLogging()
                .BuildServiceProvider();

            _cache = ActivatorUtilities.CreateInstance<ClientAccessTokenCache>(services, new ClientAccessTokenManagementOptions());
        }

        [Theory]
        [InlineData("some_access_token")]
        [InlineData("some___access___token")]
        public async Task Should_get_and_set(string accessToken)
        {
            // Given
            var clientName = "client_name";
            var expiresIn = 1234;
            var expectedExpiration = DateTimeOffset.UtcNow.AddSeconds(expiresIn);

            // When
            await _cache.SetAsync(clientName, accessToken, expiresIn, null);
            var retrievedToken = await _cache.GetAsync(clientName, null);

            // Then
            Assert.Equal(accessToken, retrievedToken.AccessToken);
            Assert.Equal(expectedExpiration.UtcDateTime, retrievedToken.Expiration.UtcDateTime, TimeSpan.FromSeconds(5));
        }
    }
}
