using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Abstraction for managing user access tokens
    /// </summary>
    public interface IUserAccessTokenManagementService
    {
        /// <summary>
        /// Returns the user access token. If the current token is expired, it will try to refresh it.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="parameters"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<string> GetUserAccessTokenAsync(ClaimsPrincipal user, UserAccessTokenParameters parameters = null, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Revokes the current refresh token
        /// </summary>
        /// <returns></returns>
        Task RevokeRefreshTokenAsync(ClaimsPrincipal user, UserAccessTokenParameters parameters = null, CancellationToken cancellationToken = default);
    }
}