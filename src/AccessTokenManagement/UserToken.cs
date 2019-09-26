using System;
using System.Collections.Generic;
using System.Text;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    public class UserToken
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTimeOffset Expiration { get; set; }
    }
}
