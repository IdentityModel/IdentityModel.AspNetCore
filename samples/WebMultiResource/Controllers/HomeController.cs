﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

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
            var client = _httpClientFactory.CreateClient("user_client");

            var response = await client.GetStringAsync("test");
            ViewBag.Json = JArray.Parse(response).ToString();

            return View("CallApi");
        }
        
        public async Task<IActionResult> CallApiAsUserResource3()
        {
            var client = _httpClientFactory.CreateClient("user_client_resource3");

            var response = await client.GetStringAsync("test");
            ViewBag.Json = JArray.Parse(response).ToString();

            return View("CallApi");
        }

        public async Task<IActionResult> CallApiAsUserTyped([FromServices] TypedUserClient client)
        {
            var response = await client.CallApi();
            ViewBag.Json = JArray.Parse(response).ToString();

            return View("CallApi");
        }

        [AllowAnonymous]
        public async Task<IActionResult> CallApiAsClient()
        {
            var client = _httpClientFactory.CreateClient("client");

            var response = await client.GetStringAsync("test");
            ViewBag.Json = JArray.Parse(response).ToString();

            return View("CallApi");
        }

        [AllowAnonymous]
        public async Task<IActionResult> CallApiAsClientTyped([FromServices] TypedClientClient client)
        {
            var response = await client.CallApi();
            ViewBag.Json = JArray.Parse(response).ToString();

            return View("CallApi");
        }
    }
}