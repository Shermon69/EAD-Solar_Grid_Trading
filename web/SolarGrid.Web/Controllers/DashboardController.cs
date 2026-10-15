/*
 * File:        DashboardController.cs
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: Backoffice dashboard page showing pending prosumer count and
 *              approved future reservations count. Reads data from the Web API.
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Web.Services;

namespace SolarGrid.Web.Controllers;

/// <summary>
/// Backoffice-only dashboard page. Displays live counts fetched from the
/// central Web API (no direct database access from the web client).
/// </summary>
[Authorize(Roles = "Backoffice")]
public class DashboardController : Controller
{
    private readonly ProsumerApiService _api;

    // Injects the API service via DI.
    public DashboardController(ProsumerApiService api) => _api = api;

    /// <summary>Shows the dashboard with pending and approved-future counts.</summary>
    public async Task<IActionResult> Index()
        => View(await _api.GetDashboardAsync());
}