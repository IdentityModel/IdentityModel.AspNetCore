using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Polly;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.AccessTokenManagement;

namespace MvcCode
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;

            services.AddControllersWithViews();

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "cookie";
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie("cookie", options =>
                {
                    options.Cookie.Name = "mvccode";

                    options.Events.OnSigningOut = async e =>
                    {
                        await e.HttpContext.RevokeUserRefreshTokenAsync();
                    };
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://demo.duendesoftware.com";

                    options.ClientId = "interactive.confidential.short";
                    options.ClientSecret = "secret";

                    options.ResponseType = "code";
                    options.ResponseMode = "query";

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("offline_access");
                    
                    options.Scope.Add("resource1.scope1");
                    options.Scope.Add("resource2.scope1");
                    options.Scope.Add("resource3.scope1");
                    options.Scope.Add("scope3");
                    options.Scope.Add("scope4");

                    // keeps id_token smaller
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };

                    options.Events.OnRedirectToIdentityProvider = e =>
                    {
                        // prepare token requests, so a resource specific token can be requested
                        e.ProtocolMessage.Resource = "urn:resource3";
                    
                        return Task.CompletedTask;
                    };
                });

            // adds user and client access token management
            services.AddAccessTokenManagement(options =>
                {
                    // ask for a token for a specific resource
                    //options.Client.Resource = "urn:resource3";
                    
                    // ask for a specific scope
                    //options.Client.Scope = "shared.scope";
                })
                .ConfigureBackchannelHttpClient()
                    .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(new[]
                    {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(3)
                    }));

            // registers HTTP client that uses the managed user access token
            services.AddUserAccessTokenClient("user_client", configureClient: client =>
            {
                client.BaseAddress = new Uri("https://demo.duendesoftware.com/api/");
            });
            
            // registers HTTP client that uses the managed user access token for a specific resource
            services.AddUserAccessTokenClient("user_client_resource3", new UserAccessTokenParameters { Resource = "urn:resource3" }, configureClient: client =>
            {
                client.BaseAddress = new Uri("https://demo.duendesoftware.com/api/");
            });

            // registers HTTP client that uses the managed client access token
            services.AddClientAccessTokenClient("client", configureClient: client =>
            {
                client.BaseAddress = new Uri("https://demo.duendesoftware.com/api/");
            });

            // registers a typed HTTP client with token management support
            services.AddHttpClient<TypedUserClient>(client =>
            {
                client.BaseAddress = new Uri("https://demo.duendesoftware.com/api/");
            })
                .AddUserAccessTokenHandler();

            services.AddHttpClient<TypedClientClient>(client =>
            {
                client.BaseAddress = new Uri("https://demo.duendesoftware.com/api/");
            })
                .AddClientAccessTokenHandler();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute()
                    .RequireAuthorization();
            });
        }
    }
}