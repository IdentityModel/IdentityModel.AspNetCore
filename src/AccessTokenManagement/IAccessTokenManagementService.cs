using IdentityModel.Client;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Abstraction for managing user and client accesss tokens
    /// </summary>
    public interface IAccessTokenManagementService
    {
        Task<string> GetUserAccessTokenAsync();
        Task RevokeRefreshTokenAsync();
        Task<TokenResponse> RefreshUserAccessTokenAsync();

        Task<string> GetClientAccessTokenAsync(string name = null);
    }
}