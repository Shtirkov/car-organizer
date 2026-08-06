# GarageBox — app design snapshot

Frozen copy of the **GarageBox mobile app** design, imported from Claude Design on **6 Aug 2026**.
It is stored here so the client work in Phase 7 has a stable reference — the remote project can
keep moving without changing what we agreed to build.

Nothing here is wired into the build. It is documentation and a browsable prototype.

## Provenance

| | |
| --- | --- |
| Project | *Garage box mobile app design* |
| URL | https://claude.ai/design/p/97fe9b32-a72a-4dae-8dcd-e79c747f5c5b |
| Design system | *GarageBox Design System* (`_ds/garagebox-design-system-0843ebc0-…`) |
| Upstream sync | 2026-08-06T05:42:22Z, from `Shtirkov/car-organizer` @ `main` |
| Imported | 2026-08-06 |

The design was generated **from this repo** — every label, enum, limit and validation rule in it
traces back to our `Domain/` entities and `Application/` DTOs. That is why the gap list below is
worth taking seriously: it is a list of things the design needs and the API genuinely cannot do,
not a wish list from someone who never read the code.

## What's in here

```
design/
├── GarageBox App.dc.html   # the whole app: 12 screens, clickable prototype, gap list
├── GBDashboard.dc.html     # dashboard alone, 3 directions (renewals / fleet / timeline)
├── support.js              # the .dc.html runtime (generated; don't edit)
├── github.md               # upstream sync record + screen → source-file map
└── _ds/garagebox-design-system-0843ebc0-…/
    ├── readme.md           # the design system spec — palette, type, voice, motion, icons
    ├── styles.css          # single entry point; imports the six token files
    ├── tokens/*.css        # fonts, colors, typography, spacing, effects, base
    └── _ds_bundle.js       # the 25 components, compiled (generated; don't edit)
```

**Not copied:** `_ds_manifest.json`, `_adherence.oxlintrc.json` and `.thumbnail` — tooling metadata
for the Design System pane, of no use to the implementation. The remote project stays the source of
record for those.

To view: open `GarageBox App.dc.html` in a browser. The relative paths are preserved, so it renders
without a server — but **not offline**: `support.js` pulls React, ReactDOM and Babel from unpkg at
runtime, and the three webfonts come from Google Fonts.

## Screens

Fourteen routes in the prototype; the design calls it "12 screens".

`auth` (sign in / register) · `onboarding` · `home` (dashboard) · `empty` (empty garage) ·
`vehicle` (detail) · `records` (fleet-wide) · `addVehicle` · `addRecord` · `addObligation` ·
`documents` · `docViewer` · `reminders` · `settings` · `paywall`

Tab bar: Garage · Records · Add · Documents · Profile.

`GBDashboard.dc.html` explores three home screens. **Direction B (fleet-first — pick a car, then
read its paperwork) is the one that matches what we built**: `GET /api/dashboard` already groups by
vehicle, which is exactly this shape. Direction A (one big countdown) would want a fleet-wide
"soonest renewal" the endpoint doesn't surface; Direction C (one timeline) flattens across the
garage, which we deliberately decided against.

## Backend gaps

The design's own list, all thirteen verified against the current code. Ordered by our roadmap.

### Phase 6b — reminders + push (the next phase; these are its scope)

1. **Reminder generation and delivery.** `Reminder`, `ReminderType` and `ReminderConfiguration`
   exist; nothing else does — no store, no service, no controller, nothing turning `ValidUntil`
   into a reminder and nothing sending it. Needs a scheduled generator, `GET /api/reminders`, and
   a completion endpoint.
2. **Push + email channels.** No device-token storage, no mail sender, no per-obligation channel
   preference. The "Remind me" block on *Add renewal* writes nowhere today. Needs
   `NotificationPreference`, `DeviceToken`, an `ISender`.

### Before the Expo client ships (Phase 7)

3. **User profile fields.** `User : IdentityUser<Guid>` has no display name; `/api/auth/me` returns
   id and email only. "Hello, Ivan" and the avatar initials both need a name.
4. **SSO, password reset, email confirmation.** `AuthController` has register / login / refresh /
   logout / me. Apple and Google sign-in, forgot-password and confirmation mail are all absent —
   and Apple sign-in is effectively mandatory on the App Store once any third-party SSO is offered.
5. **Owner-wide documents list.** Documents are only reachable per vehicle
   (`api/vehicles/{vehicleId:guid}/documents`). The Documents tab lists the whole fleet, so it
   needs `GET /api/documents` with filters — or one round trip per car.
6. **Search.** The header search hits nothing. One endpoint across plate, make/model, provider,
   policy number and file name covers every use in the design. *(Already on our deferred list, and
   an MVP feature in the PRD.)*
7. **Cost totals.** `DashboardResponse` carries no money. "Spent 2026" on the vehicle screen needs a
   per-vehicle, per-period aggregate — summing client-side means fetching every record.
8. **Renewing an obligation.** There is no renew action; the client would POST a fresh obligation
   and the old one just sits there. Decide whether a renewal supersedes its predecessor and link
   them.
9. **Odometer update without a service.** `CurrentMileage` only moves via a maintenance record or a
   full vehicle `PUT`. The app wants a one-tap odometer update.
10. **Language / locale.** No locale on the user, and `ObligationType` is Bulgarian-named in the
    enum while the design's UI is English. Decide where the translation lives before the client
    hard-codes it. Note the enum wire format is append-only once the app ships.

### Phase 8 — deploy (+ R2)

11. **Thumbnails and signed URLs.** We expose metadata plus a byte stream at `/{id}/content`. A
    phone list wants a thumbnail and a short-lived URL rather than streaming 15 MB through the API
    to render a preview. *(Sits with the presigned-upload item already deferred to Phase 8.)*

### Undecided — product calls, not just missing endpoints

12. **Subscriptions and entitlements.** The design has a paywall, a Pro trial and a free tier
    (1 car, 10 documents). We have no plan, no billing, no server-side limits. If this ships, the
    caps belong in `VehicleService.CreateAsync` and `DocumentService.UploadAsync` — not hidden in
    the UI. **The PRD does not describe a business model at all; this is new scope.**
13. **Currency on money fields.** `Cost` is a bare `decimal` on both `MaintenanceRecord` and
    `VehicleObligation`, and there is no user currency preference. Multi-currency needs a column or
    a user-level setting before any total can be trusted — and gap 7 (totals) depends on it.

## Implementation notes for Phase 7

- **The tokens are CSS custom properties; React Native has none.** Port `tokens/*.css` to a
  TypeScript theme object once, and treat that file as generated-from-here. The values, not the
  mechanism, are what was approved.
- **Dark only.** There is no light theme, by design. Don't build one "for free".
- **Status language is fixed**: green valid · orange expiring · red overdue · violet informational,
  with the amber accent reserved for *actions* so a button never reads as a status. Our dashboard's
  overdue/expiring split maps onto it directly.
- **Enum labels are English in the design, Bulgarian in the enum** (Гражданска отговорност, Каско,
  Технически преглед, Винетка, Данък МПС → liability insurance, full coverage, roadworthiness
  inspection, road tax, vehicle tax). See gap 10 — this is a decision, not a translation task.
- The design system's `readme.md` is the authority on voice, spacing, motion and iconography.
  Read it before writing screen copy; it is opinionated and specific (no emoji, no exclamation
  marks in status copy, concrete numbers only).
