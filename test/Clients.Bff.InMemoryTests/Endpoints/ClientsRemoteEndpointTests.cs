using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Clients.Bff.InMemoryTests.TestFramework;
using Clients.Bff.InMemoryTests.TestHosts;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Clients.Bff.InMemoryTests.Endpoints
{
    public class ClientsRemoteEndpointTests : ClientsBffIntegrationTestBase
    {


        public ClientsRemoteEndpointTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact]
        public async Task post_to_remote_endpoint_should_carry_useraccesstokenparameter()
        {
            WhichLazy.UseOldLazy = false;

            await BffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Post, BffHost.Url("/api_user/test"));
            req.Headers.Add("x-csrf", "1");
            req.Content = new StringContent(JsonSerializer.Serialize(new TestPayload("hello test api")), Encoding.UTF8,
                "application/json");
            var response = await BffHost.BrowserClient.SendAsync(req);
        }

        [Fact]
        public async Task calls_to_remote_endpoint_with_useraccesstokenparameters_having_stored_named_token_should_forward_user_to_api()
        {
            WhichLazy.UseOldLazy = false;

            var loginResponse = await BffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user_with_useraccesstokenparameters_having_stored_named_token/test"));
            req.Headers.Add("x-csrf", "1");
            var response = await BffHost.BrowserClient.SendAsync(req);

            response.IsSuccessStatusCode.Should().BeTrue();
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");
            var json = await response.Content.ReadAsStringAsync();
            var apiResult = JsonSerializer.Deserialize<ApiResponse>(json);
            apiResult.Method.Should().Be("GET");
            apiResult.Path.Should().Be("/test");
            apiResult.Sub.Should().Be("alice");
            apiResult.ClientId.Should().Be("spa");
        }

        [Fact]
        public async Task calls_to_different_remote_endpoints_with_useraccesstokenparameters_having_stored_respective_named_tokens_should_forward_user_to_api_with_corresponding_tokens()
        {
            WhichLazy.UseOldLazy = false;

            var loginResponse = await BffHost.BffLoginAsync("alice");

            var anotherreq = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user_with_useraccesstokenparameters_having_stored_another_named_token/test"));
            anotherreq.Headers.Add("x-csrf", "1");
            var anotherreqresponse = BffHost.BrowserClient.SendAsync(anotherreq);

            var andanotherreq = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user_with_useraccesstokenparameters_having_stored_and_another_named_token/test"));
            andanotherreq.Headers.Add("x-csrf", "1");
            var andanotherresponse = BffHost.BrowserClient.SendAsync(andanotherreq);

            await Task.WhenAll(new List<Task>() { anotherreqresponse, andanotherresponse });
        }

        [Fact]
        public async Task calls_to_different_remote_endpoints_with_useraccesstokenparameters_having_stored_respective_named_tokens_should_fail_to_forward_user_to_api_with_corresponding_tokens_race()
        {
            WhichLazy.UseOldLazy = false;

            await BffHost.BffLoginAsync("alice");

            var anotherreq = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user_with_useraccesstokenparameters_having_stored_another_named_token/test?seconds=90000"));
            anotherreq.Headers.Add("x-csrf", "1");
            var anotherreqTask = DelayThenAwaitSendAsync(0, anotherreq);
            
            var andanotherreq = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user_with_useraccesstokenparameters_having_stored_another_named_token/test?seconds=0"));
            andanotherreq.Headers.Add("x-csrf", "1");
            var andanotherreqTask = DelayThenAwaitSendAsync(30000, andanotherreq);

            var reqTasks = new List<Task<HttpResponseMessage>> { anotherreqTask, andanotherreqTask };

            Stopwatch sp = new Stopwatch();
            sp.Start();

            while (reqTasks.Count > 0)
            {
                var finishedReqTask = await Task.WhenAny(reqTasks);
                if (finishedReqTask == anotherreqTask)
                {
                    _testOutputHelper.WriteLine($"{nameof(anotherreqTask)} completed in {sp.ElapsedMilliseconds/1000} seconds");
                }
                if (finishedReqTask == andanotherreqTask)
                {
                    _testOutputHelper.WriteLine($"{nameof(andanotherreqTask)} completed in {sp.ElapsedMilliseconds / 1000} seconds");
                }
                reqTasks.Remove(finishedReqTask);
            }

            sp.Stop();
            sp.Reset();
        }

        private async Task<HttpResponseMessage> DelayThenAwaitSendAsync(int milliSeconds, HttpRequestMessage httpRequestMessage)
        {
            await Task.Delay(milliSeconds);
            return await BffHost.BrowserClient.SendAsync(httpRequestMessage);
        }

        [Fact]
        public async Task calls_to_different_remote_endpoints_with_useraccesstokenparameters_having_stored_respective_named_tokens_should_fail_to_forward_user_to_api_with_corresponding_tokens()
        {
            WhichLazy.UseOldLazy = true;

            var loginResponse = await BffHost.BffLoginAsync("alice");

            var anotherreq = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user_with_useraccesstokenparameters_having_stored_another_named_token/test"));
            anotherreq.Headers.Add("x-csrf", "1");
            var anotherreqresponse = BffHost.BrowserClient.SendAsync(anotherreq);

            var andanotherreq = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user_with_useraccesstokenparameters_having_stored_and_another_named_token/test"));
            andanotherreq.Headers.Add("x-csrf", "1");
            var andanotherresponse = BffHost.BrowserClient.SendAsync(andanotherreq);

            await Task.WhenAll(new List<Task>() { anotherreqresponse, andanotherresponse });
        }


        [Fact]
        public async Task calls_to_remote_endpoint_with_useraccesstokenparameters_having_not_stored_corresponding_named_token_finds_no_matching_token_should_fail()
        {
            WhichLazy.UseOldLazy = false;

            var loginResponse = await BffHost.BffLoginAsync("alice");

            var req = new HttpRequestMessage(HttpMethod.Get, BffHost.Url("/api_user_with_useraccesstokenparameters_having_not_stored_named_token/test"));
            req.Headers.Add("x-csrf", "1");

            var response = await BffHost.BrowserClient.SendAsync(req);
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}