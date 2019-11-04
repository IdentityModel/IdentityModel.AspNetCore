using System.Net.Http;
using System.Threading.Tasks;

namespace TokenManagement3.Clients
{
    public class TypedUserClient
    {
        private readonly HttpClient _client;

        public TypedUserClient(HttpClient client)
        {
            _client = client;
        }

        public async Task<string> CallApi()
        {
            return await _client.GetStringAsync("test");
        }
    }
}