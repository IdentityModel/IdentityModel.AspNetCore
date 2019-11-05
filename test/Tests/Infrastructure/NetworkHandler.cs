using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Infrastructure
{
    class NetworkHandler : HttpMessageHandler
    {
        public Uri Address { get; set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Address = request.RequestUri;

            return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
        }
    }
}