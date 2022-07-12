using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.AspNetCore.AccessTokenManagement;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace Tests
{
    public class FakeAuthenticationSessionUserAccessTokenStore : AuthenticationSessionUserAccessTokenStore
    {
        public bool AppendChallengeSchemeToTokenNamesReturns { get; private set; }

        public FakeAuthenticationSessionUserAccessTokenStore(IHttpContextAccessor contextAccessor, ILogger<AuthenticationSessionUserAccessTokenStore> logger, UserAccessTokenManagementOptions options) : base(contextAccessor, logger, options)
        {
        }

        protected override bool AppendChallengeSchemeToTokenNames(UserAccessTokenParameters parameters)
        {
            AppendChallengeSchemeToTokenNamesReturns = base.AppendChallengeSchemeToTokenNames(parameters);
            return AppendChallengeSchemeToTokenNamesReturns;
        }
    }

    public class AuthenticationSessionUserAccessTokenStoreTests
    {
        private const string TokenPrefix = ".Token.";

        [Theory]
        [InlineData(true, true, true)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        public async Task When_ConfigOptionUseChallengeSchemeScopedTokens_and_ParametersChallengeScheme_set_AppendChallengeSchemeToTokenNames_returns_true(bool useChallengeSchemeScopedTokens, bool setChallengeScheme, bool appendChallengeSchemeToTokenNamesReturns)
        {
            var userAccessTokenManagementOptions = new UserAccessTokenManagementOptions();
            userAccessTokenManagementOptions.UseChallengeSchemeScopedTokens = useChallengeSchemeScopedTokens;

            var authenticationService = new Mock<IAuthenticationService>();
            var sp = new Mock<IServiceProvider>();
            sp.Setup(s => s.GetService(typeof(IAuthenticationService)))
                .Returns(() => authenticationService.Object);

            var dic = new Dictionary<string, string> {{$"{TokenPrefix}aToken", "aTokenValue"}};
            
            var authenticationTicket = new AuthenticationTicket(new ClaimsPrincipal(), new AuthenticationProperties(dic, null), "anAuthScheme");
            authenticationService.Setup(a => 
                a.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()))
                .ReturnsAsync(AuthenticateResult.Success(authenticationTicket));

            var httpContext = new DefaultHttpContext
            {
                RequestServices = sp.Object
            };

            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.Setup(h => h.HttpContext).Returns(httpContext);

            var authenticationSessionUserAccessTokenStore =
                new FakeAuthenticationSessionUserAccessTokenStore(httpContextAccessor.Object,
                    NullLogger<FakeAuthenticationSessionUserAccessTokenStore>.Instance,
                    userAccessTokenManagementOptions);

            var parameters = new UserAccessTokenParameters()
                { SignInScheme = "aSigninScheme", Resource = "aResource" };

            if (setChallengeScheme)
                parameters.ChallengeScheme = "aChallengeScheme";


            await authenticationSessionUserAccessTokenStore.GetTokenAsync(new ClaimsPrincipal(), parameters);

            Assert.Equal(authenticationSessionUserAccessTokenStore.AppendChallengeSchemeToTokenNamesReturns, appendChallengeSchemeToTokenNamesReturns);
        }
    }
}