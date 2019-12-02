using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Infrastructure
{
    class NetworkHandler : HttpMessageHandler
    {
        public Uri Address { get; set; }

        public HttpContent Content { get; set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Address = request.RequestUri;
            Content = request.Content;

            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        }
    }
}