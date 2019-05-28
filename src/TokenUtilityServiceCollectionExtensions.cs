using IdentityModel.AspNetCore;
using System;
using System.Net.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TokenUtilityServiceCollectionExtensions
    {
        public static TokenUtilitiesBuilder AddTokenUtilities(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddTransient<ITokenStore, AuthenticationSessionTokenStore>();
            services.AddTransient<TokenEndpointService>();
            services.AddHttpClient<TokenEndpointService>();

            return new TokenUtilitiesBuilder(services);
        }
    }

    public class TokenUtilitiesBuilder
    {
        public IServiceCollection Services { get; }

        public TokenUtilitiesBuilder(IServiceCollection services)
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