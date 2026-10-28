# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

.NET 10 solution (`ECommBackend.slnx`). Package versions are managed centrally in `Directory.Packages.props`. Add `<PackageReference>` entries without versions and put the version there. `Directory.Build.props` sets `net10.0`, nullable and implicit usings for every project.

```bash
dotnet restore ECommBackend.slnx
dotnet build ECommBackend.slnx
dotnet test ECommBackend.slnx                                      # all tests (NUnit)
dotnet test ECommBackend.Tests --filter "FullyQualifiedName~VariantModelTests"   # one fixture
dotnet test ECommBackend.Tests --filter "Name=SomeTestName"                       # one test

dotnet run --project ECommBackend.AppHost     # Aspire: API + email consumer + Mailpit container
dotnet run --project ECommBackend.csproj      # API only (Scalar UI at /scalar in Development)
docker compose up --build                     # API :5000, consumer :5001, Mailpit UI :8025
```

EF Core migrations use the local tool (`dotnet tool restore` first). CI fails if the model has changes that no migration covers:

```bash
dotnet tool run dotnet-ef migrations add <Name> --project ECommBackend.csproj --context SQLiteConn
dotnet tool run dotnet-ef migrations has-pending-model-changes --project ECommBackend.csproj --startup-project ECommBackend.csproj --context SQLiteConn
```

CI (`.github/workflows/ci.yml`) runs restore, a Release build, the tests and the pending-model check, then builds both Docker images. It pushes them to GHCR only from `main` and from `v*` tags.

`scripts/stress-orders.ps1` (PowerShell 7) sends concurrent `POST /api/orders` requests for load testing.

## Configuration / secrets

- The API and the consumer both need `SecretKeys:PrimaryConn` (an Azure Service Bus connection string). It comes from user secrets and is validated on start, so both apps fail at startup without it. docker-compose mounts `%APPDATA%/Microsoft/UserSecrets` into the containers.
- The database is SQLite (`ConnectionStrings:ECommSQLite`, the file `ECommBackend.db`). `SQLiteConn` is the only DbContext in use. `SQLConn.cs` is an unused leftover; don't add migrations against it.
- OTLP export turns on only when `OTEL_EXPORTER_OTLP_ENDPOINT` is set (see `ECommBackend.ServiceDefaults/Extensions.cs`).

## Architecture

Projects:
- **`ECommBackend.csproj`** (repo root): the ASP.NET Core Web API. Because it sits at the root, its `DefaultItemExcludes` leaves out the sibling project folders. Keep that list current if you add a project folder.
- **`ConsumerProject`**: a separate worker that reads the `orderemailqueue` Service Bus queue and sends emails over SMTP with MailKit. Locally, mail goes to Mailpit.
- **`ECommBackend.ServiceDefaults`**: shared `AddServiceDefaults(serviceName)` (OpenTelemetry, health endpoints) and `MessageTracing`, which propagates trace context through Service Bus `ApplicationProperties`. ActivitySources and Meters named `ECommBackend.*` are picked up automatically.
- **`ECommBackend.AppHost`**: the Aspire orchestrator.
- **`ECommBackend.Tests`**: NUnit tests that reference the API project. Most coverage is model-validation tests. The `RepositoryTests/*` files are empty stubs, and `OrderMockTests.cs` is fully commented out.

API layering (all registered in `Program.cs`): `Controllers` → `Services` (concrete classes, no interfaces) → `Repositories` (behind the interfaces in `Repositories/RepoInterfaces`) → `SQLiteConn`. `DTOs/FrontendDTO` holds the request shapes, the `DTOs/*DTO.cs` records are the response shapes, and `DTOs/MapToDomain` holds the extension methods (`ModelToRecordDTO()` and similar) that map between them. Domain "not found" exceptions live in `CustomErrors`. `GlobalExceptionLayer` middleware turns unhandled exceptions into a JSON 500 and records them on the current trace span.

Auth: JWT bearer, with the token read from the `access_token` cookie in `OnMessageReceived`. The policies are `adminUsers` (role `admin`) and `users` (role `logged_in`). Some endpoints take the caller's id from a `loggedInUserId` header.

### Asynchronous order flow (spans several files)

1. `OrderService.CreateOrder` saves the order as `Pending`. It then publishes one message **per variant** to the `userorderqueue` queue through `AzureServiceBusClass`. Each message is an `OrderModel` serialized with Newtonsoft, and its `MessageId` is the order id.
2. `Services/AzureBackgroundService` (a hosted service inside the API) receives these messages. Orders that are no longer `Pending` are skipped as duplicates. Otherwise it picks the cheapest in-stock variant for each product and calls `VariantService.TryReserveUnits`. It then calls `OrderService.UpdateOrderStatus` to set `Processed` or `Rejected`. Unreadable or empty messages go to the dead-letter queue.
3. `UpdateOrderStatus` records metrics in `OrderTelemetry` and publishes an `OrderEmailMessage` to `orderemailqueue`.
4. `ConsumerProject/Services/ConsumerBackground` sends the email. `OrderEmailMessage` exists in **both** projects (`DTOs/OrderEmailMessage.cs` and `ConsumerProject/Models/OrderEmailMessage.cs`), so a change to one must be made to the other.

Both sides create their queues on demand if they don't exist. Queue names and options are constants and factory methods on `AzureServiceBusClass`. `PendingOrdersGaugeService` feeds the pending-orders gauge.

## Code review

`.github/agents/senior-swe-review.agent.md` defines the review style this repo expects. Review findings for correctness, security, data integrity and reliability before style. Each finding gets a severity (Blocker/High/Medium/Low), a location, the issue, its impact and a recommendation. Performance claims must be backed by evidence, not assumed.
