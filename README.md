# NiWaterWatch

A REST API for monitoring river water quality across Northern Ireland — seeded with 30+ years of official DAERA monitoring data, layered with user-submitted community readings.

Built as a portfolio project to demonstrate a full, production-shaped .NET backend: SQL-backed persistence, layered architecture, JWT authentication, and automated testing.

## What it does

- Serves official dissolved oxygen readings from **840 monitoring stations** across Northern Ireland, spanning **1990–2024** (151,037 readings, sourced from DAERA/NIEA open data).
- Lets registered users submit their own readings for a station, clearly distinguished from official data.
- Exposes it all through a documented, paginated REST API.

## Tech stack

| Layer | Technology |
|---|---|
| Runtime | .NET 10 / ASP.NET Core Web API |
| Database | PostgreSQL, via EF Core |
| Auth | JWT bearer tokens, BCrypt password hashing |
| Testing | xUnit, Moq, EF Core InMemory provider |
| API docs | Swagger / OpenAPI (Swashbuckle) |
| Local dev | Docker (Postgres container) |

## Architecture

A layered / clean architecture, kept deliberately explicit rather than collapsed into a single project:

```
NiWaterWatch.Domain          — entities only (Station, Reading, ApplicationUser). No dependencies.
NiWaterWatch.Infrastructure  — EF Core: AppDbContext, Repository, migrations. Depends on Domain.
NiWaterWatch.Api             — Controllers, Services, Contracts (DTOs). Depends on both.
NiWaterWatch.Tests           — xUnit tests for the Api service layer.
NiWaterWatch.Importer        — one-off CLI tool that seeded the database from the DAERA CSV export.
```

Domain has zero external dependencies by design — that's what makes the service layer unit-testable without a running database (see `NiWaterWatch.Tests`).

Full design notes, including the request-flow trace and database schema decisions, are in [`ARCHITECTURE.md`](ARCHITECTURE.md), [`ARCHITECTURE_NOTES.md`](ARCHITECTURE_NOTES.md), and [`DATABASE.md`](DATABASE.md).

## Getting started

**Prerequisites:** .NET 10 SDK, Docker Desktop.

**1. Clone the repo**

```bash
git clone https://github.com/pdoherty1002/NiWaterWatch.git
cd NiWaterWatch
```

**2. Start Postgres**

```bash
docker run --name niwaterwatch-db -e POSTGRES_PASSWORD=postgres -e POSTGRES_DB=niwaterwatch -p 127.0.0.1:5432:5432 -d postgres:16
```

**3. Apply migrations**

```bash
dotnet ef database update --project src/NiWaterWatch.Infrastructure --startup-project src/NiWaterWatch.Api
```

**4. Run the API**

```bash
dotnet run --project src/NiWaterWatch.Api
```

Swagger UI is available at `/swagger` once the API is running.

> The database seeded by the migrations above is empty. `NiWaterWatch.Importer` (in `tools/`) is the tool that loaded the original 151,037 DAERA readings — it expects the raw DAERA CSV locally and isn't required to run the API itself.

## Running tests

```bash
dotnet test
```

Tests use Moq (for repository-backed services) and the EF Core InMemory provider (for services querying `AppDbContext` directly) — no Postgres connection required.

## API reference

| Method | Endpoint | Auth | Description |
|---|---|---|---|
| GET | `/api/stations` | — | List all monitoring stations |
| GET | `/api/stations/search?name=` | — | Case-insensitive station name search |
| GET | `/api/stations/{id}` | — | Get a single station |
| GET | `/api/stations/{id}/readings?page=&pageSize=` | — | Paginated readings for a station, most recent first |
| POST | `/api/stations/{id}/readings` | Required | Submit a new reading for a station |
| POST | `/api/auth/register` | — | Create an account |
| POST | `/api/auth/login` | — | Log in, receive a JWT |

Authenticated requests carry the JWT in an `Authorization: Bearer <token>` header. Full request/response schemas are in Swagger.

## Data source

Dissolved oxygen readings sourced from [DAERA](https://www.daera-ni.gov.uk/) (Department of Agriculture, Environment and Rural Affairs) open data. Official data is imported unmodified; readings above 20 mg/l (0.21% of the dataset) are flagged as statistically unusual during import but not filtered out.

## Roadmap

- CI/CD (GitHub Actions — build + test on push)
- Deployment
- Frontend: map homepage with station pins, colour-coded by status
