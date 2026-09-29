/*
 * File:        ProsumerManagementController.cs
 * Author:      Premaratne R.A.N.C (IT22050908)
 * Description: MVC pages for Prosumer Management and Pending Activations
 *              (Backoffice only). UI layer only — all data comes from the API.
 * Created:     29/09/2026
 */
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SolarGrid.Web.Models;
using SolarGrid.Web.Services;

namespace SolarGrid.Web.Controllers;

/// <summary>
/// Backoffice-only MVC controller for managing prosumer accounts:
/// listing, searching, editing, activating and deactivating.
/// Delegates all data access to the central Web API.
/// </summary>
[Authorize(Roles = "Backoffice")]
public class ProsumerManagementController : Controller
{
    private readonly ProsumerApiService _api;

    // Injects the API service via DI.
    public ProsumerManagementController(ProsumerApiService api) => _api = api;

    /// <summary>Shows all prosumers with optional search and status filter.</summary>
    public async Task<IActionResult> Index(string? search, string? status)
    {
        ViewBag.Search = search;
        ViewBag.Status = status;
        return View(await _api.GetProsumersAsync(search, status));
    }

    /// <summary>Shows prosumers waiting for Backoffice activation.</summary>
    public async Task<IActionResult> Pending()
        => View(await _api.GetPendingAsync());

    /// <summary>Shows the edit form for one prosumer (GET).</summary>
    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var p = await _api.GetByNicAsync(id);
        if (p == null) return NotFound();

        return View(new ProsumerEditVm
        {
            Nic = p.Nic,
            FullName = p.FullName,
            Email = p.Email,
            Phone = p.Phone,
            Address = p.Address
        });
    }

    /// <summary>Saves the edited prosumer through the API (POST).</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProsumerEditVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (await _api.UpdateAsync(vm))
            TempData["Success"] = "Prosumer updated.";
        else
            TempData["Error"] = "Could not update this prosumer.";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>Activates a prosumer (approve pending or reactivate deactivated).</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Activate(string id, string returnTo = "Index")
    {
        if (await _api.ActivateAsync(id))
            TempData["Success"] = "Prosumer activated.";
        else
            TempData["Error"] = "Could not activate this prosumer.";

        return RedirectToAction(returnTo == "Pending" ? nameof(Pending) : nameof(Index));
    }

    /// <summary>Deactivates a prosumer account.</summary>
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Deactivate(string id)
    {
        if (await _api.DeactivateAsync(id))
            TempData["Success"] = "Prosumer deactivated.";
        else
            TempData["Error"] = "Could not deactivate this prosumer.";

        return RedirectToAction(nameof(Index));
    }
}