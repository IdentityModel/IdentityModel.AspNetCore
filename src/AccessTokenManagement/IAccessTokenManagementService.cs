using IdentityModel.Client;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    public interface IAccessTokenManagementService
    {
        Task<string> GetUserAccessTokenAsync();
        Task RevokeRefreshTokenAsync();
        Task<TokenResponse> RefreshUserAccessTokenAsync();

        Task<string> GetClientAccessTokenAsync(string name = null);
    }
}