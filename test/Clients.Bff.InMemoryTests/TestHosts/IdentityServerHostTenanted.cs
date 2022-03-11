// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Clients.Bff.InMemoryTests.TestFramework;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Clients.Bff.InMemoryTests.TestHosts
{
    public class IdentityServerHostTenanted : GenericHost
    {
        public IdentityServerHostTenanted(ITestOutputHelper testOutputHelper, string baseAddress = "https://identityservertenantedtwo") 
            : base(testOutputHelper, baseAddress)
        {
            OnConfigureServices += ConfigureServices;
            OnConfigure += Configure;
        }

        public List<Client> Clients { get; set; } = new List<Client>();
        public List<IdentityResource> IdentityResources { get; set; } = new List<IdentityResource>()
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        };

        public List<ApiScope> ApiScopes { get; set; } = new List<ApiScope>(){
            // local API
            new ApiScope("IdentityServerApi"),
            new ApiScope("cdp.api"),
        };
        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
                new ApiResource($"{"urn:cdp.api"}")
                {
                    Scopes = { "cdp.api" },
                    ApiSecrets =
                    {
                        new Secret("secret".Sha256())
                    }
                },
                new ApiResource($"{"urn:IdentityServerApi"}")
                {
                    Scopes = { "IdentityServerApi" },
                    RequireResourceIndicator = true,
                    ApiSecrets =
                    {
                        new Secret("secret".Sha256())
                    }

                }
            };
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddAuthorization();

            services.AddLogging(logging => {
                logging.AddFilter("Duende", LogLevel.Debug);
            });

            var builder = services.AddIdentityServer(options=> 
            {
                options.EmitStaticAudienceClaim = true;
            })
                .AddInMemoryApiResources(ApiResources)
                .AddInMemoryClients(Clients)
                .AddInMemoryIdentityResources(IdentityResources)
                .AddInMemoryApiScopes(ApiScopes);
            builder.AddExtensionGrantValidator<TokenExchangeGrantValidator>();
            builder.AddProfileService<ProfileService>();

            services.AddLocalApiAuthentication();

        }

        private void Configure(IApplicationBuilder app)
        {
            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapGet("/api/user", async context =>
                {
                    await context.Response.WriteAsync(context.User.Identity.Name);
                });

                endpoints.MapGet("/account/login", context =>
                {
                    return Task.CompletedTask;
                });
                endpoints.MapGet("/account/logout", async context =>
                {
                    // signout as if the user were prompted
                    await context.SignOutAsync();

                    var logoutId = context.Request.Query["logoutId"];
                    var interaction = context.RequestServices.GetRequiredService<IIdentityServerInteractionService>();

                    var signOutContext = await interaction.GetLogoutContextAsync(logoutId);
                    
                    context.Response.Redirect(signOutContext.PostLogoutRedirectUri);
                });

                endpoints.Map("api/{**catch-all}", async context =>
                {
                    var path = context.Request.Path;
                    //await context.Response.WriteAsync(context.User.Identity.Name);
                });
            });
        }

        public async Task CreateIdentityServerSessionCookieAsync(string sub, string sid = null)
        {
            var props = new AuthenticationProperties();
            
            if (!String.IsNullOrWhiteSpace(sid))
            {
                props.Items.Add("session_id", sid);
            }
            
            await IssueSessionCookieAsync(props, new Claim("sub", sub));
        }
    }
}
