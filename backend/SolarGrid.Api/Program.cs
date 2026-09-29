/*
 * File:        Program.cs
 * Author:      Shermon H (IT22177964)
 * Description: Entry point of the Web API. Registers the database, services,
 *              JWT authentication and Swagger, then starts the server.
 *              When adding a new service, register it in the "Services" section.
 * Created:     28/09/2026
 */

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SolarGrid.Api.Data;
using SolarGrid.Api.Helpers;
using SolarGrid.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// ---------- Settings ----------
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDbSettings"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()!;
var mongoSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>()!;

// Stop early with a clear message if the local settings file has not been created
if (mongoSettings.ConnectionString.StartsWith("SET_IN") || jwtSettings.Key.StartsWith("SET_IN"))
{
    throw new InvalidOperationException(
        "Missing settings. Copy appsettings.Development.example.json to appsettings.Development.json " +
        "and fill in the MongoDB connection string and JWT key.");
}

// ---------- Database ----------
builder.Services.AddSingleton<MongoDbContext>();

// ---------- Helpers ----------
builder.Services.AddSingleton<JwtTokenGenerator>();

// ---------- Services (business logic) ----------
// Each member registers their own service here, e.g. builder.Services.AddScoped<StationService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ReservationService>();

// ---------- Controllers ----------
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        // Return validation errors in the same { "message": "..." } format as other errors
        options.InvalidModelStateResponseFactory = context =>
        {
            var firstError = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m)) ?? "Invalid request.";
            return new BadRequestObjectResult(new { message = firstError });
        };
    });

// ---------- JWT authentication ----------
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
        };
    });
builder.Services.AddAuthorization();

// ---------- Swagger (with an "Authorize" button for the JWT token) ----------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Smart Solar Microgrid API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Paste the token from /api/auth/login",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// ---------- Sample data ----------
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MongoDbContext>();
    await DataSeeder.SeedAsync(db);
}

// ---------- Request pipeline ----------
app.UseMiddleware<ErrorHandlingMiddleware>();

// Swagger is kept on in every environment so it can also be used on IIS for the demo
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
