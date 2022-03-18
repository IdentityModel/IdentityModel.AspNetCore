using Azure.Core;
using System;

namespace IdentityModel.AspNetCore.AccessTokenManagement.Azure
{
    public class KeyVaultOptions
    {
        public Uri Url { get; set; }

        public TokenCredential Credential { get; set; }
    }
}
