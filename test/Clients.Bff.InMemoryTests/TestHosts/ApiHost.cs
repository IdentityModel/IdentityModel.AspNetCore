// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Clients.Bff.InMemoryTests.TestFramework;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Clients.Bff.InMemoryTests.TestHosts
{
    public class ApiHost : GenericHost
    {
        private readonly ITestOutputHelper _testOutputHelper;
        public int? ApiStatusCodeToReturn { get; set; }

        private readonly IdentityServerHostTenanted _identityServerHost;
        private readonly bool _useForwardedHeaders;

        public ApiHost(IdentityServerHostTenanted identityServerHost, string scope, ITestOutputHelper testOutputHelper, string baseAddress = "https://api", bool useForwardedHeaders = false) 
            : base(testOutputHelper, baseAddress)
        {
            _identityServerHost = identityServerHost;
            _testOutputHelper = testOutputHelper;
            _useForwardedHeaders = useForwardedHeaders;

            _identityServerHost.ApiScopes.Add(new ApiScope(scope));

            OnConfigureServices += ConfigureServices;
            OnConfigure += Configure;
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
            services.AddAuthorization();

            services.AddAuthentication("token")
                .AddJwtBearer("token", options =>
                {
                    options.Authority = _identityServerHost.Url();
                    options.Audience = _identityServerHost.Url("/resources");
                    options.MapInboundClaims = false;
                    options.BackchannelHttpHandler = _identityServerHost.Server.CreateHandler();
                });
        }

        private void Configure(IApplicationBuilder app)
        {
            if (_useForwardedHeaders)
            {
                app.UseForwardedHeaders(new ForwardedHeadersOptions
                {
                    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost,
                    
                    // allow double-hop scenarios
                    ForwardLimit = null
                });
            }
            
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.Map("/{**catch-all}", async context =>
                {
                    var path = context.Request.Path;
                    if (path.ToString().Contains("cdpapitest"))
                    {
                        var claim = context.User.Claims.SingleOrDefault(c => c.Type.Equals("amr") && c.Value.Equals("urn:ietf:params:oauth:grant-type:token-exchange"));
                        
                        if (claim == null)
                        {
                            ApiStatusCodeToReturn = 401;
                            //context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            //await context.Response.WriteAsync("401");
                        }
                    }
                    if (path.ToString().Contains("api/user"))
                    {
                        var claim = context.User.Claims.SingleOrDefault(c => c.Type.Equals("scope") && c.Value.Equals("IdentityServerApi"));

                        if (claim == null)
                        {
                            ApiStatusCodeToReturn = 401;
                            //context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            //await context.Response.WriteAsync("401");
                        }
                    }

                    // capture body if present
                    var body = default(string);
                    if (context.Request.HasJsonContentType())
                    {
                        using (var sr = new StreamReader(context.Request.Body))
                        {
                            body = await sr.ReadToEndAsync();
                        }
                    }
                    
                    // capture request headers
                    var requestHeaders = new Dictionary<string, List<string>>();
                    foreach (var header in context.Request.Headers)
                    {
                        var values = new List<string>(header.Value.Select(v => v));
                        requestHeaders.Add(header.Key, values);
                    }

                    var response = new ApiResponse(
                        context.Request.Method,
                        context.Request.Path.Value,
                        context.User.FindFirst(("sub"))?.Value,
                        context.User.FindFirst(("client_id"))?.Value,
                        context.User.Claims.Select(x => new ClaimRecord(x.Type, x.Value)).ToArray())
                    {
                        Body = body,
                        RequestHeaders = requestHeaders
                    };

                    context.Response.StatusCode = ApiStatusCodeToReturn ?? 200;
                    ApiStatusCodeToReturn = null;

                    context.Response.ContentType = "application/json";

                    Microsoft.Extensions.Primitives.StringValues timeinseconds;
                    context.Request.Query.TryGetValue("seconds", out timeinseconds);
                    var timeinsecondsDidParse = Int32.TryParse(timeinseconds, out int timeinsecondsInt);
                    await Task.Delay(timeinsecondsDidParse? timeinsecondsInt : 0);
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                });
            });
        }
    }
}
