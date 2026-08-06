# GarageBox Design System

GarageBox is a mobile app for car owners: every service, every renewal and every PDF that belongs
to a car, kept against that car instead of scattered across a glovebox, a drawer and three inboxes.
The owner logs a **maintenance record** (oil change, tires, brakes…) or an **obligation**
(liability insurance, full coverage, roadworthiness inspection, road tax, vehicle tax), attaches
the document that proves it, and the app counts down to the next date.

**Dark-only, one warm amber accent, mono for anything you'd read out loud.** Playful in the copy,
completely literal about dates and money.

## Sources this system was built from

- **GitHub — https://github.com/Shtirkov/car-organizer** (branch `main`). The backend
  (ASP.NET Core, clean architecture) is real and fully defines the product's domain: `Vehicle`,
  `MaintenanceRecord`, `VehicleObligation`, `Document`, `Reminder`, the `DashboardResponse`
  payload, and the `MaintenanceType` / `ObligationType` / `ReminderType` enums. Every label,
  field and ordering rule in this system traces back to those files — read them if you want to
  extend the kit accurately.
- The repo's `frontend/` is still the untouched Vite + React starter (the roadmap puts the React
  frontend at phase 7). **There was no existing UI, no brand, no logo and no fonts to copy.**
  Everything visual here is new work, approved by the product owner: dark palette, amber accent
  `#FFB454`, Space Grotesk / Plus Jakarta Sans / JetBrains Mono, and a GarageBox mark.
- The repo also references `Car_Maintenance_Tracker_PRD_v1.docx` — **not present in the repo**,
  so it was not read. If you have it, it is the best next input for this system.

Locale note: the backend's obligation types carry Bulgarian names (Гражданска отговорност, Каско,
Технически преглед, Винетка, Данък МПС). Per the product owner these are surfaced with generic
international wording — *liability insurance, full coverage, roadworthiness inspection, road tax,
vehicle tax*.

## Content fundamentals

**Voice: playful but professional — dry, warm, short.** The app jokes about *paper*, never about
*deadlines*. A renewal date is stated flatly; the sentence around it can have a smile.

- **Person:** we say *we* for the app ("we'll nudge you"), *you* for the owner. Never "the user".
- **Casing:** sentence case everywhere — buttons, headers, labels. Uppercase only for the 11px
  eyebrow labels (RENEWALS, RECENT SERVICES), tracked +0.08em.
- **Length:** headlines ≤ 8 words, body ≤ 2 lines, button labels 1–3 words. Empty states get one
  warm line and one action.
- **Numbers and dates:** always concrete and mono — "18 days left", "€89", "184 300 km",
  "12 Mar 2026". Never "soon", never "a while ago".
- **Emoji: never.** Not in UI, not in notifications. The status color carries the emotion.
- **No exclamation marks in status copy.** "Inspection expired 9 days ago." not "Expired!"

Examples that are *right*:

> Your car's paperwork lives in six places.
> Insurance renews in 18 days. Want the PDF on file?
> Nice — that receipt is filed. One less thing in the glovebox.
> Park it in the garage *(the save button on Add car)*
> Cancel any time. No handbrake turns.

Examples that are *wrong*:

> Oops! You totally forgot your inspection 🙈 *(jokes about a missed deadline, emoji)*
> Leverage GarageBox to optimise your vehicle compliance workflow. *(corporate)*
> Your document was successfully uploaded to the system. *(system-speak; say "Filed.")*

## Visual foundations

**Palette.** Dark gamma only — there is no light theme. Surfaces climb in five near-black steps
(`#08090B` sunken → `#0D0F13` app → `#14161B` card → `#1A1D23` raised → `#23272F` hover), all
cool and low-chroma so the accent is the only warm thing on screen. One accent: `#FFB454`,
hover `#FFC880`, press `#E09330`, plus a 14% tint for selected states. Blue `#5B8CFF` is demoted to
the "service" category hue. Status is a fixed language:
green `#3DDC97` valid, orange `#FF8A3D` expiring, red `#FF5F5F` overdue, violet `#A78BFA`
informational. "Expiring" sits deliberately one step warmer than the amber accent so an action
never reads as a status. Category hues (service blue, insurance green, inspection violet, tax orange) stay
constant per record family. Text runs four steps only: `#F4F6FA` → `#B6BDC9` → `#8B93A3` → `#5A6272`.

**Type.** Space Grotesk 600/700 for anything titled (screen headlines, vehicle names, prices),
Plus Jakarta Sans 400–600 for every sentence and label, JetBrains Mono for plates, VIN, odometer,
money and dates. Display tracks tight (−0.03em), body slightly tight (−0.005em), eyebrows tracked
wide (+0.08em) and uppercase. Nothing below 11px; body rows are 14px, screen headlines 27–34px.

**Spacing & layout.** 4px base scale (4/8/12/16/20/24/32/40/48). Screen gutter 20px, 12px between
stacked cards, 28px between sections, 44px minimum tap target. Screens are a single scrolling
column; the tab bar is the only fixed element and it floats over the content on a translucent
`rgba(13,15,19,.86)` + 18px blur. Blur is used *only* there and behind modal scrims — never
decoratively.

**Backgrounds.** Flat `#0D0F13`. No photography, no textures, no patterns, no illustration. The
one exception is a soft amber radial glow (`--grad-hero`) behind the selected vehicle card and the
onboarding hero — 22% at the top, gone by 70%. The linear amber gradient `--grad-accent` appears
only in the logo mark. **No purple-blue mesh gradients, ever.**

**Cards.** `--surface-card` fill, 1px `#23272F` hairline, 20px radius, and a shadow that reads as
depth rather than drama: `0 8px 24px -12px rgba(0,0,0,.7)` with a 3% inner top highlight. Inner
cards use the raised surface at 14px radius. Radii ladder: 6 (checkbox) / 10 (icon button,
segment) / 14 (button, input) / 20 (card) / 28 (sheet) / pill (badges, chips, progress).

**Elevation on dark is lightness, not shadow** — a raised element gets a lighter surface plus the
hairline; the shadow only stops it from floating. The single glow in the system is
`--shadow-accent` under primary buttons and the selected plan card.

**Interaction.** Hover lightens the surface one step (never opacity fades). Press scales to 0.975
and holds the hover color — no color flash. Focus is a 3px `--accent-tint` halo plus an amber
border, never a browser outline. Disabled = 45% opacity on the raised surface.

**Motion.** Quick and boring on purpose: 120ms for hovers and presses, 200ms for surface and color
changes, 320ms for a bar or sheet travelling. Easing `cubic-bezier(.2,.8,.2,1)` out,
`cubic-bezier(.4,0,.2,1)` in-out. The only spring in the system (`cubic-bezier(.34,1.4,.5,1)`) is
the switch knob. No bounces on screens, no parallax, no scroll-linked animation, and nothing
animates on a status number — an overdue date should not be cute.

**Transparency** is limited to: the tab bar, modal scrims (`rgba(8,9,11,.72)`), status tints at
14% and the accent tint at 14/26%. Component fills are otherwise solid.

## Iconography

**Lucide** ([lucide.dev](https://lucide.dev), ISC) at stroke width **1.75** (Lucide ships 2 —
GarageBox runs one notch lighter), sizes 16 / 18 / 20 / 22px, always `currentColor`. The 45 icons
actually used are copied into `assets/icons/` and inlined in `components/core/Icon.jsx`, so no CDN
is needed at runtime. This is a **substitution** — the source repo contained no icon set of its
own (only the Vite starter's demo sprite, ignored). Swap the set and the `Icon` component is the
only file that changes.

Meaningful mappings, kept consistent: `car-front` a vehicle, `wrench` service, `droplet` oil,
`disc` tires, `battery`, `funnel` filters, `shield-check` insurance, `badge-check` full coverage,
`scan-line` inspection/VIN scan, `receipt` tax, `file-text` a document, `folder` documents,
`gauge` odometer, `bell` reminders, `sparkles` paid features. Emoji and unicode glyphs are never
used as icons. The provider marks inside `SsoButton` are the only multi-color, brand-owned art in
the system.

## Logo

There was **no logo in the source repo**. The mark in `assets/` is new: a rounded amber-gradient
square holding an abstract garage — a roof line over three door slats, the bottom one short.
`assets/logo.svg` (gradient tile) for light or neutral contexts, `assets/logo-dark.svg` (mono, on
app background) inside the product. The wordmark is simply *GarageBox* in Space Grotesk 700 at
−0.03em, mark height ≈ wordmark cap height × 1.6, clear space one third of the mark.

## Index

| Path | What's there |
| --- | --- |
| `styles.css` | The single entry point consumers link — imports everything below |
| `tokens/` | `fonts.css`, `colors.css`, `typography.css`, `spacing.css`, `effects.css`, `base.css` |
| `components/core/` | Primitives |
| `components/app/` | Product blocks |
| `ui_kits/mobile-app/` | The five-screen click-through app kit + its README |
| `guidelines/` | Foundation specimen cards (colors, type, spacing, brand, variations) |
| `assets/` | `logo.svg`, `logo-dark.svg`, `icons/` (45 Lucide SVGs) |
| `SKILL.md` | Agent-skill wrapper for use outside this project |
| `github.md` | Upstream repo association and sync record |

### Components

Core primitives — `Button`, `IconButton`, `Icon`, `Input`, `Select`, `Checkbox`, `Switch`,
`Card`, `Badge`, `SegmentedControl`, `ProgressBar`, `ListRow`.

Product blocks — `VehicleCard`, `StatTile`, `ObligationRow`, `MaintenanceRow`, `PlanCard`,
`SsoButton`, `EmptyState`, `AppHeader`, `TabBar`, `DocumentChip`, `SectionHeader`.

Each has a sibling `.d.ts` (props contract) and `.prompt.md` (what & when, with a usage example).

### Intentional additions

The source defines no components at all, so the whole inventory above is new. Two calls worth
naming: `Icon` exists as a wrapper so the icon set can be swapped in one file, and `ListRow` was
factored out because `ObligationRow` and `MaintenanceRow` are the same line with different
semantics.

### Variations to decide on

`guidelines/variation-a.card.html` is the shipping direction: the warm amber action color from
Direction B on the graphite base and type of Direction A. `guidelines/variation-b.card.html` keeps
the original blue accent for comparison — not wired into the tokens.

## Known gaps

- No font binaries: the three families load from Google Fonts via `tokens/fonts.css`. If GarageBox
  licenses its own faces, drop the files in `assets/fonts/` and swap that `@import` for real
  `@font-face` rules.
- No product screenshots or marketing site exist yet, so there is one UI kit (the app) and no
  slide template.
