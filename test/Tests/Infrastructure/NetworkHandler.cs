using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Tests.Infrastructure
{
    class NetworkHandler : HttpMessageHandler
    {
        public Uri Address { get; set; }

        public HttpContent Content { get; set; }

        public IDictionary<string, object> Properties { get; set; }
        
        #if NET5_0_OR_GREATER
        public HttpRequestOptions Options { get; set; }
        #endif

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Address = request.RequestUri;
            Content = request.Content;
            
            #if NET5_0_OR_GREATER
            Options = request.Options;
            #else 
            Properties = request.Properties;
            #endif
            
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));
        }
    }
}