using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace IdentityModel.AspNetCore
{
    public class TokenManagementBuilder
    {
        public IServiceCollection Services { get; }

        public TokenManagementBuilder(IServiceCollection services)
        {
            Services = services; ;
        }

        public IHttpClientBuilder ConfigureBackchannelHttpClient()
        {
            return Services.AddHttpClient<TokenEndpointService>();
        }

        public IHttpClientBuilder ConfigureBackchannelHttpClient(Action<HttpClient> configureClient)
        {
            return Services.AddHttpClient<TokenEndpointService>(configureClient);
        }
    }
}