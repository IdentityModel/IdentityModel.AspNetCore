using System;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    public class ClientAccessToken
    {
        public string AccessToken { get; set; }
        public DateTimeOffset Expiration { get; set; }
    }
}