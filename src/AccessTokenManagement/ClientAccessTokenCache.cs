using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    public class ClientAccessTokenCache : IClientAccessTokenCache
    {
        private readonly IDistributedCache _cache;

        public ClientAccessTokenCache(IDistributedCache cache)
        {
            _cache = cache;
        }

        public Task<ClientAccessToken> GetAsync(string clientName)
        {
            throw new NotImplementedException();
        }

        public Task PutAsync(string clientName, string accessToken, int expiresIn)
        {
            throw new NotImplementedException();
        }
    }
}
