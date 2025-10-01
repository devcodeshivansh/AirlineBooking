# AirlineBooking (Commercial-grade .NET 8 API)

**Architecture:** Modular Monolith + Clean/Hexagonal + Vertical Slices (CQRS)  
**DB (dev):** SQLite (zero-setup) · **DB (prod-ready):** PostgreSQL  
**Key libs:** ASP.NET Core, EF Core, MediatR, FluentValidation, Serilog, Swagger, ProblemDetails

## Why this design
- Single deployable unit for speed and simplicity, **service-ready** via clear module boundaries.
- **CQRS/Vertical slices** keep features isolated and testable.
- **Idempotency** and **transactional consistency** via EF Core and a simple idempotency store.
- Switch DBs easily (SQLite ↔ PostgreSQL) using configuration.

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
  Infrastructure/ # EF Core DbContext, Repos, Idempotency
tests/
  UnitTests/      # Domain/Application unit tests (xUnit + FluentAssertions)
build/
  Directory.Build.props  # shared compiler/package settings
```