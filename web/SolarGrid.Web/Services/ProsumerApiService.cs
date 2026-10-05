/*
 * File:        ProsumerApiService.cs
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: Calls the Web API for prosumer features. The web app never
 *              touches MongoDB. Assumes Member 1 stores the JWT in session
 *              under the key "JWT" and that an HttpClient named "Api" is
 *              registered (see Program.cs).
 */
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SolarGrid.Web.Helpers;
using SolarGrid.Web.Models;

namespace SolarGrid.Web.Services;

/// <summary>
/// HTTP client wrapper for all prosumer-related Web API calls. Attaches
/// the logged-in user's JWT to every request. The web client never talks
/// to MongoDB directly.
/// </summary>
public class ProsumerApiService
{
    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _http;

    /// <summary>
    /// Injects the HttpClient factory and the current HTTP context.
    /// </summary>
    public ProsumerApiService(IHttpClientFactory factory, IHttpContextAccessor http)
    {
        _factory = factory;
        _http = http;
    }

    /// <summary>
    /// Creates an HttpClient that sends the logged-in user's JWT in the
    /// Authorization header. The token is read from the auth cookie claim
    /// (AppClaims.AccessToken), which is set by AccountController at login.
    /// </summary>
    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient("Api");

        // Token is stored as a claim in the login cookie, not in session
        var token = _http.HttpContext?.User?.FindFirst(AppClaims.AccessToken)?.Value;

        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    /// <summary>
    /// Sends a PATCH request (used for activate, deactivate).
    /// </summary>
    private async Task<bool> PatchAsync(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Patch, url);
        var response = await CreateClient().SendAsync(request);
        return response.IsSuccessStatusCode;
    }

    /// <summary>Gets prosumers with optional search text and status filter.</summary>
    public async Task<List<ProsumerVm>> GetProsumersAsync(string? search, string? status)
    {
        var url = $"prosumers?search={Uri.EscapeDataString(search ?? "")}&status={Uri.EscapeDataString(status ?? "")}";
        return await CreateClient().GetFromJsonAsync<List<ProsumerVm>>(url) ?? new();
    }

    /// <summary>Gets prosumers waiting for Backoffice activation.</summary>
    public async Task<List<ProsumerVm>> GetPendingAsync()
        => await CreateClient().GetFromJsonAsync<List<ProsumerVm>>("prosumers/pending") ?? new();

    /// <summary>Gets one prosumer by NIC, or null if not found.</summary>
    public async Task<ProsumerVm?> GetByNicAsync(string nic)
    {
        var res = await CreateClient().GetAsync($"prosumers/{nic}");
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<ProsumerVm>() : null;
    }

    /// <summary>Creates a new prosumer through the API (Backoffice only). The new
    /// account starts as Pending until Backoffice activates it.</summary>
    public async Task<bool> CreateAsync(ProsumerCreateVm vm)
    {
        var res = await CreateClient().PostAsJsonAsync("prosumers", vm);
        return res.IsSuccessStatusCode;
    }

    /// <summary>Updates a prosumer's details through the API.</summary>
    public async Task<bool> UpdateAsync(ProsumerEditVm vm)
    {
        var res = await CreateClient().PutAsJsonAsync($"prosumers/{vm.Nic}", vm);
        return res.IsSuccessStatusCode;
    }

    /// <summary>Activates (approves pending or reactivates deactivated) a prosumer.</summary>
    public async Task<bool> ActivateAsync(string nic)
        => await PatchAsync($"prosumers/{nic}/activate");

    /// <summary>Deactivates a prosumer account.</summary>
    public async Task<bool> DeactivateAsync(string nic)
        => await PatchAsync($"prosumers/{nic}/deactivate");

    /// <summary>Gets the Backoffice dashboard counts from the API.</summary>
    public async Task<DashboardVm> GetDashboardAsync()
        => await CreateClient().GetFromJsonAsync<DashboardVm>("dashboard/operator") ?? new();
}