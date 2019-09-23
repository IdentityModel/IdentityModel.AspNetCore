using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    public class DefaultAccessTokenManagementService : IAccessTokenManagementService
    {
        private readonly TokenEndpointService _tokenEndpointService;

        public DefaultAccessTokenManagementService(TokenEndpointService tokenEndpointService)
        {
            _tokenEndpointService = tokenEndpointService;
        }

        public async Task<string> GetClientAccessTokenAsync(string name = null)
        {
            var response = await _tokenEndpointService.RequestClientAccessToken(name);

            // todo: error handling

            return response.AccessToken;
        }
    }
}