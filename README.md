# Smart Solar Microgrid Trading System

SE4040 – Enterprise Application Development | Assignment 1 | Year 4 Semester 2, 2026

A client-server system for trading solar energy on a microgrid. Backoffice staff and grid operators use a
web application, solar prosumers and grid operators use a native Android app, and both clients talk to a
central C# Web API (hosted on IIS) backed by MongoDB.

```
  Android app (Java + SQLite)  ──┐
                                 ├──►  C# Web API on IIS  ──►  MongoDB
  Web app (React + Bootstrap 5) ─┘
```

## Repository

- **Git repository:** https://github.com/Shermon69/EAD-Solar_Grid_Trading
- **Demo video:** _link to be added_

## Project structure

| Folder | Description |
|--------|-------------|
| [backend/](backend/) | ASP.NET Core Web API (.NET 8), MongoDB, JWT authentication. All business logic lives here |
| [web/](web/) | React + Bootstrap 5 web application for Backoffice and Grid Operator users |
| [mobile/](mobile/) | Pure native Android app (Java) with SQLite, Google Maps and QR code scanning |
| [docs/](docs/) | Team plan, diagrams and screenshots |

## Tech stack

- **Web service:** C# ASP.NET Core Web API, MongoDB.Driver, JWT, hosted on Windows IIS
- **Database:** MongoDB (collections: `Users`, `SolarStationInfo`, `EnergyBookingSlots`, `EnergyReservations`)
- **Web app:** React (Vite), Bootstrap 5, Axios
- **Mobile app:** Android (Java), SQLite, Retrofit, Google Maps SDK, ZXing

## Getting started

Setup steps for each part will be added to that part's folder as the project grows.
See [docs/TEAM_PLAN.md](docs/TEAM_PLAN.md) for the team plan, database design, API contract and coding standards.

## Team and individual contributions

| Member | IT Number | Contribution |
|--------|-----------|--------------|
| Member 1 (Team Lead) | ITXXXXXXXX | Project setup, login and role-based access, staff user management, IIS deployment |
| Member 2 | ITXXXXXXXX | Prosumer accounts, pending activations, booking views and dashboards |
| Member 3 | ITXXXXXXXX | Microgrid node and slot management, Google Maps, operator QR verification |
| Member 4 | ITXXXXXXXX | Energy reservation workflow and business rules, booking QR codes |
