using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Tests
{
    public class FakeAuthenticationSessionUserAccessTokenStore : AuthenticationSessionUserAccessTokenStore
    {
        public bool AppendChallengeSchemeToTokenNames { get; private set; }

        public FakeAuthenticationSessionUserAccessTokenStore(IHttpContextAccessor contextAccessor, ILogger<AuthenticationSessionUserAccessTokenStore> logger) : base(contextAccessor, logger)
        {
        }

        protected override bool ShouldAppendChallengeSchemeToTokenNames(AuthenticateResult result,
            UserAccessTokenParameters parameters)
        {
            AppendChallengeSchemeToTokenNames = base.ShouldAppendChallengeSchemeToTokenNames(result, parameters);
            
            return AppendChallengeSchemeToTokenNames;
        }
    }

    public class AuthenticationSessionUserAccessTokenStoreTests
    {
        private const string TokenPrefix = ".Token.";
        private const string AppendChallengeSchemeToTokenNames = ".AppendChallengeSchemeToTokenNames";

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        public async Task With_AppendChallengeSchemeToTokenNames_and_ChallengeSchemwe_set_ShouldAppendChallengeSchemeToTokenNames_returns_true(bool setAppendChallengeSchemeToTokenNamesTrue, bool setChallengeScheme, bool shouldAppendChallengeSchemeToTokenNames)
        {
            var authenticationService = new Mock<IAuthenticationService>();
            var sp = new Mock<IServiceProvider>();
            sp.Setup(s => s.GetService(typeof(IAuthenticationService)))
                .Returns(() => {
                    return authenticationService.Object;
                });

            var dic = new Dictionary<string, string> {{$"{TokenPrefix}AToken", "ATokenValue"}};
            if(setAppendChallengeSchemeToTokenNamesTrue)
                dic.Add(AppendChallengeSchemeToTokenNames, "true");

            var authenticationTicket = new AuthenticationTicket(new ClaimsPrincipal(), new AuthenticationProperties(dic, null), "authScheme");
            authenticationService.Setup(a => 
                a.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .ReturnsAsync(AuthenticateResult.Success(authenticationTicket));

            var httpContext = new DefaultHttpContext();
            httpContext.RequestServices = sp.Object;

            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            var authenticationSessionUserAccessTokenStore =
                new FakeAuthenticationSessionUserAccessTokenStore(httpContextAccessor.Object,
                    NullLogger<FakeAuthenticationSessionUserAccessTokenStore>.Instance);

            var parameters = new UserAccessTokenParameters()
                {SignInScheme = "mySigninScheme"};

            if (setChallengeScheme)
                parameters.ChallengeScheme = "myChallengeScheme";

            await authenticationSessionUserAccessTokenStore.GetTokenAsync(new ClaimsPrincipal(), parameters);

            Assert.Equal(authenticationSessionUserAccessTokenStore.AppendChallengeSchemeToTokenNames, shouldAppendChallengeSchemeToTokenNames);
        }
    }
}