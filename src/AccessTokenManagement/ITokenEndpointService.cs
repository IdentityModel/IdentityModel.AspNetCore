using System.Threading.Tasks;
using IdentityModel.Client;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    public interface ITokenEndpointService
    {
        Task<TokenResponse> RefreshUserAccessTokenAsync(string refreshToken);
        Task<TokenResponse> RequestClientAccessToken(string clientName = null);
        Task<TokenRevocationResponse> RevokeRefreshTokenAsync(string refreshToken);
    }
}