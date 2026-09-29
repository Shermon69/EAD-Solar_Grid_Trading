/*
 * File:        ProsumerApiService.cs
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: Calls the Web API for prosumer features. The web app never
 *              touches MongoDB. Assumes Member 1 stores the JWT in session
 *              under the key "JWT" and that an HttpClient named "Api" is
 *              registered (see Program.cs).
 * Created:     29/09/2026
 */
using System.Net.Http.Headers;
using System.Net.Http.Json;
using SolarGrid.Web.Models;

namespace SolarGrid.Web.Services;

/// <summary>
/// HTTP client wrapper for all prosumer-related Web API calls. Attaches
/// the logged-in user's JWT to every request. The web client never talks
/// to MongoDB directly.
/// </summary>
public class ProsumerApiService
{
    private const string TokenKey = "JWT";
    private readonly IHttpClientFactory _factory;
    private readonly IHttpContextAccessor _http;

    // Injects the HttpClient factory and the current HTTP context.
    public ProsumerApiService(IHttpClientFactory factory, IHttpContextAccessor http)
    {
        _factory = factory;
        _http = http;
    }

    /// <summary>
    /// Creates an HttpClient that sends the logged-in user's JWT in the
    /// Authorization header. Reads the token from session.
    /// </summary>
    private HttpClient CreateClient()
    {
        var client = _factory.CreateClient("Api");
        var token = _http.HttpContext?.Session.GetString(TokenKey);

        if (!string.IsNullOrEmpty(token))
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

        return client;
    }

    /// <summary>Gets prosumers with optional search text and status filter.</summary>
    public async Task<List<ProsumerVm>> GetProsumersAsync(string? search, string? status)
    {
        var url = $"api/prosumers?search={Uri.EscapeDataString(search ?? "")}&status={Uri.EscapeDataString(status ?? "")}";
        return await CreateClient().GetFromJsonAsync<List<ProsumerVm>>(url) ?? new();
    }

    /// <summary>Gets prosumers waiting for Backoffice activation.</summary>
    public async Task<List<ProsumerVm>> GetPendingAsync()
        => await CreateClient().GetFromJsonAsync<List<ProsumerVm>>("api/prosumers/pending") ?? new();

    /// <summary>Gets one prosumer by NIC, or null if not found.</summary>
    public async Task<ProsumerVm?> GetByNicAsync(string nic)
    {
        var res = await CreateClient().GetAsync($"api/prosumers/{nic}");
        return res.IsSuccessStatusCode ? await res.Content.ReadFromJsonAsync<ProsumerVm>() : null;
    }

    /// <summary>Updates a prosumer's details through the API.</summary>
    public async Task<bool> UpdateAsync(ProsumerEditVm vm)
    {
        var res = await CreateClient().PutAsJsonAsync($"api/prosumers/{vm.Nic}", vm);
        return res.IsSuccessStatusCode;
    }

    /// <summary>Activates (approves pending or reactivates deactivated) a prosumer.</summary>
    public async Task<bool> ActivateAsync(string nic)
        => (await CreateClient().PutAsync($"api/prosumers/{nic}/activate", null)).IsSuccessStatusCode;

    /// <summary>Deactivates a prosumer account.</summary>
    public async Task<bool> DeactivateAsync(string nic)
        => (await CreateClient().PutAsync($"api/prosumers/{nic}/deactivate", null)).IsSuccessStatusCode;

    /// <summary>Gets the Backoffice dashboard counts from the API.</summary>
    public async Task<DashboardVm> GetDashboardAsync()
        => await CreateClient().GetFromJsonAsync<DashboardVm>("api/prosumers/dashboard") ?? new();
}