using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using System;
using System.Net.Http;

namespace IdentityModel.AspNetCore.Infrastructure
{
    internal static class HttpClientFactoryExtensions
    {
        public static IHttpClientBuilder AddOrUpdateHttpClient<T>(this IServiceCollection services, Action<HttpClient> configureClient = null)
            where T: class
        {
            var name = typeof(T).Name;
            IHttpClientBuilder httpBuilder;

            if (configureClient != null)
            {
                httpBuilder = services.AddHttpClient(name, configureClient);
            }
            else
            {
                httpBuilder = services.AddHttpClient(name);
            }

            httpBuilder.Services.AddTransient(s =>
            {
                var httpClientFactory = s.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(name);

                var typedClientFactory = s.GetRequiredService<ITypedHttpClientFactory<T>>();
                return typedClientFactory.CreateClient(httpClient);
            });

            return httpBuilder;
        }
    }
}