using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    public interface IAccessTokenManagementService
    {
        Task<string> GetClientAccessTokenAsync(string name = null);
    }
}