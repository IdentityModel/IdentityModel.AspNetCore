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

        private readonly string _clientName;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="httpContextAccessor"></param>
        /// <param name="clientName"></param>
        public ClientAccessTokenHandler(IHttpContextAccessor httpContextAccessor, string clientName = null)
        {
            _httpContextAccessor = httpContextAccessor;
            _clientName = clientName;
        }

        /// <inheritdoc/>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _httpContextAccessor.HttpContext.GetClientAccessTokenAsync(_clientName);

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}