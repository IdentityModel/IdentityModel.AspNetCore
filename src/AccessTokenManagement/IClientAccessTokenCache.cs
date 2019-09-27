using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Abstraction for caching client access tokens
    /// </summary>
    public interface IClientAccessTokenCache
    {
        /// <summary>
        /// Caches a client access token
        /// </summary>
        /// <param name="clientName"></param>
        /// <param name="accessToken"></param>
        /// <param name="expiresIn"></param>
        /// <returns></returns>
        Task SetAsync(string clientName, string accessToken, int expiresIn);
        
        /// <summary>
        /// Retrieves a client access token from the cache
        /// </summary>
        /// <param name="clientName"></param>
        /// <returns></returns>
        Task<ClientAccessToken> GetAsync(string clientName);
    }
}