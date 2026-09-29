/*
 * File: DashboardController.cs
 * Author: Premaratne R.A.N.C (IT22050908)
 * Description: Backoffice dashboard page showing pending prosumer count and approved future reservations count.
 * Created: 29/09/2026
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Web.Services;

namespace SolarGrid.Web.Controllers;

[Authorize(Roles = "Backoffice")]
public class DashboardController : Controller
{
    private readonly ProsumerApiService _api;

    // Receives the API service through dependency injection
    public DashboardController(ProsumerApiService api) => _api = api;

    // Shows the dashboard with counts from the API
    public async Task<IActionResult> Index()
        => View(await _api.GetDashboardAsync());
}
