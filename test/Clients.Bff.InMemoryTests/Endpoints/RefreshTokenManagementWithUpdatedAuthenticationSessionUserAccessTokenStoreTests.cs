using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Clients.Bff.InMemoryTests.TestFramework;
using Clients.Bff.InMemoryTests.TestHosts;
using Xunit;
using Xunit.Abstractions;

namespace Clients.Bff.InMemoryTests.Endpoints
{
    public class RefreshTokenManagementWithUpdatedAuthenticationSessionUserAccessTokenStoreTests : ClientsBffIntegrationTestBase
    {
        public RefreshTokenManagementWithUpdatedAuthenticationSessionUserAccessTokenStoreTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper, true)
        {
        }

        [Fact]
        public async Task Succeeds_with_support_for_refresh_token_per_challenge_scheme()
        {
            await BffHost.BffLoginAsync("alice");

            var anotherreq = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api/user"));
            anotherreq.Headers.Add("x-csrf", "1");
            var anotherreqresponse = BffHost.BrowserClient.SendAsync(anotherreq);

            var req = new HttpRequestMessage(HttpMethod.Post, BffHost.Url("/cdpapitest"));
            req.Headers.Add("x-csrf", "1");
            req.Content = new StringContent(JsonSerializer.Serialize(new TestPayload("hello test api")), Encoding.UTF8,
                "application/json");
            var response = BffHost.BrowserClient.SendAsync(req);

            await Task.WhenAll(new List<Task>() { response, anotherreqresponse });

            var bothRequestsSucceeded = !response.Result.StatusCode.Equals(HttpStatusCode.Unauthorized) &&
                                        !anotherreqresponse.Result.StatusCode.Equals(HttpStatusCode.Unauthorized);

            Assert.True(bothRequestsSucceeded);
        }
    }
}