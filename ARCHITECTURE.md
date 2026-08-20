# API architecture

Three layers, each with exactly one job. None of them know how the others do theirs.

| Layer | File example | Job | Knows about |
|---|---|---|---|
| Controller | `StationsController.cs` | Turn HTTP into a method call, and a return value into HTTP | The service it calls. Nothing else. |
| Service | `StationService.cs` | Business logic. Maps entities → DTOs | The repository interface. Not EF Core, not HTTP. |
| Repository | `Repository.cs` | Generic data access | `AppDbContext`. Nothing about DTOs or HTTP. |

## Request flow — GET /api/stations

1. **Swagger/client** sends `GET /api/stations`.
2. **`StationsController`** — `[Route]` matches the URL to this class, `[HttpGet]` matches the method. Calls `_stationService.GetAllAsync()`. Wraps the result in `Ok(...)`.
3. **`StationService`** — calls the repository for raw `Station` entities, maps each one to a `StationDto`. **This is the entity → DTO boundary.**
4. **`Repository<Station, int>`** — thin wrapper, calls `AppDbContext`'s `DbSet.ToListAsync()`.
5. **EF Core** generates `SELECT * FROM "Stations"`, runs it, maps rows back to `Station` objects.

Response unwinds back up the same path: `List<Station>` → `List<StationDto>` → wrapped in `Ok()` → serialized to JSON automatically by ASP.NET Core.

## Why split it into layers at all

- **Controller doesn't know Postgres exists.** Service doesn't know HTTP exists. Repository doesn't know what a DTO is.
- **Testability** — `StationService` depends on `IRepository<T, TKey>` (the interface), not `Repository<T, TKey>` (the real EF Core class). A test can hand it a fake repository and never touch a real database.
- **DTOs are the API's contract, not the database's.** `Station` (entity) can change internally without breaking `StationDto` (what callers see), and vice versa.

## Glossary

- **DTO** — Data Transfer Object. Plain record shaping what the API sends back.
- **DI** — Dependency Injection. `Program.cs` registers `AddScoped<StationService>()`; ASP.NET Core builds and hands it to the controller automatically — nothing manually `new`'d.
- **Repository pattern** — abstraction over data access, so services depend on an interface, not EF Core directly.
- **`IRepository<T, TKey>`** — generic: `T` = entity type, `TKey` = its primary key type (`int` for Station, `Guid` for User).
