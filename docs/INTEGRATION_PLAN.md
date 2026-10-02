# Integration Plan – Smart Solar Microgrid Trading System

> **Deadline: Tuesday 6 October 2026, 11:59 PM.** We submit on **6 Oct by 6:00 PM** at the latest.
> All feature branches are merged into `development`. This plan takes us from "features merged" to "submitted".

Read this once, then do your tasks in Section 3 for the day. Tick the boxes in your own PR or tell the group when done.

---

## 1. Where we are (2 Oct)

| Part | Status |
|---|---|
| Web API | Builds. All endpoints in TEAM_PLAN §7 exist. Rules R3–R10 enforced in the services |
| Web app | Builds. All pages exist |
| Mobile app | **Did not compile** on `development` – fixed on `fix/integration` (see below) |
| Docs / report / video | Not started |
| IIS hosting | Not started |

### Fixed on `fix/integration` (M1)
- `QrScanActivity` used private `Reservation` fields and a missing `scheduledDate` → now uses the getters
- `MapActivity` called `ApiClient.getClient()`, which does not exist → now `ApiClient.getService(this)`
- Prosumer home: **Dashboard** and **Profile** cards were reset to "coming soon" by a merge → linked again to `DashboardActivity` and `ProfileActivity`
- `GET /api/prosumers` and `GET /api/prosumers/{nic}` were Backoffice-only, so Grid Operators got 403 on **Reservations → Create** in the web app → now Backoffice + GridOperator (as in TEAM_PLAN §7)
- Missing header block on `CreateReservationViewModel.cs`
- `.gitignore`: `appsettings.Production.json` and Java crash logs

---

## 2. Branch rules for the last 4 days

```
fix/integration ──► development ──► main (only on 6 Oct, final version)
fix/<your-area>  ──►      ▲
```

1. After `fix/integration` is merged: `git checkout development` → `git pull` → `git checkout -b fix/<area>-<short-name>`
   (e.g. `fix/prosumer-create`, `fix/stations-comments`, `fix/reservations-comments`).
2. **Before opening a PR**, all three must build (Section 6). A PR that breaks the mobile build is not merged.
3. Small PRs, merged the same day. Pull `development` before you start each day.
4. Only touch your own files. If you must change someone else's file, tell them first (that is how the Dashboard link got lost).
5. **Code freeze: Monday 5 Oct, 8:00 PM.** After that only bug fixes from the test run, and only with the group's OK.

---

## 3. Tasks by day

### Fri 2 Oct – Unblock
| Who | Task |
|---|---|
| M1 | Merge `fix/integration` into `development`, post in the group |
| All | Pull `development`, run API + web locally, compile mobile (Section 6). Report anything that does not work |
| M3 | Add each member's **debug SHA-1** to the Maps key restrictions in Google Cloud (or remove the Android-app restriction until submission), otherwise the map is blank on other laptops |

### Sat 3 Oct – Finish features, comments and hosting

**M1 – Shermon H (IT22177964)**
- [ ] Host the API on IIS (Section 5) and share the URL
- [ ] Point the web app (`ApiSettings:BaseUrl`) and mobile (`API_BASE_URL`) at it
- [ ] Fill in README: real names + IT numbers, contribution lines, run instructions for the IIS build

**M2 – Premaratne R.A.N.C (IT22050908)**
- [ ] **Create prosumer from the web.** The brief requires "Create, update, and deactivate prosumer profiles"; we only have update/deactivate
  - API: `POST /api/prosumers` (Backoffice) in `ProsumersController` → `ProsumerService.CreateAsync`: validate NIC (`NicValidator`), reject duplicate NIC, hash password, `Role = Prosumer`, `Status = Active` (created by Backoffice, so no activation needed)
  - Web: `Prosumers/Create` page + "Add prosumer" button on the Prosumers list
- [ ] Pending Activations page: also list **deactivation requests** (`DeactivationRequested == true`) with a Deactivate button (TEAM_PLAN §2)
- [ ] Comment above the constructor in `backend/.../Controllers/ProsumersController.cs`
- [ ] Database design section of the report (4 collections, fields, relationships, a sample document each)

**M3 – WMVSB Wahundeniya (IT22292872)**
- [ ] Method comments in `MapActivity` (`onCreate`, `onMapReady`, `onRequestPermissionsResult`, both callbacks), `QrScanActivity` (`onCreate`, 4 callbacks), `StationDetailsActivity` (`onCreate`), `Station()` and `QrVerifyRequest(...)` constructors
- [ ] `backend/.../Controllers/StationsController.cs`: put the `/// <summary>` comment **above** the `[HttpGet]` attribute, not below it
- [ ] Map: refresh the marker list when you come back to the screen; check the station details screen opens from a marker
- [ ] DFD level 0 + level 1 → `docs/diagrams/`

**M4 – Dissanayake D.M.S.N (IT22210692)**
- [ ] Method comments on all Retrofit `onResponse`/`onFailure` and spinner `onItemSelected`/`onNothingSelected` callbacks in `CreateBookingActivity`, `EditReservationActivity`, `MyReservationsActivity`, `ReservationDetailsActivity`
- [ ] Javadoc on the getters/constructors in `models/Reservation.java`, `models/CreateReservationRequest.java`, `models/Slot.java`, and the constructor of the web `ReservationsController`
- [ ] Use case diagram → `docs/diagrams/`

> Tip: in Java a one-line `/** Called when the API returns the station list. */` above each callback is enough. Every method needs one.

### Sun 4 Oct – Full end-to-end test (all 4 together, on the IIS build)

Run every step in Section 4 on **the IIS-hosted API**, with the web app and the mobile app on a real phone.
One person drives, one person writes results. Every failure goes in the bug log (Section 4.1) with an owner.
Fix bugs the same day in small `fix/...` PRs, then re-run only the failed steps.

### Mon 5 Oct – Evidence (code freeze 8:00 PM)
| Who | Task |
|---|---|
| All | Re-seed fresh data (Section 5, step 9), then take **screenshots of all your own screens** → `docs/screenshots/` named `<member>-<platform>-<screen>.png` (e.g. `m4-mobile-create-booking.png`) |
| All | Your report sections: screenshots with captions, **source code pasted as text**, individual contribution (specific list), challenges, references. Send to M1 by 6:00 PM |
| All | Record your ~1 min of the video (follow Section 4 order). M1 joins the clips, ≤ 5 min total, uploads to YouTube (unlisted) or OneDrive |
| M1 | High-level architecture diagram, IIS deployment steps, compile the report (DOCX + PDF) |

### Tue 6 Oct – Submit
- [ ] M1: put the video link in README, final commit on `development`
- [ ] M1: PR `development` → `main`, merge
- [ ] M1: fresh clone of `main`, check it builds, then zip **without** `bin/ obj/ build/ .gradle/ .idea/ .vs/ local.properties appsettings.Development.json`
- [ ] Zip contains: all project folders, report (PDF + DOCX), screenshot of the main opening screen, README
- [ ] Zip named with IT number (e.g. `IT22177964.zip`), **submitted by 6:00 PM**
- [ ] Everyone: practise explaining your own part + the whole flow for the viva (TEAM_PLAN §12 "Before the viva")

---

## 4. End-to-end test script

Use the IIS API. Sample password for seeded users: `Password@123`.

| # | Step | Client | Expected | Owner | Pass? |
|---|---|---|---|---|---|
| 1 | Log in as Backoffice `199012345678` | Web | Backoffice home; staff menu visible | M1 | |
| 2 | Log in as Grid Operator `199234567891` | Web | Operator home; **no** Users / Prosumers admin menu; typing `/Users` → Access denied | M1 | |
| 3 | Backoffice creates a Grid Operator, edits it, deactivates it, then that user tries to log in | Web | Created/updated; deactivated user cannot log in | M1 | |
| 4 | Backoffice creates a station with lat/long, capacity, battery slots, schedule; edits the schedule | Web | Saved and listed | M3 | |
| 5 | Grid Operator adds slot blocks for the next days, changes availability | Web | Slots listed with correct counts | M3 | |
| 6 | Backoffice creates a prosumer from the web | Web | Created as Active | M2 | |
| 7 | Prosumer registers on mobile with a new NIC | Mobile | "Pending activation"; login is refused | M2 | |
| 8 | Backoffice opens **Pending Activations** and activates them | Web | Prosumer can now log in | M2 | |
| 9 | Prosumer logs in | Mobile | **Prosumer home**; session survives closing/reopening the app (SQLite) | M1 | |
| 10 | Grid Operator logs in on mobile | Mobile | **Operator home** (not prosumer home) | M1 | |
| 11 | Prosumer opens **Nearby stations** → taps a marker → details | Mobile | Markers at the stored lat/long, details shown | M3 | |
| 12 | Turn off Wi-Fi, open the map again | Mobile | Cached stations from SQLite shown | M3 | |
| 13 | Prosumer creates a booking | Mobile | **Summary page**; status Pending; slot's available count drops by 1 on web | M4 | |
| 14 | Try booking **8 days ahead** | Mobile + Web | Rejected with the 7-day message | M4 | |
| 15 | Prosumer modifies the booking (> 12 h away) | Mobile | Summary page shows new details | M4 | |
| 16 | Grid Operator approves it | Web | Approved; QR token created | M4 | |
| 17 | Prosumer opens the booking | Mobile | **QR code** shown | M4 | |
| 18 | Grid Operator scans the QR on the operator phone → Complete | Mobile | Server verifies; status Completed; scanning again is rejected | M3 | |
| 19 | Prosumer checks Dashboard, current/pending list, **history**, **search** | Mobile | Counts and lists updated live | M2 | |
| 20 | Backoffice dashboard | Web | Pending count + approved future count correct | M2 | |
| 21 | Cancel a booking **< 12 h** before start (use a seeded one) | Mobile + Web | Rejected with the 12-hour message | M4 | |
| 22 | Cancel a booking > 12 h away | Mobile | Cancelled; slot count goes back up by 1 | M4 | |
| 23 | Grid Operator creates, edits and cancels a booking for a prosumer | Web | All work | M4 | |
| 24 | Backoffice deactivates a station with active bookings | Web | **Blocked** with a message | M3 | |
| 25 | Prosumer edits profile, then requests deactivation | Mobile | Saved; request visible to Backoffice | M2 | |
| 26 | Backoffice deactivates them; Grid Operator tries to reactivate | Web | Only Backoffice can reactivate | M2 | |
| 27 | Stop the API, use each client | Web + Mobile | Friendly error message, no crash | All | |

### 4.1 Bug log (post in the group in this format)
```
#3 | Step 15 | Mobile | Edit booking crashes when changing station | Owner: M4 | Status: Fixed in PR #12
```

---

## 5. IIS hosting (M1)

The API reads the Mongo connection string and JWT key from `appsettings.Development.json`, which does **not** load on IIS
(IIS runs as *Production*). Without step 4 the API stops at startup with "Missing settings".

1. Windows Features → enable **Internet Information Services** (+ *World Wide Web Services*).
2. Install the **ASP.NET Core 8 Hosting Bundle**, then run `iisreset`.
3. Publish (to D: because C: is nearly full):
   `dotnet publish backend/SolarGrid.Api -c Release -o D:\inetpub\SolarGridApi`
4. In `D:\inetpub\SolarGridApi`, create `appsettings.Production.json` with the same `MongoDbSettings` and `JwtSettings`
   as your `appsettings.Development.json`. It is gitignored. **Never commit it.**
5. IIS Manager → Application Pools → add `SolarGridApi`, **.NET CLR version: No Managed Code**.
6. Sites → Add Website: name `SolarGridApi`, path `D:\inetpub\SolarGridApi`, pool `SolarGridApi`, port **8080**
   (5080 stays free for `dotnet run`). Give the pool identity read access to the folder.
7. Windows Firewall → inbound rule for TCP 8080 (so the phone can reach it). MongoDB Atlas → Network Access → allow this PC's IP.
8. Check `http://localhost:8080/swagger` and log in through Swagger.
9. Fresh demo data: the seeder only runs on an **empty** database and adds slots for the 7 days after it runs.
   Before screenshots/video/viva: drop the database in Compass, then restart the IIS site → fresh users, stations, slots and bookings.
10. Clients:
    - Web: `web/SolarGrid.Web/appsettings.json` → `ApiSettings:BaseUrl` = `http://localhost:8080/api/`
    - Mobile: `mobile/local.properties` → `API_BASE_URL=http://<PC Wi-Fi IP>:8080/api/` (phone and PC on the same Wi-Fi)
11. Screenshot each step for the report's deployment section.

---

## 6. Before every PR: build check

```
dotnet build backend/SolarGrid.Api
dotnet build web/SolarGrid.Web
cd mobile
gradlew.bat :app:compileDebugJavaWithJavac
```
The last command compiles the Android app **without an emulator**. If `gradlew` complains about Java, first run
`$env:JAVA_HOME = "C:\Program Files\Android\Android Studio\jbr"` (PowerShell).

Also check:
- [ ] Header block at the top of every new file, comment above every method (TEAM_PLAN §8)
- [ ] No keys or connection strings in the diff (`git diff --cached`)
- [ ] Commit message says what changed, from your own GitHub account

### Testing the mobile app without an emulator
Use a real Android phone: Settings → About phone → tap *Build number* 7 times → Developer options → **USB debugging** (or *Wireless debugging*).
Plug it in, pick it in Android Studio and press Run. Set `API_BASE_URL` to your PC's Wi-Fi IP (Section 5, step 10).

### Google Maps key
Shared privately by M3. It goes **only** in `mobile/local.properties` as `MAPS_API_KEY=...` (gitignored). Never paste it into code, docs or commits.
