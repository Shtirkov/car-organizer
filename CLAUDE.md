# CLAUDE.md

Working notes for the **Car Maintenance Tracker** — a web app for tracking vehicle
maintenance history, documents and reminders. Full product spec:
[`Car_Maintenance_Tracker_PRD_v1.docx`](./Car_Maintenance_Tracker_PRD_v1.docx).

> This file is the quick reference. Update it whenever conventions, structure, or the
> roadmap change so future sessions don't have to re-scan the whole project.

## Tech stack

| Layer        | Technology                                                    |
| ------------ | ------------------------------------------------------------- |
| Backend      | ASP.NET Core **10** Web API, clean architecture               |
| Database     | PostgreSQL + EF Core 10 (`Npgsql.EntityFrameworkCore.PostgreSQL`) |
| Auth         | Own JWT via ASP.NET **Identity** (access + refresh tokens)    |
| File storage | Cloudflare R2 (S3-compatible), planned                        |
| Frontend     | React (Vite + TypeScript)                                     |
| Deployment   | Docker + GitHub Actions → Railway                             |
| Tests        | xUnit, Moq, `Microsoft.AspNetCore.Mvc.Testing`, EF InMemory   |

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
- DTOs are `record`s in a feature folder (e.g. `Application/Auth/`).
- Public types get concise `<summary>` XML doc comments (match existing style).
- Register Infrastructure services in `Infrastructure/DependencyInjection.cs` (`AddInfrastructure`).

## Auth (Phase 2 — in progress)

- `User : IdentityUser<Guid>` ([Domain/Entities/User.cs](backend/CarOrganizer.Domain/Entities/User.cs)).
  Generic `IdentityUser<TKey>` does **not** auto-assign `Id`, so the ctor sets `Id = Guid.NewGuid()`.
- `AppDbContext : IdentityUserContext<User, Guid>` — **user tables only, no role tables**
  (`AspNetUsers/UserClaims/UserLogins/UserTokens`). Chosen over `IdentityDbContext` because MVP has no roles.
- DI: `AddIdentityCore<User>(...)` (not `AddIdentity` — no cookie auth for a JWT API) +
  `AddEntityFrameworkStores<AppDbContext>()`. Options: `RequireUniqueEmail = true`,
  password min length 8, `RequireNonAlphanumeric = false`.
- Register flow: `AuthController` → `IAuthService.RegisterAsync` → `UserManager.CreateAsync`
  (hashes password; returns `IdentityResult` mapped to `Result`).
- **Done:** register endpoint. **Next:** login + JWT access token → refresh tokens → auth middleware → protected endpoints → logout/revoke.

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

## Roadmap (phase by phase)

0 setup ✅ · 1 domain + DB ✅ · **2 JWT auth (register ✅, login/refresh next)** ·
3 vehicles/garage · 4 maintenance records · 5 documents · 6 dashboard + reminders ·
7 React frontend · 8 deploy to Railway · 9 feedback & iteration
