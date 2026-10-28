# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project overview

ECommBackend is the backend for an e-commerce shop (the JWT audience is `ECommFrontend`). It's an ASP.NET Core Web API covering products and their variants (size, price, stock), categories, images, users, admins and orders. Admins own products, and users place orders.

The API accepts an order right away as `Pending`. Stock is reserved later, off the request path, by a background worker that reads an Azure Service Bus queue. That worker marks the order `Processed` or `Rejected` and queues an email. A separate consumer service sends the email over SMTP. OpenTelemetry traces cover all of it, from the HTTP request through both queues to the SMTP send.

Stack: .NET 10, EF Core on SQLite, Azure Service Bus, MailKit, Serilog, OpenTelemetry, .NET Aspire for local orchestration, Scalar for API docs, and NUnit for tests.

## Commands

The solution file is `ECommBackend.slnx`. Package versions are managed centrally in `Directory.Packages.props`: add `<PackageReference>` entries without a version and put the version there. `Directory.Build.props` sets `net10.0`, nullable and implicit usings for every project.

```bash
dotnet restore ECommBackend.slnx
dotnet build ECommBackend.slnx
dotnet test ECommBackend.slnx                                                    # all tests (NUnit)
dotnet test ECommBackend.Tests --filter "FullyQualifiedName~VariantModelTests"   # one fixture
dotnet test ECommBackend.Tests --filter "Name=SomeTestName"                      # one test

dotnet run --project ECommBackend.AppHost     # Aspire: API + email consumer + Mailpit container + dashboard
dotnet run --project ECommBackend.csproj      # API only (Scalar UI at /scalar, OpenAPI at /openapi/v1.json; Development only)
docker compose up --build                     # API :5000, consumer :5001, Mailpit UI :8025 (SMTP :1025)
```

EF Core migrations use the local tool (run `dotnet tool restore` first). CI fails if the model has changes that no migration covers:

```bash
dotnet tool run dotnet-ef migrations add <Name> --project ECommBackend.csproj --context SQLiteConn
dotnet tool run dotnet-ef database update --project ECommBackend.csproj --context SQLiteConn
dotnet tool run dotnet-ef migrations has-pending-model-changes --project ECommBackend.csproj --startup-project ECommBackend.csproj --context SQLiteConn
```

CI (`.github/workflows/ci.yml`) runs restore, a Release build, the tests and the pending-model check. It then builds two Docker images, `ecommbackend` and `ecommbackend-consumer`, and pushes them to GHCR only from `main` and from `v*` tags. `APP_VERSION` is baked into the images and served at `GET /version`.

`scripts/stress-orders.ps1` (PowerShell 7) sends concurrent `POST /api/orders` requests for load testing: `-OrderCount`, `-Concurrency`, `-UserId`, `-ProductIds`.

Health endpoints: `/health/live`, `/health/db` (a JSON report of the database check), and the ServiceDefaults endpoints `/health` and `/alive` (Development only).

## Configuration / secrets

- The API and the consumer both need `SecretKeys:PrimaryConn`, an Azure Service Bus connection string. It comes from user secrets and is validated on start, so both apps fail at startup without it. There's no local Service Bus emulator, so a real namespace is required. docker-compose mounts `%APPDATA%/Microsoft/UserSecrets` into the containers.
- The database is SQLite (`ConnectionStrings:ECommSQLite`, the file `ECommBackend.db`). `SQLiteConn` is the only DbContext in use. `SQLConn.cs` is an unused leftover; don't add migrations against it.
- JWT settings live in the `JWTSettings` section of `appsettings.json`. The keys there are for development only. The access token lasts 5 minutes and the refresh token 9 days.
- Under Aspire, the AppHost overrides the consumer's `Smtp__*` settings so its mail goes to the Mailpit container.
- OTLP export turns on only when `OTEL_EXPORTER_OTLP_ENDPOINT` is set (Aspire sets it).

## Architecture

### Projects

| Project | Role |
|---|---|
| `ECommBackend.csproj` (repo root) | The Web API, plus the order-processing `BackgroundService`. Because it sits at the root, its `DefaultItemExcludes` leaves out the sibling project folders. Keep that list current if you add a project folder. |
| `ConsumerProject` | A separate worker that reads `orderemailqueue` and sends emails with MailKit. |
| `ECommBackend.ServiceDefaults` | Shared `AddServiceDefaults(serviceName)` (OpenTelemetry, health endpoints) and `MessageTracing`, which carries trace context through Service Bus `ApplicationProperties`. Any ActivitySource or Meter named `ECommBackend.*` is picked up automatically. |
| `ECommBackend.AppHost` | The Aspire orchestrator. |
| `ECommBackend.Tests` | NUnit tests that reference the API project. |

### Layers inside the API

Everything is registered in `Program.cs`.

```
Controllers ──► Services (concrete classes) ──► Repositories (behind IXxxRepo interfaces) ──► SQLiteConn (EF Core)
     ▲                     │
     │                     └─ DTOs/MapToDomain extension methods: Model.ModelToRecordDTO()
 DTOs/FrontendDTO (request bodies: DTOProduct, DTOrder, DTOLogin, ...)
 DTOs/*DTO.cs     (response records: ProductDTO, OrderDTO, ...)
```

Some controllers call repositories directly, skipping the service layer (for example the `GET` actions in `OrdersController`).

### One HTTP request, start to finish

```
Client
  │  cookie: access_token=<JWT>     header: loggedInUserId=<guid> (some endpoints)
  ▼
GlobalExceptionLayer          catches anything unhandled → JSON 500 {message, inner, stackTrace(dev only)} + marks the trace span as an error
  ▼
OperationCancelledHandler     handles a client disconnect or cancellation
  ▼
Serilog request logging → HTTPS redirect → Routing
  ▼
Authentication                JwtBearer; OnMessageReceived reads the token from the `access_token` cookie, not the Authorization header
  ▼
Authorization                 [Authorize(Policy="adminUsers")] → role admin  |  [Authorize(Policy="users")] → role logged_in
  ▼
Controller                    parses ids (Guid.Parse / TryParse); some actions map domain exceptions to 400/404
  ▼
Service                       business rules, DTO mapping, telemetry
  ▼
Repository                    EF Core query; throws XxxNotFoundError when a row is missing
  ▼
SQLite
```

Login is `POST /api/users/auth/login` or `POST /api/admin/auth/login`. It sets the `access_token` cookie through `IJWTService.GenerateAccessToken(entity, Roles.x)`. `Roles` is defined in `Services/IJWTServices/IJWTService.cs`.

### Order lifecycle (spans both processes and both queues)

```
POST /api/orders  (DTOrder: product ids; owner comes from the loggedInUserId header, never the body)
  │
  ▼
OrderService.CreateOrder
  ├─ validate the ids and check every product exists     → 400 / 404
  ├─ total = sum of the cheapest in-stock variant per product
  ├─ save OrderModel(status = Pending)                   → 201 Created {orderId} returned to the client now
  └─ publish ONE OrderModel JSON message (each product carries only the variant priced above) ──► [Service Bus: userorderqueue]
                                                                              │
          ┌───────────────────────────────────────────────────────────────────┘
          ▼
AzureBackgroundService (hosted inside the API process)
  ├─ unreadable JSON or no products          → dead-letter
  ├─ order is Cancelled                      → complete, logged as skipped; stock untouched
  ├─ order is otherwise not Pending          → complete (treated as a duplicate)
  ├─ OrderRepo.TryFinalizeOrder              ONE transaction: stock ExecuteUpdates (inside a savepoint) + the order's status save
  │      no in-stock variant / stock short   → Rejected ("no_variant_in_stock" / "insufficient_stock")
  │      stock reserved                      → Processed
  │      lost the race to a cancel           → rolled back (stock restored), treated as Cancelled
  └─ OrderService.OnOrderFinalized
        ├─ OrderTelemetry.RecordFinalized (outcome + time from creation to final status)
        └─ publish OrderEmailMessage ──► [Service Bus: orderemailqueue]

POST /api/orders/{orderId}/cancel  ([Authorize(Policy="users")]; owner from the JWT NameIdentifier claim)
  └─ OrderRepo.CancelOrder: Pending + owned → Cancelled (200, OrderSummaryDTO) | missing/not owned → 404 | not Pending → 409
     then OnOrderFinalized("cancelled") → telemetry + cancellation email
                                                │
          ┌─────────────────────────────────────┘
          ▼
ConsumerProject / ConsumerBackground
  ├─ bad payload or no recipient → dead-letter
  ├─ EmailSender (MailKit SMTP; Mailpit locally)
  └─ SMTP failure → abandon (redelivered, MaxDeliveryCount = 5)
```

- Both sides create their queues on demand. Queue names and options are constants and factory methods on `AzureServiceBusClass`, and the consumer keeps its own copy of the email queue name.
- `OrderModel.Version` is an EF concurrency token. SQLite has no `rowversion`, so **every write that changes an order's status must set `Version = Guid.NewGuid()`**. That's what lets exactly one of a cancellation and the processor win. `ExecuteUpdateAsync` bypasses the token, so change order status only through a tracked entity and `SaveChangesAsync`.
- `OrderStatus` is stored and serialized as an int. Append new values at the end, and mirror them in `ConsumerProject/Models/OrderEmailMessage.cs`.
- `OrderEmailMessage` exists in **both** projects (`DTOs/OrderEmailMessage.cs` and `ConsumerProject/Models/OrderEmailMessage.cs`). A change to one must be made to the other.
- `PendingOrdersGaugeService` polls `CountPendingOrders` to feed a gauge in `OrderTelemetry`. Order metrics and spans use `OrderTelemetry.Source`. Telemetry for logins lives in `AuthTelemetry`, and for email in `ConsumerProject/Services/EmailTelemetry`.

## Conventions

### Style of the existing code

Match these when you edit existing code:

- **Async and cancellation:** every repository and service method is `async` and takes a `CancellationToken` named `ctx`. Controllers receive it as an action parameter and pass it down. In `UserService` and `UserRepo`, `ctx` comes *first* (`GetSingleUser(ctx, userId)`); in other classes it comes last.
- **Naming:** method parameters often have a leading underscore (`_orderId`, `_userId`). Private fields are `_camelCase`. Request DTO classes are prefixed `DTO`, response records end in `DTO`, and models end in `Model`. Ids are `Guid`, and route or header ids arrive as strings and get parsed in the controller.
- **Models:** they use `[Required]`/`[Key]`/`[Range]` attributes, `required` members, and a `[SetsRequiredMembers]` constructor. Build them through the constructor, e.g. `new OrderModel(id, total, creatorId, DateTime.UtcNow, OrderStatus.Pending)`. Model tests check them with `Validator.TryValidateObject`.
- **Missing rows:** repositories throw the custom exceptions in `CustomErrors` (`ProductNotFoundError`, `VariantNotFoundError`, `UserNotFoundError`, and so on) rather than returning null. A controller that doesn't catch them lets them reach `GlobalExceptionLayer`, which returns a 500. Only `OrdersController.CreateOrder` maps them to 404 (and `ArgumentException` to 400); follow that pattern for new endpoints.
- **Messaging:** payloads are serialized with Newtonsoft `JsonConvert`, not System.Text.Json. Every publish wraps its send in `MessageTracing.StartPublish` + `Inject`, and every receive starts its span with `MessageTracing.StartProcess`. Workers resolve scoped services through `IServiceScopeFactory.CreateScope()`, one scope per message.
- **Stock changes** must use atomic, conditional `ExecuteUpdateAsync`, not load → modify → `SaveChanges`, because several messages can be processed at once.
- **Passwords** are hashed in `UserService`/`AdminService` through `PasswordHasher` (BCrypt), so no `UserModel` is ever built around a plaintext password.
- **Logging:** use Serilog message templates (`"Order {OrderId} rejected: {Reason}"`), never string interpolation.

### Git workflow

- Branch from `dev` as `ft/<topic>`, for example `ft/controllers` or `ft/CI-CD`. Changes are promoted through pull requests: `ft/*` → `dev` → `staging` → `main`. Don't commit directly to `dev`, `staging` or `main`.
- Start every commit message with `feat:` or `fix:`, followed by a short lowercase summary, e.g. `fix: added basic jwt implementation`.
- A pull request must pass CI: the Release build, all tests, and the `has-pending-model-changes` check. CI publishes Docker images only from `main` and from `v*` tags.
- `ECommBackend.db` is tracked in git. Don't commit changes a local run makes to it unless the change is intentional. `published_code/` and `TestResults/` are ignored.

### Adding an API endpoint

1. **DTOs:** put the request class in `DTOs/FrontendDTO` (`DTOXxx`) and the response record in `DTOs/XxxDTO.cs`. Add a mapping extension to `DTOs/MapToDomain`. Return DTOs from actions, never EF models; the entities' navigation properties form cycles and expose data that shouldn't be returned.
2. **Repository:** add the method to the interface in `Repositories/RepoInterfaces` and implement it. It should be `async`, take `ctx`, and throw an `XxxNotFoundError` when a row is missing. Use `AsNoTracking()` for read-only queries, and filter in the query instead of calling `ToListAsync()` first.
3. **Service:** put business rules and mapping here. New controller actions call the service, not the repository.
4. **Controller:**
   - Add `[Authorize(Policy = "adminUsers" | "users")]` to the class or action. Make an endpoint public only with an explicit `[AllowAnonymous]`.
   - Read the caller's id from the token, `User.FindFirstValue(ClaimTypes.NameIdentifier)`, not from the `loggedInUserId` header.
   - Parse ids with `Guid.TryParse` and return `BadRequest` when parsing fails.
   - Catch domain exceptions and turn them into `NotFound` or `BadRequest`, the way `OrdersController.CreateOrder` does.
   - Give each action a route that no other action in the controller uses.
5. **DI:** register new repositories and services as `AddScoped` in `Program.cs`. Service Bus clients and the JWT service are singletons.

### Adding or changing an entity

- Put the model in `Models/` with data-annotation attributes, `required` members and a `[SetsRequiredMembers]` constructor. If other code depends on its shape, add an interface in `Models/ModInterfaces`.
- Add a `DbSet` to `SQLiteConn`, and put indexes, uniqueness and relationships in `OnModelCreating`.
- Add a migration with a PascalCase name that says what it does (`AddProductRatings`). Commit it together with the updated `SQLiteConnModelSnapshot.cs`. Never edit a migration that has already been merged; add a new one.
- Add a `<Model>Tests.cs` fixture that tests validation, following the pattern of `VariantModelTests.cs`.

### Messaging

- Declare each queue's name and options as a constant and a factory method on `AzureServiceBusClass`. Create queues with `QueueExistsAsync` → `CreateQueueAsync` on both the sending and the receiving side.
- Define each message contract as a record. If `ConsumerProject` reads the message, put an identical copy there; that project doesn't reference the API.
- Derive `MessageId` from the data (for example `$"{orderId}-{status}"`) so a re-sent message can be recognized. Handlers must be idempotent: check the current state in the database before acting.
- Settle every message explicitly:
  - Dead-letter a payload that can never succeed (bad JSON, missing data), with a reason.
  - Abandon after a temporary failure, so the message is redelivered up to `MaxDeliveryCount` times.
  - Complete after success or a deliberate rejection.

### Telemetry, configuration, packages

- **Telemetry:** for a new area, add a static `XxxTelemetry` class with `Name = "ECommBackend.<Area>"` and the `ActivitySource`/`Meter` built from it, as `OrderTelemetry` and `AuthTelemetry` do. The `ECommBackend.*` prefix is what makes ServiceDefaults pick it up, so no other registration is needed. Use `snake_case` for tag names (`order.rejection_reason`).
- **Configuration:** make a settings class with `[Required]` properties and bind it with `AddOptions<T>().Bind(section).ValidateDataAnnotations().ValidateOnStart()`. Keep secrets such as connection strings and real keys in user secrets or environment variables, never in `appsettings*.json`.
- **Packages:** add the version to `Directory.Packages.props` and leave it out of the `.csproj`. Add test-only packages (NUnit, Moq, test SDK) to `ECommBackend.Tests` only.

### Tests

- Write NUnit tests: an `internal` class marked `[TestFixture]`, with `[Test]` methods named `Member_ShouldExpectedBehavior` (e.g. `Constructor_ShouldSetVariantId`). Use a private `CreateValidXxx()` factory method for test data.
- Put repository tests in `ECommBackend.Tests/RepositoryTests/`. Run them against an in-memory SQLite database (`Data Source=:memory:` with the connection kept open) rather than `ECommBackend.db`.

## Tests

- Most coverage is model-validation tests (`*ModelTests.cs`, `ModelMockTests.cs`).
- `RepositoryTests/OrderRepoTest.cs` covers cancellation and finalization, including both orders of the cancel/processor race. It uses a shared-cache in-memory SQLite database so that two contexts see the same rows. The other `RepositoryTests/*` files are empty stubs, and `OrderMockTests.cs` is fully commented out.
- There are no integration tests for controllers or the message flow.
- Moq and NUnit are referenced by the API project itself as well as by the test project.

## Known issues

These are in the current code. Fix them deliberately and don't copy the patterns.

- `OrderRepo.GetAllOrders` ignores `_userId` and returns every order.
- `ProductsController` has no `[Authorize]`. In `OrdersController`, only the cancel action is protected; the other actions still trust the `loggedInUserId` header instead of the JWT claims.
- The product owner's foreign key is the shadow column `OwnerUserId`. `ProductModel.AdminOwnerId` isn't mapped to the relationship, so set the `Owner` navigation when you create a product.
- `OrderMapToDomain.ModelToRecordDTO` dereferences `OrderCreator`, which isn't loaded, and `UserDTO` maps back into orders. Return `OrderSummaryDTO` (`ModelToSummaryDTO()`) for orders until that's fixed.
- `OrdersController.cs` is saved in Windows-1252, not UTF-8, because of an em dash in a comment. Edit it with a byte-preserving tool or convert it.

## Code review

`.github/agents/senior-swe-review.agent.md` defines the review style this repo expects. Review findings for correctness, security, data integrity and reliability before style. Each finding gets a severity (Blocker/High/Medium/Low), a location, the issue, its impact and a recommendation. Start with a verdict (Approve / Approve with comments / Request changes). Back performance claims with evidence, or say what should be measured.
