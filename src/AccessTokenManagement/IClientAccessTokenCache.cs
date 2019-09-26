using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    public interface IClientAccessTokenCache
    {
        Task PutAsync(string clientName, string accessToken, int expiresIn);
        Task<ClientAccessToken> GetAsync(string clientName);
    }
}
