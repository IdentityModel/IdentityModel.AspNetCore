﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Http;

namespace AspNetCoreSecurity
{
    public class TypedHttpClient
    {
        private readonly HttpClient _httpClient;

        public TypedHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<string> ApiTest()
        {
            return _httpClient.GetStringAsync("api/test");
        }
    }

    public class Startup
    {
        public Startup()
        {
            // lame
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpClient("client")
                .AddHttpMessageHandler<UserAccessTokenHandler>();

            services.AddHttpClient<TypedHttpClient>(client =>
                {
                    client.BaseAddress = new Uri("https://demo.identityserver.io");
                })
                .AddHttpMessageHandler(provider =>
                {
                    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();

                    return new ClientAccessTokenHandler(httpContextAccessor, "m2m");
                });

            services.AddAccessTokenManagement(options =>
                {
                    options.Client.Clients.Add("m2m", new TokenClientOptions
                    {
                        Address = "https://demo.identityserver.io/connect/token",
                        ClientId = "m2m",
                        ClientSecret = "secret",
                    });
                })
                .ConfigureBackchannelHttpClient(client =>
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                });

            services.AddMvc(options =>
            {
                var global = new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .Build();

                options.Filters.Add(new AuthorizeFilter(global));

            }).SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "cookies";
                options.DefaultChallengeScheme = "oidc";
            })
                .AddCookie("cookies", options =>
                {
                    options.AccessDeniedPath = "/account/denied";
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Authority = "https://demo.identityserver.io";
                    options.ClientId = "server.hybrid.short";
                    options.ClientSecret = "secret";
                    options.ResponseType = "code id_token";

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("offline_access");
                    options.Scope.Add("api");

                    options.ClaimActions.MapAllExcept("iss", "nbf", "exp", "aud", "nonce", "iat", "c_hash");

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                });
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseDeveloperExceptionPage();

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseMvcWithDefaultRoute();
        }
    }
}