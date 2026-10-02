# ☀️ Member 3 & System Integration Roadmap
**Project:** Smart Solar Microgrid Trading System  
**Author / Member 3:** WMVSB Wahundeniya (IT22292872)  
**Target Deadline:** Wednesday, 30 September 2026, 11:59 PM  

---

## 🎯 Executive Overview

This document provides a detailed roadmap specifically highlighting:
1. **Member 3's Implementation Status** (Microgrid Nodes, Station Maps, and QR Code Verification).
2. **Integration Touchpoints** showing how Member 3's module connects with Member 1 (Auth & Users), Member 2 (Prosumers & Dashboards), and Member 4 (Energy Reservations).
3. **Team Integration Test Script** to run prior to final submission.

---

## 🟢 1. Member 3 Module Status & Checklist (WMVSB Wahundeniya - IT22292872)

### 📌 Ownership Scope
- **Web API**: Solar station CRUD, GPS location queries (`/nearby`), slot availability, QR verification & completion endpoints.
- **Web App**: Node management UI, slot availability grid for operators.
- **Mobile App**: Google Maps display of nearby stations, station details view, ZXing QR scanner for operators, SQLite caching for station data.
- **Report & Documentation**: DFD Level 0 and Level 1 diagrams.

---

### ✅ Completed Implementation Breakdown

#### A. Web API (`backend/SolarGrid.Api`)
- **DTOs & Models**:
  - [`StationDtos.cs`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/backend/SolarGrid.Api/DTOs/StationDtos.cs): Request/response DTOs for station creation, updates, and GPS location searches.
  - [`SlotDtos.cs`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/backend/SolarGrid.Api/DTOs/SlotDtos.cs): DTOs for battery slot availability management.
  - [`QrDtos.cs`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/backend/SolarGrid.Api/DTOs/QrDtos.cs): Request/response models for scanning and verifying QR tokens.
- **Services (Business Logic Layer)**:
  - [`StationService.cs`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/backend/SolarGrid.Api/Services/StationService.cs): Implements CRUD, Haversine formula for distance calculation (`/nearby`), and **Rule R8** enforcement (*cannot deactivate a station with active/approved reservations*).
  - [`SlotService.cs`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/backend/SolarGrid.Api/Services/SlotService.cs): Manages battery slot availability per time block.
  - [`QrVerificationService.cs`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/backend/SolarGrid.Api/Services/QrVerificationService.cs): Implements **Rule R9** (*verifies `reservationId|qrToken` pair and marks energy transfer as `Completed`*).
- **Controllers**:
  - [`StationsController.cs`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/backend/SolarGrid.Api/Controllers/StationsController.cs): Exposes `/api/stations`, `/api/stations/nearby`, `/api/stations/{id}/activate`, `/api/stations/{id}/deactivate`.
  - [`SlotsController.cs`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/backend/SolarGrid.Api/Controllers/SlotsController.cs): Exposes `/api/slots` endpoints.
  - [`QrVerificationController.cs`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/backend/SolarGrid.Api/Controllers/QrVerificationController.cs): Exposes `/api/reservations/verify-qr` and `/api/reservations/{id}/complete`.
- **Registration**: Registered in [`Program.cs`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/backend/SolarGrid.Api/Program.cs).

#### B. Web App (`web/SolarGrid.Web`)
- **Models & Controllers**:
  - [`StationViewModel.cs`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/web/SolarGrid.Web/Models/StationViewModel.cs) and [`SlotViewModel.cs`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/web/SolarGrid.Web/Models/SlotViewModel.cs).
  - [`StationsController.cs`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/web/SolarGrid.Web/Controllers/StationsController.cs) and [`SlotsController.cs`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/web/SolarGrid.Web/Controllers/SlotsController.cs).
- **Razor Views**:
  - [`Stations/Index.cshtml`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/web/SolarGrid.Web/Views/Stations/Index.cshtml): List stations, toggle active status, manage slots.
  - [`Stations/Create.cshtml`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/web/SolarGrid.Web/Views/Stations/Create.cshtml): Form to create a station with GPS coordinates and capacity.
  - [`Slots/Index.cshtml`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/web/SolarGrid.Web/Views/Slots/Index.cshtml): Manage slot availability for grid operators.

#### C. Mobile App (`mobile/app/src/main`)
- **Data Layer & SQLite Cache**:
  - [`Station.java`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/mobile/app/src/main/java/com/solargrid/app/models/Station.java): Java model for station data.
  - [`StationDao.java`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/mobile/app/src/main/java/com/solargrid/app/db/StationDao.java): SQLite DAO for offline caching of stations.
- **Activities & UI**:
  - [`MapActivity.java`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/mobile/app/src/main/java/com/solargrid/app/activities/MapActivity.java): Google Maps view plotting stations with interactive markers.
  - [`StationDetailsActivity.java`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/mobile/app/src/main/java/com/solargrid/app/activities/StationDetailsActivity.java): Displays station details and available slots.
  - [`QrScanActivity.java`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/mobile/app/src/main/java/com/solargrid/app/activities/QrScanActivity.java): Integrated ZXing barcode scanner to scan prosumer QR code, call `/verify-qr`, show customer & booking details, and mark transfer as `Completed`.
- **Navigation & Manifest**:
  - Connected in [`OperatorHomeActivity.java`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/mobile/app/src/main/java/com/solargrid/app/activities/OperatorHomeActivity.java).
  - Configured in [`AndroidManifest.xml`](file:///d:/EAD%20Assignment/EAD-Solar_Grid_Trading/mobile/app/src/main/AndroidManifest.xml).

---

### 🔲 Remaining Tasks for Member 3

| Task | Category | Location / Tool | Notes |
|---|---|---|---|
| Configure Maps Key | Setup | `mobile/local.properties` | Add `MAPS_API_KEY=your_key_here` locally |
| Merge Pull Request | Git Workflow | GitHub | PR from `feature/stations-maps` ➔ `development` |
| Draw DFD Diagrams | Report | `docs/diagrams/` | Draw DFD Level 0 and Level 1 (PNG + source) |
| Screenshots | Report | `docs/screenshots/` | Map view, station details, QR scan result |
| Individual Report Section | Report | Main Report | Write code snippets, challenges, references |

---

## 🔗 2. Module Integration Architecture

The following diagram illustrates how Member 3's module interfaces with Member 1, Member 2, and Member 4:

```
                      ┌─────────────────────────────────────────┐
                      │    Member 1: Auth & User Management     │
                      │  • JWT Token Generation                 │
                      │  • Role Authorization (Backoffice/Oper) │
                      └────────────────────┬────────────────────┘
                                           │
                                           ▼ (Bearer Token)
┌────────────────────────────────────────────────────────────────────────────────────────┐
│                        Member 3: Microgrid Nodes & Verification                        │
│                                                                                        │
│   Web API: /stations, /slots, /reservations/verify-qr, /reservations/{id}/complete     │
│   Web App: Station Mgmt, Capacity, Slot blocks                                         │
│   Mobile:  Google Maps View, SQLite Station Cache, Operator QR Scanner                 │
└──────────────┬──────────────────────────────────────────────────────────┬──────────────┘
               │                                                          │
               ▼ (Uses Stations & Slots)                                  ▼ (Verifies QR & Completes)
┌─────────────────────────────────────────┐            ┌─────────────────────────────────────────┐
│ Member 4: Energy Reservations           │            │ Member 2: Prosumers & Dashboards        │
│ • Prosumer selects Station & Slot       │            │ • Verified transfer updates Prosumer    │
│ • Upon Approval, generates `qrToken`    │            │   dashboard & booking history counts    │
│ • Mobile renders QR: `id|qrToken`       │            └─────────────────────────────────────────┘
└─────────────────────────────────────────┘
```

---

## 🤝 3. Inter-Member Integration Touchpoints

### 1. Integration with Member 1 (Auth & Roles)
- **Token Passing**: Web API endpoints created by Member 3 (`/stations`, `/slots`, `/reservations/verify-qr`) enforce `[Authorize(Roles = "GridOperator,Backoffice")]` or `[Authorize]`.
- **Mobile Session**: `QrScanActivity` and `MapActivity` retrieve the JWT token from `SessionManager` (built by Member 1) and attach it to the `Authorization: Bearer <token>` header via Retrofit `ApiClient`.

### 2. Integration with Member 2 (Prosumers & Dashboards)
- **Prosumer Data in Verification**: When Member 3's `/reservations/verify-qr` is called, the Web API fetches and returns the reservation together with Prosumer details (NIC, Full Name, Email, Phone) from the `Users` collection (managed by Member 2).
- **Dashboard Counters**: Completing a reservation via Member 3's `/complete` endpoint updates the reservation status to `Completed`, which directly feeds Member 2's prosumer & operator dashboard metrics.

### 3. Integration with Member 4 (Energy Reservations)
- **Station & Slot Selection**: Member 4's reservation creation screen consumes Member 3's `/stations` and `/stations/{id}/slots` endpoints to show active stations and open battery slots.
- **QR Handshake Protocol**:
  1. **Member 4** generates a secure `qrToken` when approving a reservation and renders a QR code on the prosumer's phone with content: `<reservationId>|<qrToken>`.
  2. **Member 3** scans the QR code using `QrScanActivity` on the operator's phone, splits the string into `reservationId` and `qrToken`, and sends them to `/reservations/verify-qr`.
  3. **Member 3** calls `/reservations/{id}/complete` to finish the energy transfer and release/update the slot status.
- **Deactivation Check (Rule R8)**: Member 3's `StationService.DeactivateAsync` checks Member 4's `EnergyReservations` collection to ensure no active (`Approved` or `Pending`) bookings exist for the station before deactivating it.

---

## 🧪 4. End-to-End Team Integration Test Script

To verify all 4 modules work together seamlessly before submission, execute the following 13-step test scenario:

| Step | Action | Executed By | Expected Result |
|---|---|---|---|
| **1** | Log into Web App as Backoffice | Member 1 | Redirects to Home Dashboard |
| **2** | Create a Grid Operator staff user | Member 1 | User created in MongoDB with role `GridOperator` |
| **3** | Create Solar Station with GPS & capacity | **Member 3** | Station saved & listed in Web App |
| **4** | Add battery slot availability for target date | **Member 3** | Slots visible in slot schedule grid |
| **5** | Register new Prosumer on Mobile App | Member 2 | Prosumer created with `Status = Pending` |
| **6** | Activate Prosumer from Web App | Member 2 | Prosumer status changes to `Active` |
| **7** | Prosumer logs in on Mobile, views Map | **Member 3** | Station marker appears on Google Maps |
| **8** | Prosumer creates booking for a slot | Member 4 | Reservation created in `Pending` state |
| **9** | Grid Operator approves reservation (Web) | Member 4 | Reservation status = `Approved`, `qrToken` generated |
| **10**| Prosumer opens booking on Mobile | Member 4 | QR code generated with `<id>|<qrToken>` |
| **11**| Grid Operator scans QR code on Mobile | **Member 3** | Server verifies QR, displays prosumer details |
| **12**| Operator taps "Complete Transfer" | **Member 3** | Status updated to `Completed` |
| **13**| Check Prosumer Dashboard & History | Member 2 | Counts updated, completed booking shown |

---

## 📋 5. Final Submission Checklist

- [ ] All feature branches merged into `development` and then `main`.
- [ ] FAT Service rule verified (0 database calls from Web or Mobile).
- [ ] Strict file headers added to every `.cs` and `.java` file.
- [ ] Screenshots collected in `docs/screenshots/`.
- [ ] 5-minute video recorded and linked in `README.md`.
- [ ] Final zip file prepared without `bin/`, `obj/`, or `build/` directories.
