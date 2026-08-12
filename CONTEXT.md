# Car Maintenance Tracker

Everything a car owner needs to remember about their car, in one place: what was done to it, what
paperwork proves it, and what falls due next. One person, their own cars — not a fleet, not a shop.

## Language

### People and cars

**Owner**:
The person a vehicle belongs to. Every vehicle has exactly one, and it never changes.
_Avoid_: user (that's the sign-in identity), account, customer, driver

**Garage**:
The whole set of vehicles belonging to one owner. A view of a collection, not a place.
_Avoid_: fleet, collection

**Vehicle**:
One car an owner keeps records for.
_Avoid_: car, auto, asset

**Mileage**:
A distance reading from a vehicle's odometer, in kilometres. Always an absolute reading, never a
delta. **Purchase mileage** is the reading when the owner acquired the vehicle and never changes;
**current mileage** is the latest known reading and only ever goes up.
_Avoid_: odometer (the instrument, not the reading), kilometrage, distance

### What happens to a car

**Maintenance record**:
A note that work was done on a vehicle on a date, at a mileage, for a cost. A record of the past.
_Avoid_: service, job, repair, entry

**Obligation**:
An administrative duty that must be kept current for the vehicle to be legal or covered —
insurance, casco, technical inspection, vignette, tax. Distinguished from a maintenance record by
having a **validity period** rather than a single date.
_Avoid_: renewal (that's the act, not the thing), policy, requirement, compliance item

**Validity period**:
The span an obligation covers. Its end is what makes an obligation expire and what everything
urgent is calculated from.

**Renewal**:
The act of replacing an expiring obligation with a fresh one covering the next period. A verb made
into a noun — the resulting thing is still an obligation.

**Document**:
A photo or PDF proving that a maintenance record or an obligation is real. Every document belongs
to exactly one of them; a document that proves nothing has no reason to exist.
_Avoid_: file, attachment, upload, paperwork

**Reminder**:
A standing instruction to tell the owner about something before it arrives. Unlike an obligation it
can be triggered by a mileage as well as a date, which is what lets it cover service intervals
("oil change every 10,000 km") that no obligation can express. A reminder is a thing the owner
created and stored — the dashboard's overdue and expiring lists are **not** reminders, they are
obligations sorted by urgency.
_Avoid_: alert, notification (that's the delivery, not the thing), task, todo

**Subject**:
What a reminder is about, named with the vocabulary that already owns it — a kind of maintenance,
or a kind of obligation. Reminders have no vocabulary of their own, so each concept has one name
and one code wherever it appears.
_Avoid_: reminder type, category

### Urgency

**Overdue**:
Past its date, still not dealt with. Counted in days since. Never expires from the list — a lapsed
renewal stays a problem however old it is.

**Expiring**:
Coming due within the horizon the owner is currently looking at, but not yet past. Counted in days
remaining. Something due **today** is expiring, not overdue: it can still be dealt with.

**Horizon**:
How far ahead the owner is currently looking. Bounds the future only; it never hides anything
overdue.

### Money and presentation

**Cost**:
What a maintenance record or an obligation cost the owner.

**Currency**:
The single currency an owner keeps their costs in. A property of the owner, not of each cost —
this project deliberately does not model an owner holding costs in several currencies at once.

**Locale**:
The language and regional formatting an owner wants to be addressed in.

### Access

**Plan**:
What an owner is entitled to: **Free** or **Pro**. Free is capped (vehicles, documents); Pro
removes the caps.
_Avoid_: tier, level, package, SKU

**Entitlement**:
An owner's current plan together with its standing — trialing, active, past due, cancelled or
expired. The authority on what an owner may do, and always established from the payment platform
rather than from anything the app is told by a client.
_Avoid_: permission, licence, grant

**Trial**:
A time-limited spell of full Pro capability. A standing that a Pro plan can be in, **not** a plan
of its own — a trialing owner can do everything a paying one can.
