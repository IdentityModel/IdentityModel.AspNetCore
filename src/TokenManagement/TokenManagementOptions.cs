using System;

namespace IdentityModel.AspNetCore
{
    public class TokenManagementOptions
    {
        public string Scheme { get; set; }
        public TimeSpan RefreshBeforeExpiration { get; set; } = TimeSpan.FromMinutes(1);
    }
}