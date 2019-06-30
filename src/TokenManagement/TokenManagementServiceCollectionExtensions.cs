using IdentityModel.AspNetCore;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TokenManagementServiceCollectionExtensions
    {
        public static TokenManagementBuilder AddTokenManagement(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddTransient<AccessTokenHandler>();
            services.AddTransient<ITokenStore, AuthenticationSessionTokenStore>();
            services.AddTransient<TokenEndpointService>();
            services.AddHttpClient<TokenEndpointService>();

            return new TokenManagementBuilder(services);
        }
    }
}