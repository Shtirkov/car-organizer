# Reminders borrow their subject vocabulary instead of owning one

**Status:** accepted (12 Aug 2026)

`ReminderType` was written in Phase 1 alongside `MaintenanceType` and `ObligationType` and never
used. It turned out to be roughly the union of the other two — the same concepts renumbered
(`TechnicalInspection` is 3 as an obligation but 2 as a reminder; `OilChange` is 1 as maintenance
but 3 as a reminder), missing `Casco` and `Tax` entirely, and adding a `Service` value that exists
nowhere else. Since enum values cross the wire as numbers and are append-only once the mobile app
ships, we decided — before building reminders — that a reminder **names its subject with the
vocabulary that already owns it**: a kind of maintenance, or a kind of obligation. `ReminderType`
goes away.

## Why not the alternatives

- **Keep all three.** Free today, but the client would carry three lookup tables for one set of
  concepts, and "remind me about the car tax" would stay impossible because `Casco` and `Tax` have
  no reminder value. Every one of these costs compounds after the app ships and the numbers freeze.
- **Merge all three into one `CarEventType`.** The cleanest end state, but it renumbers
  `MaintenanceType` and `ObligationType`, which our own append-only rule forbids, and buys little:
  maintenance kinds and obligation kinds genuinely are different sets, not one set used twice.
- **Drop the type entirely** (title + due date/mileage only). Simplest, but the client can no longer
  tell an oil change from an insurance renewal without parsing free text, which the design's
  per-kind iconography needs.

## Consequences

- `Inspection` (a garage once-over, `MaintenanceType`) and `TechnicalInspection` (the legal
  roadworthiness test, `ObligationType`) stay separate values with confusingly close names. Keep
  them distinct in labels; do not merge them.
- A reminder now carries a discriminator plus a value. Gap 01 story 01 builds this, so the schema
  change lands before anything depends on the old shape — and `Reminders` has never held a row.
- Gap 11's enum → label mapping for the client shrinks to two vocabularies instead of three.
