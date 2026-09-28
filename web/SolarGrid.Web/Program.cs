/*
 * File:        Program.cs
 * Author:      Shermon H (IT22177964)
 * Description: Entry point of the web app. Sets up MVC, cookie login and the
 *              ApiClient used to call the Web API. The web app is a UI layer
 *              only: all data and business logic come from the Web API.
 * Created:     28/09/2026
 */

using Microsoft.AspNetCore.Authentication.Cookies;
using SolarGrid.Web.Helpers;
using SolarGrid.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- MVC ----------
builder.Services.AddControllersWithViews(options =>
{
    // Sends the user back to login if the API says the token has expired
    options.Filters.Add<SessionExpiredFilter>();
});

// ---------- Cookie login ----------
// After a successful API login, the user's role and API token are kept in this cookie
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8); // same as the API token
        options.SlidingExpiration = false;
    });
builder.Services.AddAuthorization();

// ---------- Web API client ----------
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<ApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"]!);
});

var app = builder.Build();

// ---------- Request pipeline ----------
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
