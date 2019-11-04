// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityModel.AspNetCore.AccessTokenManagement
{
    /// <summary>
    /// Delegating handler that injects a client access token into an outgoing request
    /// </summary>
    public class ClientAccessTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _tokenClientName;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="httpContextAccessor">The HTTP context accessor</param>
        /// <param name="tokenClientName">The name of the token client configuration</param>
        public ClientAccessTokenHandler(IHttpContextAccessor httpContextAccessor, string tokenClientName = AccessTokenManagementDefaults.DefaultTokenClientName)
        {
            _httpContextAccessor = httpContextAccessor;
            _tokenClientName = tokenClientName;
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            await SetTokenAsync(request, forceRenewal: false);
            var response = await base.SendAsync(request, cancellationToken);

            // retry if 401
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                await SetTokenAsync(request, forceRenewal: true);
                response = await base.SendAsync(request, cancellationToken);
            }

            return response;
        }

        /// <summary>
        /// Set an access token on the HTTP request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="forceRenewal"></param>
        /// <returns></returns>
        protected virtual async Task SetTokenAsync(HttpRequestMessage request, bool forceRenewal)
        {
            var token = await _httpContextAccessor.HttpContext.GetClientAccessTokenAsync(_tokenClientName, forceRenewal);

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}