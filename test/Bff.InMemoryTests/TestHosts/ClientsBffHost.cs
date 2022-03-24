// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Bff.InMemoryTests.TestFramework;
using Duende.Bff;
using Duende.Bff.Yarp;
using FluentAssertions;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Xunit.Abstractions;

namespace Bff.InMemoryTests.TestHosts
{
    public class ClientsBffHost : GenericHost
    {
        private readonly IdentityServerHostTenanted _identityServerHostTenanted;
        private readonly ApiHost _apiHost;
        private readonly string _clientId;
        private readonly bool _useForwardedHeaders;
        public BffOptions BffOptions { get; set; } = new();
        public IConfiguration Configuration { get; set; }
        public ITestOutputHelper TestOutputHelper { get; set; }
        private const string DotRedirect = "/#/onboarding/";
        private const string DotRedirectProperty = ".redirect";
        private const string CodeVerifierProperty = "code_verifier";
        private const string Xsrf = "ACMGh5UeOFGpae0Bb6p1H96d9tLBEnA--rROIs3EHNI";
        private const string OidcCodeRedirectUri = "https://app/signin-oidc";
        private const string CorrelationProperty = ".xsrf";
        private const string CodeVerifier = "kLpTQJ47bYn_tVSO87p7ZpGmMQLcXLLTtKu3wtAjByM";
        private const string CorrelationFailed = "Correlation failed.";
        private const string CorrelationFailures = ".correlation.failures";

        public ClientsBffHost(ITestOutputHelper testOutputHelper, IdentityServerHostTenanted identityServerHostTenanted, ApiHost apiHost, string clientId, bool useMyAuthenticationSessionUserAccessTokenStore = false, string baseAddress = "https://app", bool useForwardedHeaders = false)
            : base(testOutputHelper, baseAddress)
        {
            _identityServerHostTenanted = identityServerHostTenanted;
            _apiHost = apiHost;
            _clientId = clientId;
            _useForwardedHeaders = useForwardedHeaders;

            OnConfigureServices += ConfigureServices;
            OnConfigure += Configure;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            var sp = services.BuildServiceProvider();
            Configuration = sp.GetRequiredService<IConfiguration>();

            services.AddRouting();
            services.AddAuthorization();

            var bff = services.AddBff();
            services.AddSingleton(BffOptions);

            var descriptorIUserAccessTokenManagementService = services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IUserAccessTokenManagementService));
            services.Remove(descriptorIUserAccessTokenManagementService);
            services.TryAddTransient<IUserAccessTokenManagementService, FakeBffUserAccessAccessTokenManagementService>();

            services.AddSingleton(TestOutputHelper);

            bff.ConfigureTokenClient()
                .ConfigurePrimaryHttpMessageHandler(() => _identityServerHostTenanted.Server.CreateHandler());

            services.AddSingleton<IHttpMessageInvokerFactory>(
                new CallbackHttpMessageInvokerFactory(
                    path => new HttpMessageInvoker(_apiHost.Server.CreateHandler())));

            services.AddAuthentication("cookies")
                .AddCookie("cookies", options =>
                {
                    options.Cookie.Name = "bff";
                });

            _identityServerHostTenanted.BrowserClient.BaseAddress = new Uri(_identityServerHostTenanted.Url());
            services.AddSingleton(new FakeIDPTenantlessClient(_identityServerHostTenanted.BrowserClient));
            
            bff.AddServerSideSessions();
            bff.AddRemoteApis();

            services.AddAuthentication(options =>
            {
                options.DefaultChallengeScheme = "oidc";
                options.DefaultSignOutScheme = "oidc";
            })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.Events.OnRedirectToIdentityProvider = async context =>
                    {
                        
                    };
                    options.Events.OnUserInformationReceived = async context =>
                    {
                        var cookies = context.HttpContext.Request.Cookies;
                        var idpTenantlessClient = (services.BuildServiceProvider()).GetService<FakeIDPTenantlessClient>();
                        AuthSettings authSettingsNew = new AuthSettings()
                            {TenantlessClientId = "spa", TenantlessClientSecret = "secret"};
                        await FakeTenantedTokenExchange.ExchangeTokenAsync((context.ProtocolMessage.AccessToken, context.ProtocolMessage.RefreshToken), context.Properties, authSettingsNew, context.ProtocolMessage.IdToken, idpTenantlessClient);
                    };


                    options.Events.OnMessageReceived = (context) =>
                    {
                        var mess = context.ProtocolMessage;
                        return Task.CompletedTask;
                    };

                    options.Events.OnRemoteFailure = (r) =>
                    {
                        var maxFailures = 6;
                        var failuresWithSeconds = 10;

                        if (r.Failure is not { Message: CorrelationFailed }) return Task.CompletedTask;
                        var rapidFailures = 0;
                        DateTimeOffset since = DateTime.UtcNow;
                        if (r.Request.Cookies.TryGetValue(CorrelationFailures, out var correlationFailures))
                        {
                            if (correlationFailures != null)
                            {
                                var pipeIndex = correlationFailures.IndexOf('|');
                                if (pipeIndex > 0)
                                {
                                    if (int.TryParse(correlationFailures[..pipeIndex], out rapidFailures) &&
                                        long.TryParse(correlationFailures[(pipeIndex + 1)..], out var secs))
                                    {
                                        since = DateTimeOffset.FromUnixTimeSeconds(secs);
                                    }
                                }
                            }
                        }

                        rapidFailures++;
                        if (rapidFailures >= maxFailures) return Task.CompletedTask;
                        r.HandleResponse();
                        r.Response.Cookies.Append(CorrelationFailures,
                            $"{rapidFailures}|{since.ToUnixTimeSeconds()}",
                            new CookieOptions()
                            {
                                Expires = since + TimeSpan.FromSeconds(failuresWithSeconds),
                                HttpOnly = true,
                                Secure = true
                            });
                        if (r.Properties?.RedirectUri != null)
                            r.Response.Redirect(r.Properties.RedirectUri, false);

                        return Task.CompletedTask;

                    };

                    options.Authority = _identityServerHostTenanted.Url();

                    options.ClientId = _clientId;
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    options.ResponseMode = "query";
                    options.MapInboundClaims = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;
                    options.Resource = "urn:IdentityServerApi";
                    options.Scope.Clear();
                    var client = _identityServerHostTenanted.Clients.Single(x => x.ClientId == _clientId);
                    foreach (var scope in client.AllowedScopes)
                    {
                        options.Scope.Add(scope);
                    }

                    if (client.AllowOfflineAccess)
                    {
                        options.Scope.Add("offline_access");
                    }

                    options.BackchannelHttpHandler = _identityServerHostTenanted.Server.CreateHandler();
                })
                .AddOpenIdConnect("achallengescheme", options =>
                {
                    options.Events.OnMessageReceived = (context) =>
                    {
                        var mess = context.ProtocolMessage;
                        return Task.CompletedTask;
                    };

                    options.Events.OnRemoteFailure = (r) =>
                    {
                        var maxFailures = 6;
                        var failuresWithSeconds = 10;

                        if (r.Failure is not { Message: CorrelationFailed }) return Task.CompletedTask;
                        var rapidFailures = 0;
                        DateTimeOffset since = DateTime.UtcNow;
                        if (r.Request.Cookies.TryGetValue(CorrelationFailures, out var correlationFailures))
                        {
                            if (correlationFailures != null)
                            {
                                var pipeIndex = correlationFailures.IndexOf('|');
                                if (pipeIndex > 0)
                                {
                                    if (int.TryParse(correlationFailures[..pipeIndex], out rapidFailures) &&
                                        long.TryParse(correlationFailures[(pipeIndex + 1)..], out var secs))
                                    {
                                        since = DateTimeOffset.FromUnixTimeSeconds(secs);
                                    }
                                }
                            }
                        }

                        rapidFailures++;
                        if (rapidFailures >= maxFailures) return Task.CompletedTask;
                        r.HandleResponse();
                        r.Response.Cookies.Append(CorrelationFailures,
                            $"{rapidFailures}|{since.ToUnixTimeSeconds()}",
                            new CookieOptions()
                            {
                                Expires = since + TimeSpan.FromSeconds(failuresWithSeconds),
                                HttpOnly = true,
                                Secure = true
                            });
                        if (r.Properties?.RedirectUri != null)
                            r.Response.Redirect(r.Properties.RedirectUri, false);

                        return Task.CompletedTask;

                    };

                    options.Events.OnTokenValidated = context =>
                    {
                        return Task.CompletedTask;
                    };

                    options.Authority = _identityServerHostTenanted.Url();

                    options.ClientId = _clientId;
                    options.ClientSecret = "secret";
                    options.ResponseType = "code";
                    options.ResponseMode = "query";
                    options.MapInboundClaims = false;
                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;
                    options.Scope.Clear();
                    var client = _identityServerHostTenanted.Clients.Single(x => x.ClientId == _clientId);

                    options.BackchannelHttpHandler = _identityServerHostTenanted.Server.CreateHandler();
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AlwaysFail", policy => { policy.RequireAssertion(ctx => false); });
            });
        }

        private void Configure(IApplicationBuilder app)
        {
            if (_useForwardedHeaders)
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                                       ForwardedHeaders.XForwardedProto |
                                       ForwardedHeaders.XForwardedHost
                });
            }

            app.UseAuthentication();

            app.UseRouting();

            app.UseBff();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBffManagementEndpoints();

                var anapitestUrl = _apiHost.Url("/anapitest");
                endpoints.MapRemoteBffApiEndpoint(
                        "/anapitest", anapitestUrl)
                    .WithUserAccessTokenParameter(new BffUserAccessTokenParameters("cookies", "achallengescheme", true, "urn:an.api"))
                    .RequireAccessToken();

                var apiuserUrl = _apiHost.Url("/api/user");
                endpoints.MapRemoteBffApiEndpoint(
                        "/api/user", apiuserUrl)
                    .WithUserAccessTokenParameter(new BffUserAccessTokenParameters("cookies", null, true, "urn:IdentityServerApi"))
                    .RequireAccessToken();
            });


            app.Map("/invalid_endpoint",
                invalid => invalid.Use(next => RemoteApiEndpoint.Map("/invalid_endpoint", _apiHost.Url())));
        }

        public async Task<bool> GetIsUserLoggedInAsync(string userQuery = null)
        {
            if (userQuery != null) userQuery = "?" + userQuery;

            var req = new HttpRequestMessage(HttpMethod.Get, Url("/bff/user") + userQuery);
            req.Headers.Add("x-csrf", "1");
            var response = await BrowserClient.SendAsync(req);

            (response.StatusCode == HttpStatusCode.OK || response.StatusCode == HttpStatusCode.Unauthorized).Should()
                .BeTrue();

            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<List<JsonRecord>> CallUserEndpointAsync()
        {
            var req = new HttpRequestMessage(HttpMethod.Get, Url("/bff/user"));
            req.Headers.Add("x-csrf", "1");

            var response = await BrowserClient.SendAsync(req);

            response.StatusCode.Should().Be(200);
            response.Content.Headers.ContentType.MediaType.Should().Be("application/json");

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<JsonRecord>>(json);
        }

        public async Task<HttpResponseMessage> BffRemoveCookieThenLoginAsync(string sub, string correlationUri = null, string sid = null)
        {
            var cookies = BrowserClient.CookieContainer.GetCookies(new Uri(correlationUri));
            return await BffLoginAsync(sub, sid);
        }

        public async Task<HttpResponseMessage> BffLoginAsync(string sub, string sid = null)
        {
            await _identityServerHostTenanted.CreateIdentityServerSessionCookieAsync(sub, sid);
            return await BffOidcLoginAsync();
        }

        public async Task<HttpResponseMessage> BffOidcLoginAsync()
        {
            var authenticationOptions = _appServices.GetServices<IOptions<AuthenticationOptions>>();
            var scheme = authenticationOptions.FirstOrDefault()?.Value.Schemes.ToList()[1].Build();
            var authenticationHandlerProvider = _appServices.GetService<IAuthenticationHandlerProvider>();
            var httpContext = new DefaultHttpContext { RequestServices = _appServices };

            var response = await BrowserClient.GetAsync(Url("/bff/login"));
            response.StatusCode.Should().Be(302); // authorize
            response.Headers.Location.ToString().ToLowerInvariant().Should()
                .StartWith(_identityServerHostTenanted.Url("/connect/authorize"));

            response = await _identityServerHostTenanted.BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(302); // client callback
            response.Headers.Location.ToString().ToLowerInvariant().Should().StartWith(Url("/signin-oidc"));

            response = await BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(302); // root
            response.Headers.Location.ToString().ToLowerInvariant().Should().Be("/");

            var handler = authenticationHandlerProvider.GetHandlerAsync(httpContext, scheme.Name).Result;
            var properties = new AuthenticationProperties();
            properties.Items.Add(DotRedirectProperty, DotRedirect);
            properties.Items.Add(CodeVerifierProperty, CodeVerifier);
            properties.Items.Add(CorrelationProperty, Xsrf);
            properties.Items.Add(OpenIdConnectDefaults.RedirectUriForCodePropertiesKey, OidcCodeRedirectUri);
            await handler.InitializeAsync(scheme, httpContext);
            var state = ((OpenIdConnectHandler)handler).Options.StateDataFormat.Protect(properties);


            response = await BrowserClient.PostAsync("https://app/signin-oidc", new FormUrlEncodedContent(new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("code", "rfEHSA6sb63sNcHXR-MS__3Vp5YcZ7EXvXIPMYAKpbc")
                ,new KeyValuePair<string, string>("scope", "openid token_exchange.cdp_api offline_access")
                ,new KeyValuePair<string, string>("state", state)
                , new KeyValuePair<string, string>("session_state", "6QiSSWQKGP_wUCuVFwwqYadYMeje3ZTME3tCa_V9inw.nn348Pc-JjKNqqVbe2Kxqw")
            }));
            (await GetIsUserLoggedInAsync()).Should().BeTrue();

            response = await BrowserClient.GetAsync(Url(response.Headers.Location.ToString()));
            return response;
        }

        public async Task<HttpResponseMessage> BffLogoutAsync(string sid = null)
        {
            var response = await BrowserClient.GetAsync(Url("/bff/logout") + "?sid=" + sid);
            response.StatusCode.Should().Be(302); // endsession
            response.Headers.Location.ToString().ToLowerInvariant().Should()
                .StartWith(_identityServerHostTenanted.Url("/connect/endsession"));

            response = await _identityServerHostTenanted.BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(302); // logout
            response.Headers.Location.ToString().ToLowerInvariant().Should()
                .StartWith(_identityServerHostTenanted.Url("/account/logout"));

            response = await _identityServerHostTenanted.BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(302); // post logout redirect uri
            response.Headers.Location.ToString().ToLowerInvariant().Should().StartWith(Url("/signout-callback-oidc"));

            response = await BrowserClient.GetAsync(response.Headers.Location.ToString());
            response.StatusCode.Should().Be(302); // root
            response.Headers.Location.ToString().ToLowerInvariant().Should().Be("/");

            (await GetIsUserLoggedInAsync()).Should().BeFalse();

            response = await BrowserClient.GetAsync(Url(response.Headers.Location.ToString()));
            return response;
        }

        public class CallbackHttpMessageInvokerFactory : IHttpMessageInvokerFactory
        {
            public CallbackHttpMessageInvokerFactory(Func<string, HttpMessageInvoker> callback)
            {
                CreateInvoker = callback;
            }

            public Func<string, HttpMessageInvoker> CreateInvoker { get; set; }

            public HttpMessageInvoker CreateClient(string localPath)
            {
                return CreateInvoker.Invoke(localPath);
            }
        }
    }
}