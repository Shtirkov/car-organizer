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
| File storage | Cloudflare R2 (S3-compatible), planned                        |
| Frontend     | React 19 (Vite + TypeScript) — **scaffolded only**, still the stock starter (Phase 7) |
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
- **ProblemDetails bodies carry a fresh `traceId` per request**, so two responses that should be
  indistinguishable aren't byte-equal. Strip `traceId` before comparing (see
  `VehicleEndpointsTests.BodyWithoutTraceIdAsync`), rather than weakening the assertion to the
  status code alone.

## Roadmap (phase by phase)

0 setup ✅ · 1 domain + DB ✅ · 2 JWT auth ✅ (register, login, validation, refresh+rotation, logout) ·
3 vehicles/garage ✅ (CRUD, owner-scoped) · **4 maintenance records (next)** · 5 documents ·
6 dashboard + reminders · 7 React frontend · 8 deploy to Railway · 9 feedback & iteration

Deferred, worth picking up when the phase that needs it arrives:
- **Search/filtering** (an MVP feature in the PRD): the garage list is unfiltered and unpaged.
  Natural home is Phase 6, once maintenance records give it something to search across.
- **Mileage coherence:** `Vehicle.Mileage` and `MaintenanceRecord.Mileage` can currently disagree
  (a service at 200k km on a car recorded at 190k). Decide the rule in Phase 4.
