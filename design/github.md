repo: Shtirkov/car-organizer
branch: main

## Last sync

date: 2026-08-06T05:42:22Z

### Updated in this project

- Designed the GarageBox iOS app (12 screens) as a stateful prototype on the bound design system.
- Three dashboard directions explored: renewal-first, fleet-first, fleet timeline.
- All labels, enums, limits and validation rules lifted from the backend domain + Application DTOs.
- Documented 13 backend gaps the design depends on (reminders, notifications, billing, currency, search…).

## Screen map

| Screen | Built from |
| --- | --- |
| Auth (sign in / register) | API/Controllers/AuthController.cs, Application/Auth/*.cs |
| Onboarding / empty garage | Application/Vehicles/VehicleResponse.cs |
| Dashboard (3 directions) | Application/Dashboard/DashboardResponse.cs, Infrastructure/Dashboard/DashboardService.cs, Application/Dashboard/DashboardLimits.cs |
| Vehicle detail | Domain/Entities/Vehicle.cs, Application/Vehicles/VehicleResponse.cs |
| Add / edit car | Application/Vehicles/CreateVehicleRequest.cs, VehicleLimits.cs, VehicleMileage.cs |
| Log a service | Domain/Entities/MaintenanceRecord.cs, Domain/Enums/MaintenanceType.cs, Application/MaintenanceRecords/*.cs |
| Add a renewal | Domain/Entities/VehicleObligation.cs, Domain/Enums/ObligationType.cs, Application/Obligations/*.cs |
| Records (fleet-wide) | Infrastructure/MaintenanceRecords/MaintenanceRecordService.cs |
| Documents + upload + viewer | API/Controllers/DocumentsController.cs, Application/Documents/DocumentLimits.cs, DocumentResponse.cs |
| Reminders | Domain/Entities/Reminder.cs, Domain/Enums/ReminderType.cs (no service/controller yet) |
| Settings / profile | Domain/Entities/User.cs, AuthController.Me |
| Paywall | no backend source — new, see backend gaps |

Files: `GarageBox App.dc.html` (all screens + gap list), `GBDashboard.dc.html` (dashboard, 3 variants).
