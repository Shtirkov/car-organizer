# CLAUDE.md

Working notes for the **Car Maintenance Tracker** — an app for tracking vehicle maintenance history,
documents and renewal dates. Full product spec:
[`Car_Maintenance_Tracker_PRD_v1.docx`](./Car_Maintenance_Tracker_PRD_v1.docx).

## This file is the source of truth

**When any other document disagrees with this one, this one wins and the other gets fixed.** Other
documents are where answers get *worked out*; this is where they *live*. If a session settles
something — in a design catalogue, an issue thread, a review — bring the answer up here, then edit
the other document to match.

Everything else ranks below it:

| Source | Authoritative for | Standing |
| ------ | ----------------- | -------- |
| **This file** | conventions, decisions, phase status, the *why* behind the shape | wins every conflict |
| [`CONTEXT.md`](./CONTEXT.md) · [`docs/adr/`](./docs/adr/) | domain vocabulary · recorded decisions | delegated by this file, not rivals to it |
| the GitHub issues | per-story detail: what to build, acceptance criteria | subordinate — promote answers up here |
| the code and its XML docs | how something works *today* | this file records why, not what |
| [the PRD](./Car_Maintenance_Tracker_PRD_v1.docx) | the original product intent | superseded wherever this file says so |

Keep it current: edit a fact in the one place that owns it — a fact stated twice will drift. Record
*why*, not what the code already says.

## What we're building

Car owners lose invoices and forget service intervals, insurance renewals and inspections — records
live scattered across paper, email and memory. The app centralizes them.

**MVP:** auth · garage of one or more vehicles · vehicle details (VIN, mileage, engine, registration,
year) · maintenance records (type, date, mileage, cost, notes) · document upload (images/PDF) ·
dashboard of what's overdue and expiring · basic search and filtering.

**Domain vocabulary is [`CONTEXT.md`](./CONTEXT.md); decisions are [`docs/adr/`](./docs/adr/).** Use
the glossary's words in issue titles, DTO names and tests — the PRD says "dashboard with upcoming
reminders", but *reminder* now means the stored thing only.

**Not in MVP:** mechanic marketplace, parts store, AI recommendations, fleet management. Fuel
tracking, expense analytics and PDF export are post-MVP — the PRD's `FuelEntry` and `Expense`
entities are deliberately unbuilt.

**Phase numbers mean *our* roadmap below, never the PRD's.** The PRD's are offset by one
(PRD "Phase 3 Authentication" = our Phase 2). Say "PRD Phase N" explicitly when you mean theirs.

## Where we are

| Phase  | Scope                                                                       | State |
| ------ | --------------------------------------------------------------------------- | ----- |
| 0      | Project setup                                                               | ✅    |
| 1      | Domain entities + database                                                  | ✅    |
| 2      | JWT auth — register, login, validation, refresh + rotation, logout          | ✅    |
| 3      | Vehicles / garage — owner-scoped CRUD                                       | ✅    |
| 4      | Maintenance records (CRUD + mileage auto-advance) · vehicle obligations     | ✅    |
| 5      | Documents — upload/download/delete, mandatory link, cascade + file sweep    | ✅    |
| 6a     | Dashboard — `GET /api/dashboard`, grouped by vehicle                        | ✅    |
| **6b** | **Reminders + push — next up** (gaps 01–02)                                 | ⬜    |
| 7      | Expo / React Native app (UI designed) + the backend gaps it needs (04–12)   | ⬜    |
| —      | **Subscriptions + entitlements (gap 03) — P0, ships *with* Phase 7**        | ⬜    |
| 8      | Deploy to Railway + swap `IFileStorage` to R2 (gap 13)                      | ⬜    |
| 9      | Feedback & iteration                                                        | ⬜    |

### The remaining work is 13 features on GitHub Issues

Each feature is one issue, its stories are **sub-issues**, and every `Depends on` is a native
**blocking link** — so "what can I start now?" is a query, not a read-through. The issues are the
only copy: the `docs/backend-gaps/` catalogue they were generated from is gitignored precisely so
it can't drift out of sync. **This file stays authoritative** on phase and decisions.

| Gap | Feature | Issue | Gap | Feature | Issue |
| --- | ------- | ----- | --- | ------- | ----- |
| 01 | Reminder generation and delivery | [#1](https://github.com/Shtirkov/car-organizer/issues/1) | 08 | Owner-wide documents list | [#28](https://github.com/Shtirkov/car-organizer/issues/28) |
| 02 | Push and email channels | [#6](https://github.com/Shtirkov/car-organizer/issues/6) | 09 | Search | [#31](https://github.com/Shtirkov/car-organizer/issues/31) |
| 03 | Subscriptions and entitlements | [#11](https://github.com/Shtirkov/car-organizer/issues/11) | 10 | Renewing an obligation | [#35](https://github.com/Shtirkov/car-organizer/issues/35) |
| 04 | User profile fields | [#17](https://github.com/Shtirkov/car-organizer/issues/17) | 11 | Language / locale | [#38](https://github.com/Shtirkov/car-organizer/issues/38) |
| 05 | Odometer quick update | [#20](https://github.com/Shtirkov/car-organizer/issues/20) | 12 | SSO, reset, email confirm | [#41](https://github.com/Shtirkov/car-organizer/issues/41) |
| 06 | Currency on money fields | [#22](https://github.com/Shtirkov/car-organizer/issues/22) | 13 | Thumbnails and signed URLs | [#46](https://github.com/Shtirkov/car-organizer/issues/46) |
| 07 | Cost totals | [#25](https://github.com/Shtirkov/car-organizer/issues/25) | | | |

Labels are `feature`/`story` plus `P0`/`P1`/`P2`. To find what's ready to pick up:

```bash
gh issue list --label story --state open --json number,title,body --jq '.[] | "\(.number) \(.title)"'
```

then drop any whose `issue_dependencies_summary.blocked_by` is above zero.

### Deferred, and the phase that should pick each up

These predate the gap catalogue and are *not* in it — they stay here.

- **Orphaned-blob sweeper** — a delete removes the row then the file, so a crash between the two
  strands a file: invisible and harmless, but it occupies space. A job matching storage keys against
  rows closes it. Phase 8, or whenever it bites.
- **Presigned uploads** — client straight to R2, skipping the API. Same `IFileStorage` seam.
  Phase 8, only if egress or CPU ever matters.
- **Mileage monotonicity** — a backdated maintenance record may sit below an earlier one; there is no
  lower-bound or date-vs-mileage rule. Add if it ever bites.

(**Search / filtering** used to sit here; it is now gap 09 and tracked with the rest.)

## Tech stack

| Layer        | Technology                                                                          |
| ------------ | ----------------------------------------------------------------------------------- |
| Backend      | ASP.NET Core **10** Web API, clean architecture                                     |
| Database     | PostgreSQL + EF Core 10 (`Npgsql.EntityFrameworkCore.PostgreSQL`)                    |
| Auth         | Own JWT via ASP.NET **Identity** (access + refresh tokens)                           |
| File storage | Local disk behind `IFileStorage`; Cloudflare R2 (S3-compatible) at deploy (Phase 8)  |
| Client       | Expo / React Native (TypeScript), mobile only — **not started**                      |
| Deployment   | Docker + GitHub Actions → Railway                                                    |
| Tests        | xUnit, Moq, `Microsoft.AspNetCore.Mvc.Testing`, EF InMemory                          |
| API docs     | Swagger UI via `Swashbuckle.AspNetCore` (Development only, `/swagger`)               |

SDK: .NET 10 (`dotnet --version` → 10.0.x). `dotnet-ef` installed globally.

⚠️ **`frontend/` is a dead React 19 + Vite scaffold, superseded by the Expo decision.** It still
builds and it is still in the repo, but nothing depends on it and no new work goes there. The Phase 7
client is a new Expo app.

## Solution layout (`backend/CarOrganizer.slnx`)

```
backend/
├── CarOrganizer.Domain/          # Entities, enums, BaseEntity. No deps except Identity.Stores (for User).
├── CarOrganizer.Application/     # Contracts + DTOs. Depends only on Domain.
│   ├── Interfaces/               # ALL service and store interfaces, no exceptions
│   ├── Common/                   # Result / Result<T>
│   └── Auth|Vehicles|MaintenanceRecords|Obligations|Documents|Dashboard/   # DTOs + limits
├── CarOrganizer.Infrastructure/  # EF Core, Identity, storage. Implements Application interfaces.
│   ├── Persistence/              # AppDbContext, Configurations/, Migrations/, and every *Store
│   ├── Authentication/           # AddJwtAuthentication (bearer validation wiring)
│   ├── Storage/                  # LocalFileStorage, FileStorageSettings
│   ├── DependencyInjection.cs    # AddInfrastructure — register services here
│   └── Identity|Vehicles|MaintenanceRecords|Obligations|Documents|Dashboard/   # the *Services
├── CarOrganizer.API/             # Controllers/, Extensions/, Middleware/, Program.cs
└── tests/
    ├── CarOrganizer.UnitTests/        # no I/O, mocked collaborators
    └── CarOrganizer.IntegrationTests/ # full HTTP pipeline over EF InMemory
```

**Layer rule: Domain ← Application ← Infrastructure ← API.** Application says *what* (interfaces),
Infrastructure says *how*. Application never references an ASP.NET Identity, EF or `IFormFile` type —
results cross the line as `Result`, DTOs or plain values.

**Feature folder naming is not uniform, so read it here rather than guessing:** the obligations
feature is `Obligations/` in both Application and Infrastructure, while its entity, service and
controller are `VehicleObligation*`.

### Conventions

- **Every interface lives in `Application/Interfaces/`** (namespace `CarOrganizer.Application.Interfaces`),
  including stores — not beside its DTOs, not in Infrastructure.
- **DTOs are `record`s** in a feature folder, one record per operation even when two shapes are
  identical (`Login`/`Register`, `Create`/`Update`) — they drift apart eventually.
- **Validation bounds are consts in `<Feature>Limits.cs`** next to the DTOs (`VehicleLimits`,
  `MaintenanceLimits`, `ObligationLimits`, `DocumentLimits`, `DashboardLimits`). Attributes need
  compile-time constants, and Create/Update must not drift apart.
- **Feature shape** — `I<Feature>Store` (EF, in `Infrastructure/Persistence/`) ← `I<Feature>Service`
  (mapping + rules, in `Infrastructure/<Feature>/`) ← controller (HTTP only). Every feature since
  vehicles follows it; use it for reminders too.
- **Service implementations live in Infrastructure**, one folder per feature, even for a service with
  no infrastructure dependency of its own. Application stays contracts-only, so there is no
  `AddApplication` to maintain. Register in `Infrastructure/DependencyInjection.cs`.
- Public types get a concise `<summary>`; put the *why* in `<remarks>`. The XML docs are the primary
  home for per-class rationale — this file records what spans classes.

## Cross-cutting API rules

These hold for every feature. A feature section below states only where it *departs* from them.

- **The owner comes from the token's `sub` claim, never from a route or body.** `User.GetUserId()`
  ([API/Extensions/ClaimsPrincipalExtensions.cs](backend/CarOrganizer.API/Extensions/ClaimsPrincipalExtensions.cs))
  parses it and throws on a missing/non-Guid `sub` — an assertion about our own token generator, not
  input validation, since a forged token never reaches the action.
- **Someone else's row → 404, never 403.** A 403 confirms the id exists. Store lookups take
  `(id, ownerId)` — or `(id, vehicleId)` under a vehicle — so ownership is part of the question and a
  caller cannot forget it. A vehicle that isn't yours 404s its whole child collection. Integration
  tests assert the two 404 bodies are byte-identical (modulo `traceId`).
- **`Result` carries real failures only.** It exists for Identity's error lists. Plain CRUD has no
  such failure: model validation catches shape errors before the service runs, and "no such row" is
  said by `null`/`false`. Don't wrap those in an always-empty `Result`.
- **Cross-field rules are `IValidatableObject` on the request record → 400**, so services stay
  failure-free (`VehicleMileage.ValidateOrder`, `ObligationValidity.ValidateOrder`).
- **Enums go over the wire as numbers.** System.Text.Json's default; no `JsonStringEnumConverter` is
  registered. DTOs reject undefined values with `[EnumDataType(typeof(...))]`. Treat the numeric
  values as **append-only** — reordering or removing one silently breaks installed apps. Switching to
  strings is still safe today and gets less safe after Phase 7 ships.
- **`PUT` is full replacement** everywhere it exists: every editable field is written, so omitting an
  optional one clears it.
- **Deleting a parent takes its documents with it** — DB cascade for the rows, an explicit file sweep
  for the blobs, identical in all three delete paths. The *Documents* section owns the mechanism.

### API surface

| Route                                                 | Methods                          | Notes                        |
| ----------------------------------------------------- | -------------------------------- | ---------------------------- |
| `/api/auth/register\|login\|refresh\|logout`          | POST                             | anonymous                    |
| `/api/auth/me`                                        | GET                              | `[Authorize]`                |
| `/api/vehicles`                                       | GET, POST                        | POST → 201 + `Location`      |
| `/api/vehicles/{id:guid}`                             | GET, PUT, DELETE                 | DELETE → 204                 |
| `/api/vehicles/{vehicleId:guid}/maintenance-records`  | GET, POST                        | list order `Date` desc       |
| `…/maintenance-records/{id:guid}`                     | GET, PUT, DELETE                 |                              |
| `/api/vehicles/{vehicleId:guid}/obligations`          | GET, POST                        | list order `ValidUntil` asc  |
| `…/obligations/{id:guid}`                             | GET, PUT, DELETE                 |                              |
| `/api/vehicles/{vehicleId:guid}/documents`            | GET, POST                        | POST is multipart            |
| `…/documents/{id:guid}` · `…/{id:guid}/content`       | GET, DELETE · GET                | no PUT — uploads are immutable |
| `/api/dashboard?withinDays=&recentCount=`             | GET                              | never 404s                   |

Everything except the four anonymous auth endpoints is `[Authorize]` at the controller level.

## Auth (Phase 2 ✅)

- `User : IdentityUser<Guid>` ([Domain/Entities/User.cs](backend/CarOrganizer.Domain/Entities/User.cs)).
  Generic `IdentityUser<TKey>` does **not** auto-assign `Id`, so the ctor sets `Id = Guid.NewGuid()`.
- `AppDbContext : IdentityUserContext<User, Guid>` — **user tables only, no role tables**. Chosen over
  `IdentityDbContext` because the MVP has no roles.
- DI is `AddIdentityCore<User>` (not `AddIdentity` — a JWT API wants no cookie auth) +
  `AddEntityFrameworkStores<AppDbContext>()`. `RequireUniqueEmail = true`, password min length 8,
  `RequireNonAlphanumeric = false`.
- **Access token** ([Infrastructure/Identity/JwtTokenGenerator.cs](backend/CarOrganizer.Infrastructure/Identity/JwtTokenGenerator.cs)):
  HS256, 15 min, claims `sub` = user id, `email`, `jti`, plus `iss`/`aud`/`nbf`/`exp`.
- **Config split:** structural settings (`Issuer`, `Audience`, `AccessTokenMinutes`, `RefreshTokenDays`)
  sit in committed `appsettings.json`; the secret `Jwt:Key` sits in gitignored
  `appsettings.Development.json` (user-secrets or env in prod). `AddInfrastructure` throws if the key
  is missing or under 32 bytes. Bound via `Configure<JwtSettings>`.
- **Validation is the framework's `JwtBearerHandler`, not hand-written middleware.**
  `AddJwtAuthentication` ([Infrastructure/Authentication/](backend/CarOrganizer.Infrastructure/Authentication/AuthenticationServiceCollectionExtensions.cs))
  sets `ClockSkew = 0` and `MapInboundClaims = false` so claims stay `sub`/`email`. The pipeline steps
  are grouped into `app.UseApiMiddleware()`
  ([API/Middleware/](backend/CarOrganizer.API/Middleware/MiddlewareExtensions.cs)) — one line in Program.cs.
- **Refresh tokens** are opaque random hex (`RandomNumberGenerator`), stored **SHA-256 hashed** as
  [RefreshToken](backend/CarOrganizer.Domain/Entities/RefreshToken.cs) rows. Login issues an
  access + refresh pair; `refresh` validates the hash, checks `IsActive`, then **rotates** (revoke
  old, issue new). Reusing a rotated token → 401.
- **Bad credentials → 401 with one generic message**, so login never reveals whether an email exists.
- **Logout takes no `[Authorize]`** — the refresh token *is* the credential, and an expired access
  token must not block logout. Always 204, even for an unknown or already-revoked token, so the
  endpoint can't be used to probe.

## Vehicles / garage (Phase 3 ✅)

- `VehicleResponse` omits `OwnerId` — the caller is always the owner. `OwnerId` is not editable
  either; a vehicle can't change hands.
- **Mileage is two fields.** `PurchaseMileage` (odometer at acquisition, fixed) and `CurrentMileage`
  (advanced by maintenance records). Create takes `PurchaseMileage` required plus optional
  `CurrentMileage`, defaulting to purchase. The `CurrentMileage >= PurchaseMileage` invariant is a
  cross-field rule. The `SplitVehicleMileage` migration renamed the old `Mileage` column and seeded
  `CurrentMileage` from it.
- **`Vehicle.OwnerId` has a real FK** to `AspNetUsers` with cascade delete (`AddVehicleOwnerForeignKey`
  migration), configured `HasOne<User>().WithMany()` — no navigation property, so `Vehicle` stays free
  of the Identity type.
- **Deleting a vehicle takes its whole paper trail**: records, obligations and document rows by DB
  cascade, stored files by `VehicleService` itself.

## Maintenance records (Phase 4 ✅)

- The entity and table already existed from Phase 1, so Phase 4 added **no migration** — only the
  store/service/controller/DTO layer.
- `MaintenanceRecordResponse` omits `VehicleId`; it's in the URL.
- **Mileage auto-advance is the one real rule here.** `MaintenanceRecordService` takes `IVehicleStore`
  as well as its own store. On create and update it loads the owner's vehicle — which doubles as the
  ownership gate — and bumps `CurrentMileage` if the record's mileage exceeds it. **One `SaveChanges`
  commits both**: the vehicle is tracked by the same request-scoped `AppDbContext` the record store
  saves through, so the service mutates it in memory and never calls `_vehicleStore.UpdateAsync`
  (that would be a second save). Delete does **not** pull `CurrentMileage` back — it's a high-water
  mark.

## Vehicle obligations (Phase 4 ✅)

- The administrative side of ownership — insurance, casco, technical inspection, vignette, tax —
  modelled as **one entity** [`VehicleObligation`](backend/CarOrganizer.Domain/Entities/VehicleObligation.cs)
  with an `ObligationType` enum, rather than crammed into `MaintenanceType`. Obligations have what
  maintenance lacks: a validity period (`ValidFrom?`/`ValidUntil`), a provider and a policy number.
- **`ValidUntil` is required and indexed, and that index is why obligations need no `Reminder` rows** —
  the dashboard derives "expiring soon" straight from the column. `ValidFrom <= ValidUntil` is a
  cross-field rule.
- `VehicleObligationService` uses `IVehicleStore` for the ownership gate (`OwnsVehicleAsync`) only,
  never to mutate. Table added in the `AddVehicleObligations` migration.

## Documents (Phase 5 ✅)

- **Uploads are immutable — there is no PUT.** Replacing a file is another upload. List `CreatedAtUtc` desc.
- **`IFileStorage`** ([Application/Interfaces/IFileStorage.cs](backend/CarOrganizer.Application/Interfaces/IFileStorage.cs))
  is the seam: `SaveAsync(Stream) → storageKey`, `OpenReadAsync(key) → Stream?`, idempotent `DeleteAsync`.
  **The storage owns its key scheme** — `SaveAsync` returns the key it chose rather than accepting one,
  so no caller string ever reaches a file path. `LocalFileStorage` (a directory, key =
  `Guid.NewGuid("N")`, root `Storage:LocalRoot`, `App_Data/` gitignored) runs now. Transfer is
  **proxied through the API**, not presigned.
- **`IFormFile` never enters Application.** The controller unpacks it into
  `UploadDocumentRequest(Stream, FileName, ContentType, SizeBytes, MaintenanceRecordId, ObligationId)` —
  so the layer rule holds, Application needs no `FrameworkReference`, and service tests pass a
  `MemoryStream`.
- **Multipart is not a validated record**, since `[Required]`-style attributes are for JSON bodies.
  The controller checks shape → **400**: file present and non-empty, content type in
  `DocumentLimits.AllowedContentTypes` (normalised, so `image/jpeg; x=y` matches), ≤ 15 MB, exactly one
  link id. `[RequestSizeLimit]` at 2× the cap is a server backstop yielding **413**; the explicit
  `file.Length` check owns the user-facing message.
- **A document links to exactly one maintenance record or obligation — never both, never neither.**
  A file whose purpose nobody can name is worse than no file, so "belongs to this vehicle" is not
  enough. Enforced twice deliberately: the controller returns **400** naming the missing field (a
  malformed request, not a missing resource), and `LinkTargetExistsAsync` refuses an unlinked request
  so no non-HTTP caller slips one past. The target must be on *this* vehicle, checked **before any
  bytes are written**, so a bad link 404s without orphaning a blob.
- **The link FKs are `Cascade`, not `SetNull`** (`CascadeDocumentsWithTheirLink` migration): `SetNull`
  would leave paperwork of unknown purpose, the exact state the rule forbids. The invariant therefore
  holds at rest, not only at creation.
- **The DB cascade removes rows only, so the three delete paths sweep the files themselves.**
  `MaintenanceRecordService`, `VehicleObligationService` and `VehicleService` each take `IDocumentStore`
  + `IFileStorage`, read the storage keys **before** the delete (afterwards the rows are gone), remove
  the parent, then delete the blobs — via `IDocumentStore.ListByMaintenanceRecordAsync`/`ListByObligationAsync`.
  Loading the documents also *tracks* them, so EF cascades them in the change tracker: that is what
  makes the behaviour identical on InMemory (no real FKs) and on Postgres.
- **Blob/row ordering is deliberate both ways.** Upload writes bytes → row, with a compensating
  `DeleteAsync` on `CancellationToken.None` if the insert throws (a cancelled request is exactly when
  cleanup must still run). Delete removes row → bytes: a leftover blob is invisible and reclaimable,
  while the reverse leaves a document that lists fine and 404s on download. Not one transaction —
  hence the deferred sweeper.
- File names are sanitised (leaf after `/` and `\`, fallback `"document"`, truncated to 255): metadata
  and a Content-Disposition value, never a path.
- **HEIC is rejected** though iPhones shoot it. Accepting it buys nothing — Expo's `ImagePicker` emits
  JPEG by default — while storing bytes that browsers can't render, which breaks Swagger, any direct
  download, and any browser client we might one day build. (Also in `DocumentLimits.cs`.)
- ⚠️ **`LocalFileStorage` is development-only.** A container filesystem is ephemeral: uploads vanish on
  redeploy without a mounted volume and are invisible to a second instance. **Phase 8 ships on R2**
  behind the same interface — part of that phase, not an optimisation to defer.

## Dashboard (Phase 6a ✅)

- **One endpoint, one screen.** A mobile client gets its whole home screen in one round trip.
- **It never 404s.** An owner with no vehicles gets an empty garage, so `IDashboardService.GetAsync`
  returns a non-nullable response. There is no `vehicleId` in the route — a vehicle that isn't the
  caller's is simply absent.
- **Grouped by vehicle, not flattened across the garage** (decided with the user, Aug 2026). The client
  shows one selected car at a time and switching must not need another request. Grouping is also why
  rows carry no denormalised `vehicleMake`/`vehicleModel` — the block header has them.
- **Two buckets per vehicle, and the order is the point.** `OverdueObligations` (past `ValidUntil`,
  longest-overdue first) sits above `ExpiringObligations` (due within the horizon, soonest first).
  Separate records rather than one type with a signed day count: each carries only the number that
  applies (`DaysOverdue`/`DaysRemaining`) and they render as different things. Expiring **today**
  counts as expiring (`DaysRemaining = 0`), not overdue — you can still renew it.
- **Overdue is unbounded; the horizon bounds the future only.** A renewal that lapsed two years ago is
  still a problem and still shows.
- **A read model — no new entity, store or migration.** Two lookups were added to existing stores:
  - `IVehicleObligationStore.ListByOwnerDueByAsync(ownerId, dueBy)` — that store's **only** owner-scoped
    lookup, joining through `Vehicle.OwnerId` because the dashboard spans the garage instead of sitting
    under one vehicle. Both buckets are slices of this one result.
  - `IMaintenanceRecordStore.ListRecentByVehicleAsync(vehicleId, count)` — `Take` in the database, so a
    dashboard never drags years of history across the wire. `recentCount` is **per vehicle**.
- **Query cost is 2 + N** (vehicles, obligations, then one small indexed `Take` per vehicle). Accepted
  deliberately: a personal garage is 1–3 cars, and loading every record for the owner to slice in
  memory gets worse as history grows.
- **"Today" is the server's UTC date.** For a UTC+2/+3 user that can differ from their local date for a
  couple of hours around midnight — immaterial for renewals measured in weeks. Take the client's date
  if a day-level off-by-one ever becomes visible.
- `withinDays` 1–365 (default 30) and `recentCount` 1–50 (default 5) are `[Range]` on the action
  parameters, so `[ApiController]` turns anything outside into a **400** before the service runs. The
  response echoes `WithinDays` so the client needn't assume the default.
- **Deliberately absent:** cost totals (post-MVP), service intervals (Phase 6b), search/filtering.

## Reminders and push (Phase 6b — next)

Renewal notifications are the product's core value, which is why this got its own design pass instead
of being bolted onto the dashboard.

- **`Reminder` stays — decided, build on it** (gap 01, Aug 2026). The entity and `Reminders` table have
  existed since Phase 1 wired to nothing: no store, no service, no controller, and `Vehicle.Reminders`
  is a dead navigation property. Gap 01 adds the missing layer against the existing table, so **no
  migration** unless recurrence needs a column.
- **`DueMileage` is why it survives.** `Reminder` carries `DueDate` *and* `DueMileage`; obligations
  carry only dates, so nothing in the system can answer *"when is the next oil change due?"* Mileage
  intervals are the gap, and obligations structurally cannot close it.
- **`ReminderType` is deleted; a reminder names its *subject* instead** — a discriminator plus a value
  from `MaintenanceType` or `ObligationType`
  ([ADR-0001](./docs/adr/0001-reminders-borrow-their-subject-vocabulary.md)). The old enum was those
  two renumbered, minus `Casco` and `Tax`. Do it in gap 01 story 01: `Reminders` has never held a row,
  and the numeric wire format freezes when the app ships.
- **Reminders do not replace the dashboard's obligation logic.** The dashboard keeps deriving its
  buckets straight from `VehicleObligation.ValidUntil`; reminders are an additional mechanism for what
  it can't see (mileage intervals) and for proactive delivery. **The dashboard's overdue/expiring lists
  are not "reminders"** — that word now means the stored thing only.
- The rest of the phase is the **FCM/APNs device-token table and a background sender** (gap 02, which
  depends on gap 01) — the biggest single consequence of going mobile-only.

## Client direction (decided July 2026)

**The product is a mobile app: Expo / React Native (TypeScript).** Deployment stays **Railway**;
Hetzner VPS + Coolify is the documented fallback if the bill creeps.

**A web version is a maybe, not a plan** (reaffirmed Aug 2026). React Native Web keeps the door open
at low cost, but no phase owns it, nothing is scheduled, and it may never happen. So: don't spend
backend work on a hypothetical browser client, and don't treat "the web build" as a coming event when
weighing a decision. Mobile is the only client that exists in the roadmap.

Consequences of being mobile-only that are still owed:

- **Phase 7 — session length.** 15-min access + 7-day refresh forces a monthly re-login on a phone.
  Raise `Jwt:RefreshTokenDays` to ~60 when the app work starts; rotation already handles it.
- **Phase 7 — API versioning.** Store apps can't be force-updated, so old clients keep calling for
  years. Version the API before the first release.
- **CORS** is not needed for native. Add it only if a browser client is ever actually built.
- **Store costs, for planning:** Apple $99/yr, Google Play $25 one-time, plus Google's 12-tester /
  14-day closed-test requirement for new personal accounts.

(Already banked: the HEIC rejection and the 15 MB cap both come from phone cameras.)

### App design — GarageBox (imported Aug 2026)

The Phase 7 UI is designed and frozen in **[`design/`](./design/README.md)** — screens, a design
system, and provenance; read it for the detail. The 13 backend gaps it exposed were verified against
the code and are now the GitHub issues listed under *Where we are*. What matters here:

- **A snapshot, not a dependency.** Nothing builds against it; re-import from the Claude Design
  project (id `97fe9b32-a72a-4dae-8dcd-e79c747f5c5b`) if it moves.
- **Its dashboard "Direction B" is what we built** — grouped by vehicle. The other two directions
  want the fleet-wide flattening we decided against.
- **Two gaps were re-sorted out of "open business question" (Aug 2026), and the answers are recorded
  here now:** **subscriptions (03) is P0 and launch-blocking**, with a concrete buildable default —
  only its stories 4–5 (payment platform, store consoles) still carry an open question, resolved at
  *that story's* kickoff. **Currency on `Cost` (06) is a P1 blocker** for cost totals (07), mechanical
  rather than strategic. The catalogue is where both answers were worked out; it is not where they
  live.
- **Gaps 01–02 are Phase 6b.** 04–12 are owed before the client ships; 13 lands with R2 in Phase 8.
- **Design tokens are CSS custom properties; RN has none.** Port `design/_ds/…/tokens/*.css` to a TS
  theme object once — the values were approved, the mechanism wasn't.

## Common commands

**First run on a fresh clone** — the API refuses to start without a signing key, since
`AddInfrastructure` throws when `Jwt:Key` is missing or under 32 bytes. Everything else it needs is
in the committed `appsettings.json`.

```bash
cd backend/CarOrganizer.API
cp appsettings.Development.json.example appsettings.Development.json
# then replace the placeholder Jwt:Key with 32+ random bytes:
openssl rand -base64 48
```

Agent skills are vendored, not committed — `npx skills add mattpocock/skills` restores them from
`skills-lock.json`. Their per-repo config is in [`docs/agents/`](./docs/agents/) and *is* committed.

```bash
# Local Postgres (compose file at repo root)
docker compose up -d
docker compose exec postgres psql -U carorg -d car_organizer -c '\dt'

# Build / test (from backend/)
dotnet build
dotnet test
dotnet test tests/CarOrganizer.UnitTests

# Run the API — http profile → http://localhost:5066 (https → 7150)
# Swagger (Development only) at /swagger, opens automatically
dotnet run --project CarOrganizer.API --launch-profile http

# EF Core migrations (from backend/)
dotnet ef migrations add <Name> --project CarOrganizer.Infrastructure --startup-project CarOrganizer.API
dotnet ef database update      --project CarOrganizer.Infrastructure --startup-project CarOrganizer.API
```

Local connection string: `Host=localhost;Port=5432;Database=car_organizer;Username=carorg;Password=carorg_dev_pw`

## Testing conventions

Every piece of code we add gets thorough tests — prefer over-testing.

- **UnitTests** — no I/O, collaborators mocked with **Moq**. Controllers are tested by asserting the
  returned `IActionResult` type. Mock `UserManager<User>` via
  `new Mock<UserManager<User>>(Mock.Of<IUserStore<User>>(), null!, ... ×8)`.
- **IntegrationTests** — real HTTP through `WebApplicationFactory<Program>`.
  `CustomWebApplicationFactory` swaps Npgsql for **EF InMemory** with a fresh DB per factory, and a new
  factory is built **per test** (`IDisposable`). Assert status codes; read persisted state through
  `factory.Services.CreateScope()`. For cross-user rules, drive two clients off one factory — they
  share the database.
- **Authenticating a test client, and the choice matters:** `TestJwt.Create(sub: ...)` forges a token
  for a user that doesn't exist, which is right for testing the *validation middleware* only.
  **Anything that writes a row referencing a user signs up for real** via
  `VehicleEndpointsTests.SignUpAsync` — see the InMemory/FK gotcha below.
- Nested-resource suites sign up, create a vehicle, then drive its child collection; enum `type` goes
  as its **number**.
- **Uploads run against the real `LocalFileStorage`**, not a mock: the factory points
  `Storage:LocalRoot` at a per-factory temp directory and deletes it in `Dispose(bool)`, so
  `DocumentEndpointsTests` asserts a genuine round trip — downloaded bytes must equal uploaded ones.
  Multipart is built with `MultipartFormDataContent` + `ByteArrayContent`.

## Gotchas (don't rediscover these)

- **A migration that exists is not a migration that ran.** `migrations add` only writes the file;
  without `database update` the local Postgres keeps the old schema and every query touching the new
  column fails `42703 column ... does not exist` → **500**. The suite will **not** warn you: EF
  InMemory builds its schema from the model, so integration tests stay green against a database that
  is behind. This bit us on `AddDocumentObligationLink`. After adding a migration, run `database
  update` and confirm with `\d "<Table>"`.
- **EF InMemory does not enforce foreign keys.** A test can write a `Vehicle` whose `OwnerId` points at
  nobody and only fail on real Postgres. Hence the sign-up rule above — and keep it in mind for every
  future FK, because the suite will *not* catch a violation for you.
- **`record` + validation attributes:** put `[Required]`/`[EmailAddress]` directly on the positional
  parameter. `[property: ...]` throws `InvalidOperationException` (500) during model validation for
  record types.
- **Test DB provider swap:** `AddDbContext` registers the provider through
  `IDbContextOptionsConfiguration<AppDbContext>`. Replacing Npgsql with InMemory means removing that
  descriptor too (match `ServiceType.Name.StartsWith("IDbContextOptionsConfiguration")`), else EF
  throws "Only a single database provider can be registered".
- **Connection-string guard:** `AddInfrastructure` throws when `ConnectionStrings:Default` is missing,
  so the test factory sets a dummy value before replacing the DbContext.
- **`Program` needs no `public partial` line.** On .NET 10 the generated top-level `Program` is emitted
  `public`, so `WebApplicationFactory<Program>` just works (verified: `Program.IsPublic == true`). That
  line was only needed on .NET 6–9.
- **`IdentityUser<Guid>`** lives in `Microsoft.Extensions.Identity.Stores`, not `.Core`.
- **ProblemDetails bodies carry a fresh `traceId` per request**, so two responses that should be
  indistinguishable aren't byte-equal. Strip `traceId` before comparing (see
  `VehicleEndpointsTests.BodyWithoutTraceIdAsync`) rather than weakening the assertion to the status
  code alone.
- **Cross-aggregate write in one `SaveChanges`** works because store lookups don't use `AsNoTracking`:
  the service loads the second aggregate (tracked), mutates it in memory, and lets the first store's
  single `SaveChanges` flush both. Unit tests with mocked stores can't exercise the atomicity — assert
  the mutation on the returned object and leave persistence to integration.

## Agent skills

Per-repo configuration for the skills in `.claude/skills/`. Edit these files directly; re-running
`/setup-matt-pocock-skills` is only for switching trackers or starting over.

- **Issue tracker** — GitHub Issues in `Shtirkov/car-organizer`, via the `gh` CLI. See
  [`docs/agents/issue-tracker.md`](./docs/agents/issue-tracker.md).
- **Triage labels** — the five canonical roles, label strings unchanged. Only `wontfix` exists in the
  repo so far; the other four need creating. See
  [`docs/agents/triage-labels.md`](./docs/agents/triage-labels.md).
- **Domain docs** — single-context: [`CONTEXT.md`](./CONTEXT.md) is the glossary,
  [`docs/adr/`](./docs/adr/) holds decisions. Read both before working in an area they touch, and
  keep `/domain-modeling` as the way to change them. See
  [`docs/agents/domain.md`](./docs/agents/domain.md).
