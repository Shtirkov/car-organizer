# CLAUDE.md

Working notes for the **Car Maintenance Tracker** — a web app for tracking vehicle
maintenance history, documents and reminders. Full product spec:
[`Car_Maintenance_Tracker_PRD_v1.docx`](./Car_Maintenance_Tracker_PRD_v1.docx).

> This file is the quick reference. Update it whenever conventions, structure, or the
> roadmap change so future sessions don't have to re-scan the whole project.

## What we're building (PRD digest)

Car owners lose invoices and forget service intervals, insurance renewals and inspections —
records live scattered across paper, email and memory. The app centralizes them.

**MVP features:** auth · garage of one or more vehicles · vehicle details (VIN, mileage, engine,
registration, year) · maintenance records (type, date, mileage, cost, notes) · document upload
(images/PDF) · dashboard with upcoming reminders · basic search and filtering.

**Explicit non-goals for MVP:** no mechanic marketplace, no parts store, no AI recommendations,
no fleet management. Fuel tracking, expense analytics and PDF export are post-MVP (the `FuelEntry`
and `Expense` entities in the PRD's domain model are deliberately **not** built yet).

> ⚠️ The PRD's milestone numbers are offset from our roadmap below: PRD "Phase 3 Authentication"
> = our Phase 2, PRD "Phase 4 Vehicle management" = our Phase 3, and so on. When a phase number
> is mentioned, it means **our** roadmap unless the PRD is named explicitly.

## Tech stack

| Layer        | Technology                                                    |
| ------------ | ------------------------------------------------------------- |
| Backend      | ASP.NET Core **10** Web API, clean architecture               |
| Database     | PostgreSQL + EF Core 10 (`Npgsql.EntityFrameworkCore.PostgreSQL`) |
| Auth         | Own JWT via ASP.NET **Identity** (access + refresh tokens)    |
| File storage | Local disk behind `IFileStorage`; Cloudflare R2 (S3-compatible) at deploy (Phase 8) |
| Frontend     | Expo / React Native (TypeScript) — **not started**; web later via RN Web (Phase 7) |
| Deployment   | Docker + GitHub Actions → Railway                             |
| Tests        | xUnit, Moq, `Microsoft.AspNetCore.Mvc.Testing`, EF InMemory   |
| API docs     | Swagger UI via `Swashbuckle.AspNetCore` (Dev only, `/swagger`) |

SDK: .NET 10 (`dotnet --version` → 10.0.x). `dotnet-ef` tool installed globally.

## Solution layout (`backend/CarOrganizer.slnx`)

```
backend/
├── CarOrganizer.Domain/          # Entities, enums, BaseEntity. No external deps except Identity.Stores (for User).
├── CarOrganizer.Application/     # Use-case contracts + DTOs. Depends only on Domain.
│   ├── Auth/                     # DTOs (RegisterRequest, ...)
│   ├── Common/                   # Result type
│   └── Interfaces/               # ALL service interfaces live here (e.g. IAuthService)
├── CarOrganizer.Infrastructure/  # EF Core, Identity, external services. Implements Application interfaces.
│   ├── Identity/                 # AuthService (UserManager-backed)
│   └── Persistence/              # AppDbContext, entity Configurations, Migrations
├── CarOrganizer.API/             # Controllers, Program.cs (composition root)
└── tests/
    ├── CarOrganizer.UnitTests/        # Fast, isolated, mocked deps
    └── CarOrganizer.IntegrationTests/ # Full HTTP pipeline over in-memory DB
```

**Layer dependency rule:** Domain ← Application ← Infrastructure ← API. Application defines
*what* (interfaces), Infrastructure defines *how*. Never let Application depend on
ASP.NET Identity / EF types — surface results through `Application/Common/Result.cs` instead.

### Conventions
- **Interfaces go in `Application/Interfaces/`** (namespace `CarOrganizer.Application.Interfaces`), not next to their DTOs.
- DTOs are `record`s in a feature folder (e.g. `Application/Auth/`, `Application/Vehicles/`).
  Separate records per operation even when the shape is identical (`Login`/`Register`, `Create`/`Update`).
- Public types get concise `<summary>` XML doc comments (match existing style).
- Register Infrastructure services in `Infrastructure/DependencyInjection.cs` (`AddInfrastructure`).
- **Service implementations live in Infrastructure**, one folder per feature (`Identity/`, `Vehicles/`),
  even when — like `VehicleService` — they have no infrastructure dependency of their own. Application
  stays contracts-only, so there is no `AddApplication` to maintain.
- **Feature shape** (established by vehicles, follow it for records/documents/reminders):
  `I<Feature>Store` (EF-backed, in `Infrastructure/Persistence/`) ← `I<Feature>Service`
  (mapping + rules, in `Infrastructure/<Feature>/`) ← controller (HTTP only).
- **`Result` vs plain values:** `Result` exists to carry *real* failures (Identity's error lists).
  Plain CRUD has none — model validation catches shape errors before the service runs, and
  "no such row" is said by `null`/`false`. Don't wrap those in an always-empty `Result`.

## Auth (Phase 2 ✅)

- `User : IdentityUser<Guid>` ([Domain/Entities/User.cs](backend/CarOrganizer.Domain/Entities/User.cs)).
  Generic `IdentityUser<TKey>` does **not** auto-assign `Id`, so the ctor sets `Id = Guid.NewGuid()`.
- `AppDbContext : IdentityUserContext<User, Guid>` — **user tables only, no role tables**
  (`AspNetUsers/UserClaims/UserLogins/UserTokens`). Chosen over `IdentityDbContext` because MVP has no roles.
- DI: `AddIdentityCore<User>(...)` (not `AddIdentity` — no cookie auth for a JWT API) +
  `AddEntityFrameworkStores<AppDbContext>()`. Options: `RequireUniqueEmail = true`,
  password min length 8, `RequireNonAlphanumeric = false`.
- Register flow: `AuthController` → `IAuthService.RegisterAsync` → `UserManager.CreateAsync`
  (hashes password; returns `IdentityResult` mapped to `Result`).
- Login flow: `AuthController.Login` → `IAuthService.LoginAsync` → `FindByEmailAsync` +
  `CheckPasswordAsync` → on success `IJwtTokenGenerator.GenerateAccessToken`. Bad credentials →
  **401** with a single generic message (never reveal whether the email exists).
- **JWT access token** ([Infrastructure/Identity/JwtTokenGenerator.cs](backend/CarOrganizer.Infrastructure/Identity/JwtTokenGenerator.cs)):
  HS256, claims `sub`=user id, `email`, `jti`, plus `iss`/`aud`/`nbf`/`exp`. Lifetime 15 min.
- **JWT config:** structural settings (`Issuer`/`Audience`/`AccessTokenMinutes`/`RefreshTokenDays`) live
  in committed `appsettings.json`; the secret `Jwt:Key` lives in gitignored `appsettings.Development.json`
  (or user-secrets / env in prod). `AddInfrastructure` throws if the key is missing or < 32 bytes.
  Bound to `JwtSettings` via `Configure<JwtSettings>`.
- **Token validation:** `AddJwtAuthentication` ([Infrastructure/Authentication/](backend/CarOrganizer.Infrastructure/Authentication/AuthenticationServiceCollectionExtensions.cs))
  configures `AddJwtBearer` (`TokenValidationParameters`: issuer/audience/signing key/lifetime,
  `ClockSkew = 0`, `MapInboundClaims = false` so claims stay `sub`/`email`). The pipeline step
  (`UseAuthentication`/`UseAuthorization` + Swagger etc.) is grouped in `app.UseApiMiddleware()`
  ([API/Middleware/](backend/CarOrganizer.API/Middleware/MiddlewareExtensions.cs)) — Program.cs calls it in one line.
  JWT validation is **not** a hand-written middleware — it's the framework's `JwtBearerHandler`.
- **Refresh tokens:** opaque random (`RandomNumberGenerator`, hex), stored **hashed** (SHA-256) as
  [RefreshToken](backend/CarOrganizer.Domain/Entities/RefreshToken.cs) rows via `IRefreshTokenStore`.
  Login issues an access+refresh pair; `POST /api/auth/refresh` validates the hash, checks `IsActive`
  (not revoked/expired), then **rotates** (revoke old, issue new). Reuse of a rotated token → 401.
- **Logout:** `POST /api/auth/logout { refreshToken }` revokes that refresh token. **No `[Authorize]`**
  (the refresh token is the credential; an expired access token must not block logout). Idempotent →
  always **204**, even for unknown/already-revoked tokens (no token-probing).
- Endpoints: `POST register|login|refresh|logout`, `GET me` (`[Authorize]`, reads `sub`/`email`).
- **Phase 2 complete:** register, login, JWT access token, bearer validation + `[Authorize]`,
  refresh + rotation, logout/revoke. All flows covered by unit + integration tests.

## Vehicles / garage (Phase 3 ✅)

- Endpoints ([API/Controllers/VehiclesController.cs](backend/CarOrganizer.API/Controllers/VehiclesController.cs)),
  all `[Authorize]` at the controller level:
  `GET /api/vehicles` · `GET /api/vehicles/{id:guid}` · `POST /api/vehicles` (201 + `Location`) ·
  `PUT /api/vehicles/{id:guid}` · `DELETE /api/vehicles/{id:guid}` (204).
- **The owner always comes from the token's `sub` claim, never from the body.** `User.GetUserId()`
  ([API/Extensions/ClaimsPrincipalExtensions.cs](backend/CarOrganizer.API/Extensions/ClaimsPrincipalExtensions.cs))
  parses it; it throws on a missing/non-Guid `sub`, which is an assertion about our own token
  generator (a forged token never reaches the action), not input validation.
- **Someone else's vehicle → 404, never 403.** A 403 would confirm the id exists. `IVehicleStore`'s
  lookups take `(vehicleId, ownerId)` so ownership is part of the question and can't be forgotten
  by a caller. Covered by integration tests, including that the two 404 bodies are byte-identical
  (modulo the per-request `traceId`).
- `VehicleResponse` deliberately omits `OwnerId` — the caller is always the owner.
- `PUT` is a full replacement: every editable field is written, so omitting an optional one clears
  it. `OwnerId` is not editable — a vehicle can't change hands.
- Validation bounds are consts in [Application/Vehicles/VehicleLimits.cs](backend/CarOrganizer.Application/Vehicles/VehicleLimits.cs)
  (attributes need compile-time constants, and Create/Update must not drift apart).
- **`Vehicle.OwnerId` now has a real FK** to `AspNetUsers` (cascade delete), added in the
  `AddVehicleOwnerForeignKey` migration. Configured with `HasOne<User>().WithMany()` — no navigation
  property, so `Vehicle` stays free of the Identity type.
- **Mileage is two fields** (since Phase 4): `PurchaseMileage` (odometer at acquisition, fixed) and
  `CurrentMileage` (advanced by maintenance records). Create takes `PurchaseMileage` (required) +
  optional `CurrentMileage` (defaults to purchase); the `CurrentMileage >= PurchaseMileage` invariant
  is a cross-field rule in `VehicleMileage.ValidateOrder`, wired via `IValidatableObject` on both
  request records → **400** (keeps `VehicleService` failure-free). `SplitVehicleMileage` migration
  renamed the old `Mileage` column and seeded `CurrentMileage` from it.

## Maintenance records (Phase 4 ✅)

- Nested under the vehicle: `GET|POST /api/vehicles/{vehicleId:guid}/maintenance-records`,
  `GET|PUT|DELETE .../{id:guid}` ([API/Controllers/MaintenanceRecordsController.cs](backend/CarOrganizer.API/Controllers/MaintenanceRecordsController.cs)).
  Owner from the token, vehicle from the route. The `MaintenanceRecord` entity + table already existed
  from Phase 1, so **no migration** — only the store/service/controller/DTO layer was added.
- **Feature shape unchanged from vehicles:** `IMaintenanceRecordStore` (EF, lookups scoped by
  `vehicleId`) ← `IMaintenanceRecordService` ← controller. Plain values, **no `Result`** — with
  auto-advance there's no domain failure to report; `null`/`false` = "not your vehicle or no such record".
- **Mileage auto-advance:** `MaintenanceRecordService` depends on `IVehicleStore` **and** the record
  store. On create/update it loads the owner's vehicle (this is also the ownership gate), and if the
  record's mileage exceeds `CurrentMileage`, bumps it. **One `SaveChanges` commits both** the record
  and the bump — the vehicle is tracked by the same scoped `AppDbContext` the record store saves
  through, so the service mutates it in memory and does **not** call `_vehicleStore.UpdateAsync`
  (which would be a second save). Delete does **not** pull `CurrentMileage` back (high-water mark).
- Cross-user isolation and **404-not-403** as in vehicles; a non-owned vehicle 404s the whole
  collection. `MaintenanceRecordResponse` omits `VehicleId` (it's in the URL). List order: `Date` desc.

## Vehicle obligations (Phase 4 ✅)

- The administrative/legal side of ownership — insurance, casco, technical inspection, vignette, tax —
  modelled as **one entity** [`VehicleObligation`](backend/CarOrganizer.Domain/Entities/VehicleObligation.cs)
  with an `ObligationType` enum, **not** crammed into `MaintenanceType`. They differ from maintenance
  by having a validity period (`ValidFrom?`/`ValidUntil`), a cost, a provider and a policy number.
- Nested routes `/api/vehicles/{vehicleId:guid}/obligations`, same owner-scoped/404 model as records.
- `ValidUntil` is required and indexed — the dashboard (Phase 6) derives "expiring soon" straight
  from it, so these need **no separate `Reminder` rows**. `ValidFrom <= ValidUntil` is a cross-field
  rule (`ObligationValidity.ValidateOrder`) → **400**. List order: `ValidUntil` asc (soonest first).
- No side effects, so `VehicleObligationService` only needs `IVehicleStore` for the ownership gate
  (`OwnsVehicleAsync`), not to mutate it. Table added in the `AddVehicleObligations` migration.
- Attaching the policy/certificate PDF landed in Phase 5 — see below.

## Documents (Phase 5 ✅)

- Nested under the vehicle, `[Authorize]`, owner from the token:
  `POST|GET /api/vehicles/{vehicleId:guid}/documents` · `GET|DELETE .../{id:guid}` ·
  `GET .../{id:guid}/content` (the bytes)
  ([API/Controllers/DocumentsController.cs](backend/CarOrganizer.API/Controllers/DocumentsController.cs)).
  **Documents are immutable — there is no PUT**; replacing a file is another upload. List order:
  `CreatedAtUtc` desc. Same feature shape and 404-not-403 model as records/obligations.
- **`IFileStorage`** ([Application/Interfaces/IFileStorage.cs](backend/CarOrganizer.Application/Interfaces/IFileStorage.cs))
  is the seam: `SaveAsync(Stream) → storageKey`, `OpenReadAsync(key) → Stream?`, idempotent `DeleteAsync`.
  **The storage owns its key scheme** — `SaveAsync` hands back the key it chose rather than accepting
  one, so no caller string ever reaches a file path. `LocalFileStorage` (a directory, key =
  `Guid.NewGuid("N")`) runs now; R2 slots in behind the same interface in Phase 8. Transfer is
  **proxied through the API**, not presigned. Root from `Storage:LocalRoot`; `App_Data/` is gitignored.
- **`IFormFile` never enters Application.** The controller unpacks it into
  `UploadDocumentRequest(Stream, FileName, ContentType, SizeBytes, MaintenanceRecordId, ObligationId)`,
  so the layer rule holds, the Application csproj needs no `FrameworkReference`, and service tests use
  a plain `MemoryStream`.
- **Multipart is not a validated record.** `[Required]`-style attributes are for JSON bodies; upload
  shape errors are checked in the controller → **400**: file present and non-empty, content type in
  `DocumentLimits.AllowedContentTypes`, size ≤ 15 MB, and **exactly one** link id. `[RequestSizeLimit]`
  is set to 2× the file cap as a pure server backstop (it yields **413**, not 400) — the explicit
  `file.Length` check owns the user-facing message. Content types are normalised (`image/jpeg; x=y`
  → `image/jpeg`) before matching.
- **HEIC is deliberately rejected** even though iPhones shoot it: browsers can't render it, so it
  would break the later web client. Expo's `ImagePicker` emits JPEG by default; the 400 names the
  accepted types.
- **A document must link to exactly one** maintenance record **or** obligation — never both, and
  **never neither**. A file whose purpose nobody can name is worse than no file, so "belongs to this
  vehicle" alone is not enough. Enforced in two places on purpose: the controller rejects
  both-or-neither with a **400** that names the missing field (it's a malformed request, not a missing
  resource), and `LinkTargetExistsAsync` also refuses an unlinked request so no non-HTTP caller can
  slip one past. The named target must be on *this* vehicle, checked **before any bytes are written**
  (a bad link → 404, no orphaned blob). `VehicleObligationId` (nullable, indexed) came in the
  `AddDocumentObligationLink` migration, mirroring the record link.
- **The link FKs are `Cascade`, not `SetNull`** (`CascadeDocumentsWithTheirLink` migration). `SetNull`
  would null the link and leave paperwork of unknown purpose — the exact state the rule forbids — so
  deleting a maintenance record or an obligation **deletes its documents with it**. The invariant
  therefore holds at rest, not only at creation.
- **The cascade only removes rows, so the three delete paths sweep the files themselves.**
  `MaintenanceRecordService`, `VehicleObligationService` and `VehicleService` each take
  `IDocumentStore` + `IFileStorage`, read the storage keys **before** the delete (afterwards the rows
  are gone), remove the parent, then delete the blobs. `IDocumentStore` grew
  `ListByMaintenanceRecordAsync`/`ListByObligationAsync` for exactly this. Loading the documents also
  makes them *tracked*, so EF cascades them in the change tracker — which is what makes the behaviour
  identical on InMemory (no real FKs) and on Postgres.
- **Blob/row ordering is deliberate in both directions:** upload writes bytes → row, with a
  compensating `DeleteAsync` on `CancellationToken.None` if the insert throws (a cancelled request is
  exactly when cleanup must still run). Delete removes row → bytes: a leftover blob is invisible and
  reclaimable, whereas the reverse leaves a document that lists fine but 404s on download.
- File names are sanitised (leaf after `/` and `\`, fallback `"document"`, truncated to 255) — they
  are metadata and a Content-Disposition value only, never a path.
- **Residual risk:** the row delete and the blob delete are not one transaction. If the process dies
  between them the file is orphaned — invisible and harmless, but it still occupies space. A periodic
  sweep (storage keys with no matching row) is the fix if it ever matters.

## Client direction (decided July 2026)

**Mobile-first: Expo / React Native (TypeScript)**, with the web version later via React Native Web.
The React 19 + Vite scaffold is **superseded** — don't build on it. Deployment stays **Railway**
(Docker + GitHub Actions); Hetzner VPS + Coolify is the documented fallback if the bill creeps.

Going mobile-first is mostly free on the backend, but not entirely. Consequences already banked or
still owed:

- **Phase 5 (done):** the HEIC decision and the 15 MB cap both come from phone cameras.
- **Phase 6 — push notifications.** Renewal reminders are the product's core value; on mobile that
  means FCM/APNs, i.e. a device-token table and a background sender. Design Phase 6 for push, not
  just an in-app list. Biggest single consequence of the pivot.
- **Phase 7 — session length.** 15-min access + 7-day refresh forces a monthly re-login on a phone.
  Raise `Jwt:RefreshTokenDays` to ~60 when the app work starts; rotation already handles it.
- **Phase 7 — API compatibility.** Store apps can't be force-updated, so old clients keep calling for
  years: version the API before the first release, and treat the numeric enum wire format as
  **append-only** (reordering or removing a value silently breaks installed apps).
- **CORS** isn't needed for native; add it when the RN Web build arrives.
- **Store costs, for planning:** Apple $99/yr, Google Play $25 one-time, plus Google's
  12-tester / 14-day closed-test requirement for new personal accounts.

## Enums over the wire

- Enum-typed request/response fields (`MaintenanceType`, `ObligationType`) serialize as **numbers**
  (System.Text.Json default — no `JsonStringEnumConverter` is registered). DTOs guard against
  undefined values with `[EnumDataType(typeof(...))]`. Decided deliberately; revisit if a string
  format is ever wanted (safe to add — no existing endpoint exposed an enum).

## Common commands

```bash
# Local Postgres (compose file at repo root)
docker compose up -d
docker compose exec postgres psql -U carorg -d car_organizer -c '\dt'   # inspect tables

# Build / test (run from backend/)
dotnet build
dotnet test                                   # all tests
dotnet test tests/CarOrganizer.UnitTests      # one project

# Run the API (http profile → http://localhost:5066 ; https → 7150)
# Swagger UI (Development only): http://localhost:5066/swagger — opens automatically (launchBrowser).
dotnet run --project CarOrganizer.API --launch-profile http

# EF Core migrations (run from backend/)
dotnet ef migrations add <Name> --project CarOrganizer.Infrastructure --startup-project CarOrganizer.API
dotnet ef database update      --project CarOrganizer.Infrastructure --startup-project CarOrganizer.API
```

Local Postgres connection: `Host=localhost;Port=5432;Database=car_organizer;Username=carorg;Password=carorg_dev_pw`

## Testing conventions

Every piece of code we add gets thorough tests (prefer over-testing). Two projects:

- **UnitTests** — no I/O. Mock collaborators with **Moq**.
  - Mock `UserManager<User>` via `new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null!, ... ×8)`.
  - Controllers tested by asserting the returned `IActionResult` type.
- **IntegrationTests** — real HTTP through `WebApplicationFactory<Program>`.
  - `CustomWebApplicationFactory` swaps Npgsql for **EF InMemory** and creates a fresh DB per factory.
  - A new factory is built **per test** (`IDisposable`) for isolation.
  - Assert HTTP status codes; read persisted state via a service scope (`factory.Services.CreateScope()`).
  - Two ways to authenticate a client, and the choice matters: `TestJwt.Create(sub: ...)` forges a
    token for a user that doesn't exist (fine for testing the *validation middleware*), while
    `VehicleEndpointsTests.SignUpAsync` registers + logs in for real. **Anything that writes a row
    referencing a user must sign up for real** — see the InMemory/FK gotcha below.
  - For cross-user rules, drive two clients off one factory (they share the database).
  - Nested-resource suites (`MaintenanceRecordEndpointsTests`, `VehicleObligationEndpointsTests`)
    sign up, create a vehicle, then drive its child collection; enum `type` is sent as its **number**.
  - **Uploads run against the real `LocalFileStorage`**, not a mock: the factory points
    `Storage:LocalRoot` at a per-factory temp directory and deletes it in `Dispose(bool)`. So
    `DocumentEndpointsTests` can assert a genuine round trip — the downloaded bytes must equal the
    uploaded ones. Multipart is built with `MultipartFormDataContent` + `ByteArrayContent`.

## Gotchas learned (don't rediscover these)

- **`record` + validation attributes:** put `[Required]`/`[EmailAddress]` directly on the
  positional parameter, **not** `[property: ...]`. Targeting the property throws
  `InvalidOperationException` (500) during model validation for record types.
- **Test DB provider swap:** `AddDbContext` registers the provider through
  `IDbContextOptionsConfiguration<AppDbContext>`. To replace Npgsql with InMemory you must
  remove that descriptor too (match by `ServiceType.Name.StartsWith("IDbContextOptionsConfiguration")`),
  otherwise EF throws "Only a single database provider can be registered".
- **`Program` for tests:** on **.NET 10** the generated top-level `Program` class is emitted
  as `public`, so `WebApplicationFactory<Program>` works with **no** extra code (verified via
  reflection: `Program.IsPublic == true`). On .NET 6–9 it was `internal`, requiring a
  `public partial class Program;` line — not needed here, so we don't keep it.
- **Connection-string guard:** `AddInfrastructure` throws if `ConnectionStrings:Default` is
  missing; the test factory sets a dummy value before replacing the DbContext.
- `IdentityUser<Guid>` (id in `Microsoft.Extensions.Identity.Stores`, not `.Core`).
- **EF InMemory does not enforce foreign keys.** A test can happily write a `Vehicle` whose
  `OwnerId` points at nobody, and it would only blow up on real Postgres. So integration tests that
  persist owned rows register a real user instead of forging a token with a random `sub`. Keep this
  in mind for every future FK — the test suite will *not* catch a violation for you.
- **A migration that exists is not a migration that ran.** `dotnet ef migrations add` only writes the
  file; without `dotnet ef database update` the local Postgres keeps the old schema and every query
  touching the new column fails with `42703 column ... does not exist` → **500**. The test suite will
  **not** warn you: EF InMemory builds its schema from the model, so integration tests stay green
  against a database that is actually behind. This bit us on `AddDocumentObligationLink`. After adding
  a migration, run `database update` and (cheaply) `\d "<Table>"` to confirm.
- **ProblemDetails bodies carry a fresh `traceId` per request**, so two responses that should be
  indistinguishable aren't byte-equal. Strip `traceId` before comparing (see
  `VehicleEndpointsTests.BodyWithoutTraceIdAsync`), rather than weakening the assertion to the
  status code alone.
- **Cross-aggregate write in one `SaveChanges`:** a service that mutates two aggregates (e.g.
  `MaintenanceRecordService` inserting a record *and* bumping the vehicle's `CurrentMileage`) relies on
  both stores sharing the request-scoped `AppDbContext`. It loads the vehicle (tracked), mutates it in
  memory, and lets the record store's single `SaveChanges` flush **both** — atomic, no second save.
  This works because store lookups don't use `AsNoTracking`. Unit tests (mocked stores) can't exercise
  the atomicity — assert the mutation on the returned vehicle object; leave persistence to integration.

## Roadmap (phase by phase)

0 setup ✅ · 1 domain + DB ✅ · 2 JWT auth ✅ (register, login, validation, refresh+rotation, logout) ·
3 vehicles/garage ✅ (CRUD, owner-scoped) · 4 maintenance records ✅ (CRUD, mileage auto-advance) +
vehicle obligations ✅ (insurance/casco/inspection/vignette/tax) · 5 documents ✅ (upload/download/
delete, local disk behind `IFileStorage`, optional record/obligation link) ·
**6 dashboard + reminders + push (next)** · 7 Expo / React Native app · 8 deploy to Railway (+ R2) ·
9 feedback & iteration

Deferred, worth picking up when the phase that needs it arrives:
- **Search/filtering** (an MVP feature in the PRD): the garage list is unfiltered and unpaged, and
  so are the record/obligation/document lists. Natural home is Phase 6.
- **Mileage coherence** — ✅ resolved in Phase 4: `PurchaseMileage`/`CurrentMileage` split + a
  maintenance record auto-advances `CurrentMileage` (high-water mark). No lower-bound or date-vs-mileage
  monotonicity rule yet (a backdated record may sit below an earlier one) — add if it ever bites.
- **Orphaned-blob sweeper**: deletes remove the row then the file in two steps, so a crash between
  them strands a file. A background job matching storage keys against rows would close it.
- **Presigned uploads** (client → R2 directly, skipping the API): same `IFileStorage` seam, revisit
  in Phase 8 if egress or CPU ever matters.
