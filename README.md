# Car Maintenance Tracker

Web application that helps vehicle owners keep all maintenance history, documents and
reminders in one place. See the full product spec in
[`Car_Maintenance_Tracker_PRD_v1.docx`](./Car_Maintenance_Tracker_PRD_v1.docx).

## Tech stack

| Layer       | Technology                                              |
| ----------- | ------------------------------------------------------- |
| Backend     | ASP.NET Core 10 Web API (clean architecture)            |
| Database    | PostgreSQL + Entity Framework Core                      |
| Auth        | Own JWT (ASP.NET Identity, access + refresh tokens)     |
| File storage| Cloudflare R2 (S3-compatible) via AWS SDK               |
| Frontend    | React (Vite + TypeScript)                               |
| Deployment  | Docker + GitHub Actions → Railway                       |

## Solution layout

```
car-organizer/
├── backend/
│   ├── CarOrganizer.slnx
│   ├── CarOrganizer.Domain/          # Entities, value objects, domain logic
│   ├── CarOrganizer.Application/     # Use cases, DTOs, interfaces
│   ├── CarOrganizer.Infrastructure/  # EF Core, repositories, external services
│   └── CarOrganizer.API/             # Controllers, DI, startup
├── frontend/                         # React + Vite + TypeScript
├── docs/
└── docker-compose.yml                # Local PostgreSQL
```

## Getting started (local)

```bash
# 1. Start the local database
docker compose up -d

# 2. Run the API
cd backend
dotnet run --project CarOrganizer.API

# 3. Run the frontend
cd ../frontend
npm install
npm run dev
```

Local Postgres connection: `Host=localhost;Port=5432;Database=car_organizer;Username=carorg;Password=carorg_dev_pw`

## Roadmap

MVP is built phase by phase: **0** project setup → **1** domain + DB → **2** JWT auth →
**3** vehicles/garage → **4** maintenance records → **5** documents → **6** dashboard +
reminders → **7** React frontend → **8** deploy to Railway → **9** feedback & iteration.
