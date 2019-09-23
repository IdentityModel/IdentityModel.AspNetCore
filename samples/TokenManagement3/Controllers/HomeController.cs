using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MvcCode.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        [AllowAnonymous]
        public IActionResult Index() => View();

        public IActionResult Secure() => View();

        public IActionResult Logout() => SignOut("cookie", "oidc");

        public async Task<IActionResult> CallApiAsUser()
        {
            var client = _httpClientFactory.CreateClient("client");

            var response = await client.GetStringAsync("https://demo.identityserver.io/api/test");
            ViewBag.Json = JArray.Parse(response).ToString();

            return View("CallApi");
        }

        public async Task<IActionResult> CallApiAsClient()
        {
            var client = _httpClientFactory.CreateClient();
            client.SetBearerToken(await HttpContext.GetClientAccessTokenAsync());

            var response = await client.GetStringAsync("https://demo.identityserver.io/api/test");
            ViewBag.Json = JArray.Parse(response).ToString();

            return View("CallApi");
        }
    }
}