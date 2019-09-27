using System;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    public class UserAccessToken
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTimeOffset Expiration { get; set; }
    }
}
