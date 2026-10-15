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

## Repository

- **Git repository:** https://github.com/Shermon69/EAD-Solar_Grid_Trading
- **Demo video:** _link to be added_

## Project structure

| Folder | Description |
|--------|-------------|
| [backend/](backend/) | ASP.NET Core Web API (.NET 8), MongoDB, JWT authentication. All business logic lives here |
| [web/](web/) | ASP.NET Core MVC + Bootstrap 5 web application for Backoffice and Grid Operator users |
| [mobile/](mobile/) | Pure native Android app (Java) with SQLite, Google Maps and QR code scanning |
| [docs/](docs/) | Diagrams and screenshots |

## Tech stack

- **Web service:** C# ASP.NET Core Web API, MongoDB.Driver, JWT, hosted on Windows IIS
- **Database:** MongoDB (collections: `Users`, `SolarStationInfo`, `EnergyBookingSlots`, `EnergyReservations`)
- **Web app:** ASP.NET Core MVC (.NET 8), Bootstrap 5
- **Mobile app:** Android (Java), SQLite, Retrofit, Google Maps SDK, ZXing

## Getting started

**Web API**
1. Install the .NET 8 SDK.
2. Copy `backend/SolarGrid.Api/appsettings.Development.example.json` to `appsettings.Development.json` in the same folder,
   then fill in the MongoDB connection string and a JWT key.
3. Run:
   ```
   cd backend/SolarGrid.Api
   dotnet run
   ```
4. Open http://localhost:5080/swagger. Sample data is added automatically the first time the API runs on an empty database.

Sample logins (password `Password@123`):

| Role | NIC |
|------|-----|
| Backoffice | `199012345678` |
| Grid Operator | `199234567891` |
| Prosumer | `200045678912` |
| Prosumer (pending activation) | `200167891234` |

**Web app** (the API must be running)
```
cd web/SolarGrid.Web
dotnet run
```
Open http://localhost:5090 and log in with a Backoffice or Grid Operator account. The API address is set in
`web/SolarGrid.Web/appsettings.json` (`ApiSettings:BaseUrl`).

**Mobile app** (the API must be running)
1. Open the `mobile` folder in Android Studio and wait for Gradle sync.
2. Optional: add `MAPS_API_KEY=...` to `mobile/local.properties` (created by Android Studio, not committed).
3. Run the app on an emulator. It connects to the API at `http://10.0.2.2:5080/api/`, which is the emulator's
   address for your PC. For a real phone, add `API_BASE_URL=http://<your-PC-IP>:5080/api/` to `local.properties`.
4. Log in as a Prosumer (`200045678912`) or Grid Operator (`199234567891`).

## Team and individual contributions

| Member | IT Number | Contribution |
|--------|-----------|--------------|
| Member 1 (Team Lead) | ITXXXXXXXX | Project setup, login and role-based access, staff user management, IIS deployment |
| Member 2 | ITXXXXXXXX | Prosumer accounts, pending activations, booking views and dashboards |
| Member 3 | ITXXXXXXXX | Microgrid node and slot management, Google Maps, operator QR verification |
| Member 4 | ITXXXXXXXX | Energy reservation workflow and business rules, booking QR codes |
