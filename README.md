# Smart Solar Microgrid Trading System

SE4040 – Enterprise Application Development | Assignment 1 | Year 4 Semester 2, 2026

A client-server system for trading solar energy on a microgrid. Backoffice staff and grid operators use a
web application, solar prosumers and grid operators use a native Android app, and both clients talk to a
central C# Web API (hosted on IIS) backed by MongoDB.

```
  Android app (Java + SQLite)          ──┐
                                         ├──►  C# Web API on IIS  ──►  MongoDB
  Web app (ASP.NET MVC + Bootstrap 5)  ──┘
```

## Links

- **Git repository:** https://github.com/Shermon69/EAD-Solar_Grid_Trading
- **Demo video:** _link to be added_

## Team

| Member | Name | IT Number | Feature area |
|--------|------|-----------|--------------|
| Member 1 (Team Lead) | Shermon H | IT22177964 | Project setup, login and roles, staff users, IIS deployment |
| Member 2 | Premaratne R.A.N.C | IT22050908 | Prosumer accounts, pending activations, dashboards and booking views |
| Member 3 | Wahundeniya W.M.V.S.B | IT22292872 | Microgrid nodes and slots, Google Maps, QR verification |
| Member 4 | Dissanayake D.M.S.N | IT22210692 | Energy reservations, booking rules, booking QR codes |

## Project structure

| Folder | Description |
|--------|-------------|
| [backend/](backend/) | ASP.NET Core Web API (.NET 8), MongoDB, JWT authentication. All business logic lives here |
| [web/](web/) | ASP.NET Core MVC + Bootstrap 5 web application for Backoffice and Grid Operator users |
| [mobile/](mobile/) | Pure native Android app (Java) with SQLite, Google Maps and QR code scanning |
| [deploy/](deploy/) | PowerShell script for hosting the Web API on IIS |

## Tech stack

- **Web service:** C# ASP.NET Core Web API, MongoDB.Driver, JWT, hosted on Windows IIS
- **Database:** MongoDB (collections: `Users`, `SolarStationInfo`, `EnergyBookingSlots`, `EnergyReservations`)
- **Web app:** ASP.NET Core MVC (.NET 8), Bootstrap 5
- **Mobile app:** Android (Java), SQLite, Retrofit, Google Maps SDK, ZXing

## Running the system

**Web API**
1. Install the .NET 8 SDK.
2. Copy `backend/SolarGrid.Api/appsettings.Development.example.json` to `appsettings.Development.json` in the same folder.
   Fill in the MongoDB connection string and a JWT key of **at least 32 characters**.
3. Run:
   ```
   cd backend/SolarGrid.Api
   dotnet run --urls http://0.0.0.0:5080
   ```
4. Open http://localhost:5080/swagger. Sample data is added automatically the first time the API runs on an empty database.

**Web app** (the API must be running)
```
cd web/SolarGrid.Web
dotnet run
```
Open http://localhost:5090. The API address is set in `web/SolarGrid.Web/appsettings.json` (`ApiSettings:BaseUrl`).

**Mobile app** (the API must be running)
1. Open the `mobile` folder in Android Studio and wait for Gradle sync.
2. Add your Google Maps key to `mobile/local.properties` as `MAPS_API_KEY=...` (this file is not committed).
3. On an emulator, the app connects to `http://10.0.2.2:5080/api/` by default. For a real phone on the same Wi-Fi,
   add `API_BASE_URL=http://<your-PC-IP>:5080/api/` to `local.properties` (keep the `/` at the end), then rebuild.

**Hosting on IIS**
1. Publish the API: `dotnet publish backend/SolarGrid.Api -c Release -o D:\inetpub\SolarGridApi`
2. In that folder, create `appsettings.Production.json` with the same MongoDB and JWT settings (IIS runs in Production mode).
3. From an administrator PowerShell, run `deploy/setup-iis.ps1`. It installs the hosting bundle, creates the app pool
   and website on port 8080, and opens the firewall port.
4. Point the clients at `http://<server>:8080/api/`.

## Sample logins

Password for all sample users: `Password@123`

| Role | NIC | Used on |
|------|-----|---------|
| Backoffice | `199012345678` | Web |
| Grid Operator | `199234567891` | Web and mobile |
| Prosumer | `200045678912` | Mobile |
| Prosumer (pending activation) | `200167891234` | Activate it from the web first |
