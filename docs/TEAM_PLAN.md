# Team Plan – Smart Solar Microgrid Trading System

SE4040 Enterprise Application Development – Assignment 1 (Group of 4)

> **Deadline: Wednesday 30 September 2026, 11:59 PM.** Late submissions are not accepted.
> Aim to submit a few hours early so there is time to zip, check and upload.

Everyone should read this whole file once before writing any code. If something here needs to
change, tell the team lead first and we update this file so it stays the single source of truth.

---

## 1. What we are building (in one minute)

Three parts that talk to each other **only through REST API calls**:

```
  Android app (Java + SQLite)  ──┐
                                 ├──►  C# Web API on IIS  ──►  MongoDB
  Web app (React + Bootstrap 5) ─┘
```

| Part | Who uses it | Tech |
|------|-------------|------|
| **Web API** (`backend/`) | Both clients | ASP.NET Core Web API (.NET 8), MongoDB.Driver, JWT, hosted on IIS |
| **Web app** (`web/`) | Backoffice + Grid Operator | React (Vite) + Bootstrap 5, Axios |
| **Mobile app** (`mobile/`) | Prosumer + Grid Operator | Pure native Android (Java), SQLite, Google Maps, ZXing QR |

### The golden rule: FAT service
**All business logic lives in the Web API.** This alone carries marks in two criteria.

- The web and mobile apps only show screens, collect input, call the API and display the result or error.
- Rules like the 7-day window, the 12-hour notice and "cannot deactivate a node with active bookings"
  are checked **in the API**. The client may also check them for a nicer message, but the API must reject bad requests.
- Neither client ever connects to MongoDB directly.
- The Android SQLite database is only for **local persistence**: the logged-in user/session and cached
  reference data such as the station list. It is never the source of truth.

---

## 2. Team split and responsibilities

We split the work **by feature**. Each member builds their feature through all layers: the API endpoints,
the web pages and the mobile screens. This matters because **65 of the 100 marks are individual**. In the viva each
of us shows and explains our own work, and the git history must show who did what.

The four feature areas are roughly equal in size. Member 1 also does the one-time project setup at the start,
which unblocks everyone else.

### Summary

| | Member 1 – Team Lead | Member 2 | Member 3 | Member 4 |
|---|---|---|---|---|
| **Feature area** | Project Setup, Login & Staff Users | Prosumer Accounts & Dashboards | Microgrid Nodes, Maps & QR Verification | Energy Reservations |
| **API** | Skeleton, Mongo, JWT, `/auth/login`, `/users` | `/auth/register`, `/prosumers`, `/profile`, `/my/reservations`, `/dashboard` | `/stations`, `/slots`, `verify-qr`, `complete` | `/reservations` + all booking rules |
| **Web** | Skeleton, Login, Home page, Staff user mgmt | Prosumer mgmt, Pending activations, Dashboard | Node mgmt, Slot availability | Reservation mgmt + Approve |
| **Mobile** | Skeleton, SQLite session, Login, role-based home | Register, Edit profile, Deactivate, Dashboard, Booking lists, Search | Google Map, Station details, Operator QR scan | Create / Modify / Cancel booking, Summary, QR display |
| **Report** | High-level diagram, deployment steps, compile report | Database design | DFD | Use case diagram |

### Member 1 – Team Lead: Project Setup, Login & Staff User Management
**Setup (done first, pushed to `main` so everyone can start):**
- API skeleton: `SolarGrid.Api` project, folder structure, MongoDB connection (`MongoDbContext`), the 4 model classes,
  JWT authentication, CORS, Swagger, global error handler (`BusinessRuleException` → `{ message }`)
- Web skeleton: Vite + React + Bootstrap 5, routing, axios client that adds the token, layout + navbar, `ProtectedRoute` by role
- Android skeleton: project, package structure, `ApiClient` (Retrofit), `DatabaseHelper` (SQLite), `SessionManager`, manifest permissions
- MongoDB Atlas cluster + seed data (Section 5)

**Feature:**
- API: `POST /auth/login` (checks password and account status, returns a JWT with the role), `/users` endpoints for staff (Backoffice and Grid Operator users)
- Web: **Login page** with role-based redirect, **Home (index) page**, **Staff User Management** (list, create, edit, activate/deactivate Backoffice and Grid Operator users)
- Mobile: **Login screen**, saves the logged-in user and token in **SQLite**, routes to **Prosumer home** or **Operator home** by role, Logout
- Deployment: host the API on **IIS**, point both clients at it

### Member 2: Prosumer Accounts & Dashboards
- API: `POST /auth/register`, `/prosumers` endpoints (list, create, update, activate, deactivate, pending), `/profile` endpoints,
  `/my/reservations` (current / pending / history / search), `/dashboard/prosumer`, `/dashboard/operator`
- Business rules: R3 (new prosumers are Pending until activated), R4 (only Backoffice reactivates)
- Web: **Prosumer Management** (list, create, edit, deactivate/reactivate), **Pending Activations** page
  (new registrations + deactivation requests), **Dashboard** page (pending count, approved future count)
- Mobile: **Register** (NIC as primary key), **Edit profile**, **Request deactivation**, **Prosumer dashboard**
  (pending count, approved future count, upcoming bookings), **Current/Pending bookings**, **Booking history**, **Search/filter bookings**

### Member 3: Microgrid Nodes, Maps & QR Verification
- API: `/stations` endpoints (create with GPS, update, schedule, activate, deactivate, nearby), `/slots` endpoints,
  `POST /reservations/verify-qr`, `PATCH /reservations/{id}/complete`
- Business rules: R8 (cannot deactivate a node with active bookings), R9 (QR verify + complete)
- Web: **Node Management** (list, create with latitude/longitude, capacity, battery slots, edit schedule, deactivate),
  **Slot Availability** page (grid operators add/update battery slots per time block)
- Mobile: **Nearby Stations Map** (Google Maps, markers from stored lat/long, details on tap), **Station details** screen,
  caches the station list in **SQLite**, **Operator mode**: scan QR → verify with server → show details → mark as Completed

### Member 4: Energy Reservations
- API: `/reservations` endpoints (list/filter, get, create, update, cancel, approve), QR token generation on approve
- Business rules: R5 (within 7 days), R6 (12 hours' notice to update/cancel), R7 (slot must be available), R10 (cancel frees the slot)
- Web: **Reservation Management** (list with filters, create for a prosumer, update, cancel, **approve**)
- Mobile: **Create booking** (pick station → date → slot), **Modify booking**, **Cancel booking**,
  **Summary page after each action**, **Booking details with QR code** once approved

Everyone does these for their own part: UI screenshots, source code pasted into the report, individual contribution,
challenges, and references for any code taken from tutorials.

### How this maps to the marking scheme (Table 2, individual, 65 marks)

| Marking criterion | Owner(s) |
|---|---|
| Web App Features & Business Rules (18): login/roles 4, user mgmt 4, node mgmt 5, slot booking mgmt 5 | M1 (login, staff users), M2 (prosumers), M3 (nodes), M4 (bookings) |
| Mobile Authentication & Account Mgmt (9): role-based home 2, pending activation in web 2, register 3, modify 1, deactivate 1 | M1 (login + role home), M2 (the rest) |
| Reservation Workflow (9): create, update, cancel, summary page | M4 |
| Booking Views & Dashboards (10): current/pending, history, filter, pending count, approved future count | M2 |
| Grid Operator Verification & Maps (7): QR scan + finalise, nearby stations on map | M3 |
| Service Integration, SQLite, Maps, QR (12) | Everyone calls the API. SQLite: M1 (session) + M3 (station cache). Maps: M3. QR: M4 (generate) + M3 (scan) |

> ⚠️ **Viva tip:** the viva tests "your knowledge in your own app". Know your own part deeply, but
> **everyone must be able to explain the whole flow** and the FAT service idea.

---

## 3. Repository structure

```
EAD-Solar_Grid_Trading/
├── backend/
│   └── SolarGrid.Api/
│       ├── Controllers/        # Thin: receive request → call service → return result
│       ├── Services/           # ALL business rules live here
│       ├── Models/             # MongoDB documents (User, SolarStation, EnergySlot, EnergyReservation)
│       ├── DTOs/               # Request/response objects
│       ├── Data/               # MongoDbContext, settings, seed data
│       ├── Helpers/            # JWT, password hashing, exceptions
│       └── Program.cs
├── web/                        # React (Vite) + Bootstrap 5
│   └── src/
│       ├── api/                # axios client + one file per resource (usersApi.js, stationsApi.js…)
│       ├── components/         # Navbar, ProtectedRoute, shared UI
│       ├── pages/              # One folder per feature: auth/, users/, prosumers/, stations/, reservations/, dashboard/
│       └── App.jsx
├── mobile/                     # Android Studio project (Java)
│   └── app/src/main/java/com/solargrid/app/
│       ├── activities/         # One Activity per screen
│       ├── api/                # Retrofit ApiClient + ApiService interface
│       ├── db/                 # SQLiteOpenHelper + DAO classes
│       ├── models/             # Plain Java model classes
│       └── utils/              # SessionManager, DateUtils, QrUtils
├── docs/
│   ├── TEAM_PLAN.md            # This file
│   ├── diagrams/               # High-level, use case, DFD (PNG + source)
│   └── screenshots/            # All UI screenshots, named clearly
└── README.md
```

**Do not put files outside your area without telling the owner.** Shared files like `Program.cs`, `App.jsx`
(routes), `ApiService.java` and `AndroidManifest.xml` will get merge conflicts, so keep changes to them small and pull often.

---

## 4. Tools everyone needs

| Tool | Who | Notes |
|------|-----|-------|
| Git + a GitHub account | All | Commit from **your own account**. Contributions are marked from git history |
| .NET 8 SDK + Visual Studio 2022 (or VS Code) | All (API work) | `dotnet --version` should show 8.x |
| Node.js 20+ | All | For the React app |
| Android Studio (latest) + an emulator (API 30+) | All | Pick an emulator image **with Google Play** so Maps works |
| MongoDB Compass | All | To view data. We use one shared **MongoDB Atlas** free cluster |
| IIS + ASP.NET Core Hosting Bundle | M1 | Windows Features → Internet Information Services |

**Shared secrets are never committed.** M1 shares these in the team chat:
- the MongoDB Atlas connection string → put it in `backend/SolarGrid.Api/appsettings.Development.json` (gitignored)
- the Google Maps API key (M3 creates it) → put it in `mobile/local.properties` as `MAPS_API_KEY=...` (gitignored)

---

## 5. Database design (MongoDB – 4 collections)

These four collections match the marking scheme exactly. **Use these names and field names** so all our code fits together.

### `Users` (User details: Backoffice, Grid Operator and Prosumer)
| Field | Type | Notes |
|---|---|---|
| `_id` | string | **NIC**, which is the primary key (e.g. `200012345678` or `991234567V`) |
| `FullName` | string | |
| `Email` | string | |
| `Phone` | string | |
| `Address` | string | |
| `PasswordHash` | string | BCrypt hash, never plain text |
| `Role` | string | `Backoffice` \| `GridOperator` \| `Prosumer` |
| `Status` | string | `Pending` \| `Active` \| `Deactivated` |
| `DeactivationRequested` | bool | Prosumer asked to deactivate from the mobile app |
| `CreatedAt`, `UpdatedAt` | DateTime (UTC) | |

### `SolarStationInfo` (microgrid nodes / hubs)
| Field | Type | Notes |
|---|---|---|
| `_id` | ObjectId (string) | |
| `Name` | string | e.g. "Malabe Solar Hub 01" |
| `Address` | string | |
| `Latitude`, `Longitude` | double | GPS location, used by the map |
| `CapacityKw` | double | Capacity spec (kW/h) |
| `BatterySlots` | int | Total battery storage slots |
| `Schedule` | array of `{ Day, OpenTime, CloseTime }` | Operating schedule |
| `OperatorNics` | string[] | Grid operators assigned (references `Users._id`) |
| `IsActive` | bool | |
| `CreatedAt`, `UpdatedAt` | DateTime | |

### `EnergyBookingSlots` (availability per station/time)
| Field | Type | Notes |
|---|---|---|
| `_id` | ObjectId (string) | |
| `StationId` | string | References `SolarStationInfo._id` |
| `StartTime`, `EndTime` | DateTime (UTC) | e.g. 1-hour blocks |
| `TotalSlots` | int | Battery slots offered in this block |
| `AvailableSlots` | int | Grid operators update this; the API decreases it on booking |
| `IsAvailable` | bool | |

### `EnergyReservations`
| Field | Type | Notes |
|---|---|---|
| `_id` | ObjectId (string) | Booking reference |
| `ProsumerNic` | string | References `Users._id` |
| `StationId` | string | References `SolarStationInfo._id` |
| `SlotId` | string | References `EnergyBookingSlots._id` |
| `ReservationTime` | DateTime (UTC) | Start time of the slot |
| `Type` | string | `Charging` \| `Dropoff` (buy energy / sell energy) |
| `EnergyKwh` | double | Amount of energy |
| `Status` | string | `Pending` \| `Approved` \| `Completed` \| `Cancelled` |
| `QrToken` | string | Random GUID, created by the API when approved |
| `CompletedBy` | string | Operator NIC who scanned the QR code |
| `CreatedAt`, `UpdatedAt`, `CompletedAt` | DateTime | |

**Seed data (M1):** 1 Backoffice, 2 Grid Operators, 3 Prosumers, 4–5 stations around Colombo/Malabe with real lat/long,
slots for the next 7 days, and a few reservations in each status. The marking scheme gives marks for sample data.

---

## 6. Business rules (all enforced in the API services)

| # | Rule | Where | Error |
|---|------|-------|-------|
| R1 | Only **Backoffice** can manage web users, prosumers and create/deactivate nodes | `[Authorize(Roles="Backoffice")]` | 403 |
| R2 | **Grid Operators** can update slot availability and manage/approve/cancel bookings | `[Authorize(Roles="Backoffice,GridOperator")]` | 403 |
| R3 | A prosumer who registers on mobile gets `Status = Pending` and must be **activated by Backoffice** in the web app before they can log in | `AuthService` / `UserService` | 403 "Account pending activation" |
| R4 | A **Deactivated** account can only be reactivated by Backoffice | `UserService.Activate` | 403 |
| R5 | Reservation time must be **in the future and within 7 days** from now | `ReservationService.Create/Update` | 400 |
| R6 | Update or cancel only if **at least 12 hours** remain before `ReservationTime` | `ReservationService.Update/Cancel` | 400 |
| R7 | Cannot book a slot with `AvailableSlots == 0` or at an inactive station | `ReservationService.Create` | 400 |
| R8 | A node **cannot be deactivated** while it has `Pending`/`Approved` future reservations | `StationService.Deactivate` | 409 |
| R9 | QR shown only for `Approved` bookings; scanning verifies `QrToken` with the server; completing sets `Completed` | `ReservationService.VerifyQr/Complete` | 404/400 |
| R10 | Cancelling a booking gives the slot back (`AvailableSlots + 1`) | `ReservationService.Cancel` | – |

Error responses always look like: `{ "message": "Reservations can only be cancelled at least 12 hours in advance." }`
Clients just show `message` in an alert/toast.

---

## 7. API contract

Base URL: `http://<server>/api`. All endpoints except login/register need the header `Authorization: Bearer <token>`.

**Auth & Staff Users – M1**
| Method | Endpoint | Who | Purpose |
|---|---|---|---|
| POST | `/auth/login` | All | `{ nic, password }` → `{ token, nic, fullName, role }` |
| GET | `/users?role=` | Backoffice | List staff users (Backoffice, GridOperator) |
| GET | `/users/{nic}` | Backoffice | Get one |
| POST | `/users` | Backoffice | Create Backoffice / GridOperator user |
| PUT | `/users/{nic}` | Backoffice | Update |
| PATCH | `/users/{nic}/activate` | Backoffice | Activate staff user |
| PATCH | `/users/{nic}/deactivate` | Backoffice | Deactivate staff user |

**Prosumers, Profile & Dashboards – M2**
| Method | Endpoint | Who | Purpose |
|---|---|---|---|
| POST | `/auth/register` | Prosumer (mobile) | Creates prosumer with `Status=Pending` |
| GET | `/prosumers?status=&search=` | Backoffice, GridOperator | List/filter prosumers |
| GET | `/prosumers/pending` | Backoffice | Pending activations + deactivation requests |
| GET | `/prosumers/{nic}` | Backoffice, GridOperator | Get one |
| POST | `/prosumers` | Backoffice | Create prosumer from the web |
| PUT | `/prosumers/{nic}` | Backoffice | Update |
| PATCH | `/prosumers/{nic}/activate` | Backoffice **only** | Activate / reactivate (R3, R4) |
| PATCH | `/prosumers/{nic}/deactivate` | Backoffice | Deactivate |
| GET | `/profile` | Logged-in user | My profile |
| PUT | `/profile` | Logged-in user | Edit my profile |
| POST | `/profile/deactivate-request` | Prosumer | Request deactivation |
| GET | `/my/reservations?status=&search=` | Prosumer | Current / pending / history / search |
| GET | `/dashboard/prosumer` | Prosumer | `{ pendingCount, approvedFutureCount, upcoming[] }` |
| GET | `/dashboard/operator` | Backoffice, GridOperator | Same counts across all stations |

**Stations, Slots & QR Verification – M3**
| Method | Endpoint | Who | Purpose |
|---|---|---|---|
| GET | `/stations` | All | List (`?activeOnly=true`) |
| GET | `/stations/{id}` | All | Details |
| GET | `/stations/nearby?lat=&lng=&radiusKm=` | All | For the map |
| POST | `/stations` | Backoffice | Create with GPS, capacity, battery slots |
| PUT | `/stations/{id}` | Backoffice | Update details + schedule |
| PATCH | `/stations/{id}/deactivate` | Backoffice | Blocked if active reservations (R8) |
| PATCH | `/stations/{id}/activate` | Backoffice | Reactivate |
| GET | `/stations/{id}/slots?date=` | All | Slots for a day |
| POST | `/slots` | Backoffice, GridOperator | Create slot block |
| PUT | `/slots/{id}` | Backoffice, GridOperator | Update availability |
| DELETE | `/slots/{id}` | Backoffice, GridOperator | Remove (only if no bookings) |
| POST | `/reservations/verify-qr` | GridOperator | `{ reservationId, qrToken }` → reservation + prosumer details |
| PATCH | `/reservations/{id}/complete` | GridOperator | Finalise energy transfer → `Completed` |

**Reservations – M4**
| Method | Endpoint | Who | Purpose |
|---|---|---|---|
| GET | `/reservations?status=&stationId=&nic=` | Backoffice, GridOperator | List/filter |
| GET | `/reservations/{id}` | Owner, staff | Details (includes `qrToken` if Approved) |
| POST | `/reservations` | Prosumer, staff | Create (R5, R7) |
| PUT | `/reservations/{id}` | Owner, staff | Update (R5, R6) |
| PATCH | `/reservations/{id}/cancel` | Owner, staff | Cancel (R6, R10) |
| PATCH | `/reservations/{id}/approve` | Backoffice, GridOperator | Approve + generate `QrToken` |

**QR content format:** the mobile app encodes `reservationId|qrToken` as the QR text. The operator app splits the text on `|` and calls `verify-qr`.

> M3's QR endpoints go in their own `QrVerificationController` (same `/reservations/...` URLs), so M3 and M4 don't edit the same file.

---

## 8. Coding standards (MANDATORY: code without these is not marked)

### 8.1 Header block on **every** `.cs` file (use the same style in `.java` and `.jsx` files)
```csharp
/*
 * File:        ReservationService.cs
 * Author:      Jane Perera (IT21XXXXXX)
 * Description: Business logic for energy reservations, including the
 *              7-day booking window and 12-hour change/cancel rule.
 * Created:     29/09/2026
 */
```

### 8.2 Comment at the start of **every** method
```csharp
/// <summary>
/// Cancels a reservation if at least 12 hours remain before it starts,
/// then returns the battery slot to the station.
/// </summary>
public async Task CancelAsync(string id, string userNic) { ... }
```
Java: use `/** ... */` Javadoc. JS: a `//` line above each function or component.

### 8.3 Referencing code you didn't write
If you copy or adapt code from a tutorial, Stack Overflow or docs, **comment the source right above it**:
```java
// Adapted from: https://github.com/journeyapps/zxing-android-embedded (README – scanning example)
```
Unreferenced copied code counts as plagiarism, which means **zero marks**. Also add it to the report's reference list.

### 8.4 Keep it simple
- Controllers stay thin: validate the input shape, call the service, return a result.
- Services hold the rules. Throw a small custom exception (e.g. `BusinessRuleException`) and turn it into a 400/409 with `{ message }` in one place.
- Store dates in **UTC** in the API. Clients convert to local time only for display.
- Clear names: `CreateReservationAsync`, not `DoStuff`.

---

## 9. Git workflow

Our commit history is **marked** ("meaningful, descriptive commits"), so please follow this.

1. **Clone once:** `git clone https://github.com/Shermon69/EAD-Solar_Grid_Trading.git`
2. **Always start from an up-to-date main:**
   ```
   git checkout main
   git pull
   git checkout -b feature/<area>-<short-name>
   ```
   Branch examples: `feature/project-setup`, `feature/prosumer-accounts`, `feature/stations-maps`, `feature/reservations`
3. **Commit small and often**, with a plain sentence that says what changed:
   - ✅ `Add reservation cancel endpoint with 12 hour check`
   - ✅ `Create station list page with Bootstrap table`
   - ✅ `Show nearby stations on Google Map`
   - ❌ `update`, `fix`, `asdf`, `final final 2`
4. **Push and open a Pull Request** to `main`. Tell the team in the chat. Merge once it builds.
   Pull main into your branch often (`git pull origin main`) to avoid big conflicts.
5. **Never commit:** `bin/`, `obj/`, `node_modules/`, `build/`, `.env`, connection strings, API keys (the `.gitignore` handles most of this).
6. **Use your own GitHub account.** Check with `git config user.name` / `git config user.email`.

---

## 10. Work phases

Finish each phase before moving on. Merge working code into `main` at the end of every phase.

### Phase 1 – Setup (everyone unblocked)
| Who | Task |
|---|---|
| All | Install tools (Section 4), clone repo, read this file, set git identity |
| M1 | Create MongoDB Atlas cluster, share connection string. Push the **API, web and Android skeletons** (see M1 in Section 2) to `main`, then tell the team |
| M2 | While waiting: sketch the prosumer screens (register, profile, dashboard) and the web prosumer pages |
| M3 | Create the **Google Maps API key** (needs a Google Cloud account, so do this early). Collect real lat/long for 4–5 sample stations |
| M4 | While waiting: write out the reservation rules (R5–R7, R10) and sketch the booking screens and web reservation page |

Once the skeletons are on `main`, everyone pulls, creates their feature branch and starts Phase 2.

### Phase 2 – API features
Everyone builds their API endpoints (Section 7) and tests them in **Swagger** before building UI.

### Phase 3 – Web and mobile screens
Build web pages and mobile screens on top of the working endpoints.

### Phase 4 – Integration testing
Run the full scenario in Section 11 together, end to end. List bugs in the group chat and fix them.

### Phase 5 – Polish and deploy
UI polish (consistent Bootstrap styling, responsive), seed data, M1 deploys the API to **IIS** and both clients point to it.

### Phase 6 – Documentation and submission
Screenshots → `docs/screenshots/`, ≤ 5 min video, report sections (Section 12), README update, zip and submit
**well before 30 Sep, 11:59 PM**.

**If you are stuck for more than 30 minutes, say so in the group chat.**

---

## 11. End-to-end demo scenario (our test script and the video script)

1. Backoffice logs into the web app → sees the Home dashboard → creates a Grid Operator user.
2. Backoffice creates a new solar station with GPS location, capacity and battery slots, and sets its schedule.
3. Grid Operator logs into the web app → adds/updates battery slot availability for the next days.
4. Prosumer registers in the **mobile app** with NIC → sees "pending activation".
5. Backoffice opens **Pending Activations** in the web app → activates the prosumer.
6. Prosumer logs in on mobile → role-based home/dashboard → views **nearby stations on Google Maps**.
7. Prosumer creates a booking → **summary page** → modifies it → summary page.
   Try booking 8 days ahead → rejected (7-day rule).
8. Grid Operator approves the booking (web) → prosumer sees it as Approved with a **QR code**.
9. Grid Operator logs into the **mobile app** (operator home) → **scans the QR** → server verifies → marks **Completed**.
10. Prosumer checks **history** and **search**, and the dashboard counts update.
11. Try cancelling a booking less than 12 hours before it starts → rejected (12-hour rule).
12. Backoffice tries to deactivate a station with active bookings → blocked.
13. Prosumer requests deactivation → Backoffice sees the request → deactivates → only Backoffice can reactivate.

---

## 12. Deliverables checklist

### Submission zip (named with IT number, e.g. `IT21XXXXXX.zip`)
- [ ] All project folders (`backend/`, `web/`, `mobile/`) with **no** `bin/`, `obj/`, `node_modules/`, `build/`
- [ ] The report (PDF + DOCX)
- [ ] A screenshot of the main opening screen of the app
- [ ] README.md with repo link, individual contributions and video link

### Report sections (owner)
- [ ] Introduction + system overview (M1)
- [ ] High-level architecture diagram (M1)
- [ ] Use case diagram (M4)
- [ ] DFD, level 0 + level 1 (M3)
- [ ] Database design: 4 collections, fields, relationships, sample documents (M2)
- [ ] Screenshots of **all** UIs, each with a caption (each member for their own screens)
- [ ] Source code **pasted as text**, not screenshots (each member for their own files)
- [ ] IIS hosting / deployment steps (M1)
- [ ] Git repository link (M1)
- [ ] Individual contribution: a **specific** list per member, not identical text (each member)
- [ ] Challenges: honest reflection, what went wrong and how we fixed it (each member)
- [ ] References, one consistent style e.g. IEEE (each member sends theirs to M1)

### Video (≤ 5 minutes)
Follow the scenario in Section 11. Each member narrates their own part (~1 min each) so contributions are clear.

### Before the viva
- [ ] Everyone can run the whole system on their own laptop
- [ ] Everyone can explain: FAT service, why SQLite is only local, how JWT/roles work, how the 7-day/12-hour rules are enforced, how QR verification works
- [ ] Viva attendance is **compulsory**. If you're absent, your assignment isn't marked

---

## 13. Useful tips (save time)

- **Android emulator → local API:** use `http://10.0.2.2:<port>/api/`, not `localhost`. For a real phone, use the PC's LAN IP.
- **Android HTTP (not HTTPS):** add `android:usesCleartextTraffic="true"` in the manifest for local testing.
- **"No frameworks" on Android** means no Flutter/React Native/Xamarin. Normal libraries (Retrofit, Google Maps SDK, ZXing) are fine. Use plain `SQLiteOpenHelper` for SQLite so it's clearly "SQLite".
- **CORS:** the API must allow the React dev server origin (`http://localhost:5173`), or the web app can't call it.
- **Swagger** (`/swagger`) is the fastest way to test your endpoints before building UI.
- **Dates:** send ISO strings (`2026-09-30T10:00:00Z`) between clients and API.
- **Suggested libraries:** API: `MongoDB.Driver`, `BCrypt.Net-Next`, `Microsoft.AspNetCore.Authentication.JwtBearer`. Web: `react-router-dom`, `axios`, `bootstrap`. Android: `retrofit2` + `converter-gson`, `play-services-maps`, `play-services-location`, `com.journeyapps:zxing-android-embedded`.
