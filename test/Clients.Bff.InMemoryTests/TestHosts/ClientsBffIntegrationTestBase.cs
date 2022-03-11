// Copyright (c) Duende Software. All rights reserved.
// See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Clients.Bff.InMemoryTests.TestHosts
{
    public class ClientsBffIntegrationTestBase
    {
        protected readonly IdentityServerHostTenanted IdentityServerHostTenanted;
        protected ApiHost ApiHost;
        protected ClientsBffHost BffHost;
        protected readonly ITestOutputHelper _testOutputHelper;

        public ClientsBffIntegrationTestBase(ITestOutputHelper testOutputHelper, bool useMyAuthenticationSessionUserAccessTokenStore = false)
        {
            _testOutputHelper = testOutputHelper;
            IdentityServerHostTenanted = new IdentityServerHostTenanted(testOutputHelper);
            ICollection<string> allowedGrantTypes = new[] { GrantType.AuthorizationCode, GrantType.ClientCredentials, OidcConstants.GrantTypes.TokenExchange };

            IdentityServerHostTenanted.Clients.Add(new Client
            {
                ClientId = "spa",
                ClientSecrets = { new Secret("secret".Sha256()) },
                AllowedGrantTypes = allowedGrantTypes,
                RedirectUris = { "https://app/signin-oidc" },
                PostLogoutRedirectUris = { "https://app/signout-callback-oidc" },
                BackChannelLogoutUri = "https://app/bff/backchannel",
                AllowOfflineAccess = true,
                AllowedScopes = { $"openid", "profile", "scope1", { "cdp.api" }, { "IdentityServerApi" } }
            });
            
            #if NET6_0_OR_GREATER
            IdentityServerHost.OnConfigureServices += services => {
                services.AddTransient<IBackChannelLogoutHttpClient>(provider => 
                    new DefaultBackChannelLogoutHttpClient(
                        BffHost.HttpClient, 
                        provider.GetRequiredService<ILoggerFactory>(), 
                        provider.GetRequiredService<ICancellationTokenProvider>()));
            };
            #else
            IdentityServerHostTenanted.OnConfigureServices += services =>
            {
                services.AddTransient<IBackChannelLogoutHttpClient>(provider =>
                    new DefaultBackChannelLogoutHttpClient(
                        BffHost.HttpClient,
                        provider.GetRequiredService<ILoggerFactory>()));
            };
            #endif
            
            IdentityServerHostTenanted.InitializeAsync().Wait();

            ApiHost = new ApiHost(IdentityServerHostTenanted, "scope1", _testOutputHelper);
            ApiHost.InitializeAsync().Wait();

            BffHost = new ClientsBffHost(_testOutputHelper, IdentityServerHostTenanted, ApiHost, "spa", useMyAuthenticationSessionUserAccessTokenStore);
            BffHost.TestOutputHelper = _testOutputHelper;
            BffHost.InitializeAsync().Wait();
        }

        public async Task Login(string sub)
        {
            await IdentityServerHostTenanted.IssueSessionCookieAsync(new Claim("sub", sub));
        }
    }
}
