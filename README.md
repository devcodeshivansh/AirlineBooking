# AirlineBooking (Commercial-grade .NET 8 API)

**Architecture:** Modular Monolith + Clean/Hexagonal + Vertical Slices (CQRS)  
**DB (dev):** SQLite (zero-setup) · **DB (prod-ready):** PostgreSQL  
**Key libs:** ASP.NET Core, EF Core, MediatR, FluentValidation, Serilog, Swagger, ProblemDetails

## Why this design
- Single deployable unit for speed and simplicity, **service-ready** via clear module boundaries.
- **CQRS/Vertical slices** keep features isolated and testable.
- **Idempotency** and **transactional consistency** via EF Core and a simple idempotency store.
- Switch DBs easily (SQLite ↔ PostgreSQL) using configuration.

## Quick start (Windows / Visual Studio 2022)
1. Extract the zip.
2. Open `AirlineBooking.sln` in VS 2022.
3. Set **Api** as startup project.
4. Run (F5). First run applies EF migrations and seeds sample flights.
5. Open Swagger at `https://localhost:PORT/swagger`.

## Deploy to Azure
1. Review the infrastructure templates under [`infra/`](infra/) and update `infra/parameters.example.json` with unique resource names. Copy it to `infra/parameters.json` when ready.
2. Deploy the infrastructure with the Azure CLI:
   ```bash
   az deployment group create \
     --resource-group <your-resource-group> \
     --template-file infra/main.bicep \
     --parameters @infra/parameters.json
   ```
3. Build and push the container image using Docker:
   ```bash
   az acr login --name <registryName>
   docker build -t <registryLoginServer>/airlinebooking-api:latest .
   docker push <registryLoginServer>/airlinebooking-api:latest
   ```
4. (Optional) Configure the provided [Azure Pipeline](azure-pipelines.yml) to automate build, test, image publishing, infrastructure deployment and Web App updates.
5. Set the required application settings or connection strings (e.g., `Database__UsePostgres`, `ConnectionStrings__Postgres`) either through pipeline variables or the Web App configuration UI.

### API samples
- **Search flights**: `GET /api/flights/search?from=DEL&to=BOM&date=2025-09-26`  
- **Create booking**:  
  ```json
  POST /api/bookings
  {
    "flightId": "<copy from search result>",
    "firstName": "Shivansh",
    "lastName": "Agarwal",
    "email": "agarwal.shivansh.1998@gmail.com",
    "seats": 2,
    "idempotencyKey": "client-unique-key-001"
  }
  ```
- **Confirm booking**: `POST /api/bookings/confirm/{pnr}`
- **Get booking**: `GET /api/bookings/{pnr}`

## Switch to PostgreSQL
1. Set `Database:UsePostgres` to `true` in `src/Api/appsettings.json` (or via env var).
2. Update connection string `ConnectionStrings:Postgres`.
3. Run `dotnet ef database update` (or start the API to auto-migrate).

> Tip: For local Postgres, use Docker:
> ```bash
> docker run --name pg -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16
> ```

## Project layout
```
src/
  Api/            # Minimal APIs, DI, middleware, Swagger, ProblemDetails, rate limiting
  Application/    # CQRS (MediatR), validators, pipeline behaviors, DTOs
  Domain/         # Entities, Value Objects, Domain rules
  Infrastructure/ # EF Core DbContext, Repos, Idempotency, (Outbox placeholder)
  Workers/        # Background workers (e.g., Outbox dispatcher)
tests/
  UnitTests/      # Domain/Application unit tests (xUnit + FluentAssertions)
build/
  Directory.Build.props  # shared compiler/package settings
```

## Next steps (if you want to extend)
- Add **Outbox** table and a **BackgroundService** in `Workers` to publish external events (e.g., to a bus).
- Implement **Authentication** (JWT via Azure AD / Entra) and **Authorization** (policies).
- Introduce **pricing rules**, **cancellation fees**, **PNR expiry**, and **hold reservations**.
- Add **OpenTelemetry** for tracing and metrics.
- Add **Integration Tests** using Testcontainers for Postgres.

---

Crafted for: **Shivansh Agarwal** · 2025-09-26T08:56:14.607468Z
