using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCoreSecurity
{
    public class TypedHttpClient
    {
        private readonly HttpClient _httpClient;

        public TypedHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<string> ApiTest()
        {
            return _httpClient.GetStringAsync("api/test");
        }
    }
}