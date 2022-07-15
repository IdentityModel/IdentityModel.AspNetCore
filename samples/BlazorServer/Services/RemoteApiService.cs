using System.Net;
using System.Text.Json;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorServer.Services;

public class RemoteApiService
{
    private readonly HttpClient _client;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IUserAccessTokenManagementService _tokenManagementService;

    public RemoteApiService(
        HttpClient client, 
        AuthenticationStateProvider authenticationStateProvider, 
        IUserAccessTokenManagementService tokenManagementService)
    {
        _client = client;
        _authenticationStateProvider = authenticationStateProvider;
        _tokenManagementService = tokenManagementService;
    }

    private record Claim(string type, object value);
    
    public async Task<string> GetData()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "test");
        var response = await SendRequestAsync(request);

        var json = JsonSerializer.Deserialize<IEnumerable<Claim>>(await response.Content.ReadAsStringAsync());
        return JsonSerializer.Serialize(json, new JsonSerializerOptions { WriteIndented = true });
    }

    private async Task<HttpResponseMessage> SendRequestAsync(HttpRequestMessage request)
    {
        var state = await _authenticationStateProvider.GetAuthenticationStateAsync();
        var token = await _tokenManagementService.GetUserAccessTokenAsync(state.User);

        request.SetBearerToken(token);
        return await _client.SendAsync(request);
    }
}