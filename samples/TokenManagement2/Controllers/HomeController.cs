﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace AspNetCoreSecurity.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly TypedHttpClient _typedHttpClient;

        public HomeController(IHttpClientFactory httpClientFactory, TypedHttpClient typedHttpClient)
        {
            _httpClientFactory = httpClientFactory;
            _typedHttpClient = typedHttpClient;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Secure()
        {
            return View();
        }

        public async Task<IActionResult> CallApi()
        {
            var client = _httpClientFactory.CreateClient("client");
            
            var response = await client.GetStringAsync("https://demo.identityserver.io/api/test");
            ViewBag.Json = JArray.Parse(response).ToString();

            return View();
        }

        [AllowAnonymous]
        public async Task<IActionResult> CallApi2()
        {
            var client = _httpClientFactory.CreateClient("m2m");

            var response = await _typedHttpClient.ApiTest();
            ViewBag.Json = JArray.Parse(response).ToString();

            return View("CallApi");
        }

        public async Task<IActionResult> CallApiManual()
        {
            var token = await HttpContext.GetUserAccessTokenAsync();

            var client = _httpClientFactory.CreateClient();
            client.SetBearerToken(token);

            var response = await client.GetStringAsync("https://demo.identityserver.io/api/test");
            ViewBag.Json = JArray.Parse(response).ToString();

            return View("CallApi");
        }
    }
}