# CLAUDE.md — ECommerce Platform

> This file provides Claude with full context about the project architecture, conventions, and decisions.
> Place this file in the **root of the solution** (`/ECommerce.slnx` level) so it is always visible.

---

## 📌 Project Overview

**Name:** ECommerce Platform
**Type:** Distributed microservices e-commerce system
**Language:** C# / .NET 10
**Architecture:** Vertical Slice Architecture — each feature is a self-contained slice (Command/Query/Handler/Endpoint/Validator in one folder)
**Deployment:** Separate Docker containers per service, orchestrated via Docker Compose (local) and Kubernetes (production)
**API Gateway:** YARP (Yet Another Reverse Proxy)

---

## 🛠️ Common Commands

```bash
# Build the entire solution
dotnet build ECommerce.slnx --source https://api.nuget.org/v3/index.json

# Run all tests
dotnet test ECommerce.slnx

# Run tests for a single service
dotnet test tests/ProductService/ECommerce.ProductService.Tests/

# Run a single test by name
dotnet test --filter "FullyQualifiedName~CreateProductHandlerTests"

# Add a package (always specify source to avoid private-feed 401 errors)
dotnet add <project.csproj> package <PackageName> --source https://api.nuget.org/v3/index.json

# Run a single service locally (against dockerised infra)
dotnet run --project src/Services/ProductService/ECommerce.ProductService/
```

### Docker dev stack

```bash
# Start infra only (MongoDB, Postgres, Redis, Kafka, Elasticsearch)
# → recommended for local development; run services from IDE
docker compose up -d

# Start infra + all .NET microservices + YARP gateway
docker compose --profile services up -d

# Add dev tools (Kafka UI :8090, Mongo Express :8091)
docker compose --profile tools up -d

# Full everything
docker compose --profile all up -d

# Build/rebuild a single service image
docker compose --profile services build product-service

# Tail logs for one service
docker compose logs -f product-service

# Stop and remove containers (keeps volumes)
docker compose --profile all down

# Full reset including volumes
docker compose --profile all down -v
```

> **NuGet note:** The machine has a private DevExpress NuGet source that returns 401 for public packages.
> Always add `--source https://api.nuget.org/v3/index.json` when running `dotnet add package`.

---

## 🏗️ Solution Structure

```
ECommerce.slnx                         ← solution root, CLAUDE.md lives here
│
├── src/
│   ├── Shared/
│   │   ├── ECommerce.SharedKernel/    ← base primitives: Entity, AggregateRoot, Result<T>, ICommand, IQuery, IEndpoint
│   │   └── ECommerce.Contracts/       ← Kafka integration events shared across services
│   │
│   ├── Services/
│   │   ├── ProductService/
│   │   │   └── ECommerce.ProductService/
│   │   ├── OrderService/
│   │   │   └── ECommerce.OrderService/
│   │   ├── CustomerService/
│   │   │   └── ECommerce.CustomerService/
│   │   ├── CartService/
│   │   │   └── ECommerce.CartService/
│   │   ├── PaymentService/
│   │   │   └── ECommerce.PaymentService/
│   │   ├── SearchService/
│   │   │   └── ECommerce.SearchService/
│   │   ├── NotificationService/
│   │   │   └── ECommerce.NotificationService/
│   │   └── InventoryService/
│   │       └── ECommerce.InventoryService/
│   │
│   └── Gateway/
│       └── ECommerce.Gateway/         ← YARP reverse proxy
│
├── tests/                             ← mirrors src/Services structure
├── docker-compose.yml
├── docker-compose.override.yml        ← local dev ports, volumes
└── CLAUDE.md                          ← THIS FILE
```

---

## ✂️ Vertical Slice Convention

Every feature lives in `Features/{FeatureName}/` and contains ALL related files:

```
Features/
└── CreateProduct/
    ├── CreateProductCommand.cs       ← ICommand<TResult>
    ├── CreateProductHandler.cs       ← ICommandHandler<TCommand, TResult>
    ├── CreateProductEndpoint.cs      ← IEndpoint (Minimal API)
    ├── CreateProductRequest.cs       ← HTTP request DTO
    ├── CreateProductValidator.cs     ← FluentValidation
    └── CreateProductResponse.cs      ← HTTP response DTO (optional)
```

**Rules:**
- One folder = one feature = one vertical slice
- Handlers must return `Result<T>` — never throw business exceptions
- Validators are always registered via `AddValidatorsFromAssembly`
- Endpoints are auto-discovered via `IEndpoint` + `AddEndpoints(Assembly)`, Use minimal APIs with `.MapPost()`, `.MapGet()`with separated class for endpoints
- No service-to-service direct HTTP calls inside handlers — use Kafka events or the `IEventBus` abstraction

---

## 📦 SharedKernel — Key Types

```csharp
// CQRS (custom, no MediatR)
ICommand<TResult>
IQuery<TResult>
ICommandHandler<TCommand, TResult>   // HandleAsync(command, ct)
IQueryHandler<TQuery, TResult>       // HandleAsync(query, ct)
ICommandDispatcher                   // SendAsync<TResult>(command, ct)
IQueryDispatcher                     // QueryAsync<TResult>(query, ct)
Dispatcher                           // concrete implementation, register as singleton

// Result pattern (no exceptions for business logic)
Result<T>               // Success(value) or Failure(error)
Error                   // Code + Message; Error.NotFound("X"), Error.New("CODE","msg")

// Domain
Entity<TId>             // Id, domain events list (RaiseDomainEvent / ClearDomainEvents)
AggregateRoot<TId>      // extends Entity<TId>
IDomainEvent            // marker interface

// Messaging
IIntegrationEvent       // Guid Id, DateTimeOffset OccurredAt
IEventBus               // PublishAsync<T>(T event, CancellationToken)

// Endpoint discovery
IEndpoint               // void MapEndpoint(IEndpointRouteBuilder app)
EndpointExtensions      // AddEndpoints(Assembly) + MapEndpoints(WebApplication)
```

**CQRS dispatcher registration pattern** (in each service's ServiceExtensions or Program.cs):
```csharp
services.AddSingleton<Dispatcher>();
services.AddSingleton<ICommandDispatcher>(sp => sp.GetRequiredService<Dispatcher>());
services.AddSingleton<IQueryDispatcher>(sp => sp.GetRequiredService<Dispatcher>());

// Register each handler explicitly (scoped for repo access):
services.AddScoped<ICommandHandler<CreateProductCommand, Guid>, CreateProductHandler>();
```

**ValidationBehavior** lives in SharedKernel (`CQRS/ValidationBehavior.cs`) and wraps any handler with FluentValidation before execution. Use it as a decorator when needed.

---

## 🗄️ Database Strategy

| Service            | Database      | ORM / Driver          | Notes                                      |
|--------------------|---------------|-----------------------|--------------------------------------------|
| ProductService     | MongoDB       | MongoDB.Driver        | Dynamic attributes, nested variants        |
| OrderService       | PostgreSQL    | EF Core               | ACID transactions, order lifecycle         |
| CustomerService    | PostgreSQL    | EF Core + ASP Identity| Auth, addresses, segmentation              |
| CartService        | Redis         | StackExchange.Redis   | TTL-based, ephemeral sessions              |
| SearchService      | Elasticsearch | Elastic.Clients.Elasticsearch | Facets, full-text, autocomplete  |
| PaymentService     | PostgreSQL    | EF Core               | Idempotency keys, payment intents          |
| NotificationService| —             | Stateless             | Consumes Kafka, sends via SendGrid/Twilio  |
| InventoryService   | PostgreSQL    | EF Core               | Stock levels, reservations                 |

---

## 📨 Kafka — Event Bus

**Bootstrap servers config key:** `Kafka__BootstrapServers`
**Library:** Confluent.Kafka
**Pattern:** Outbox pattern for guaranteed delivery (Polly + DB outbox table)

### Topics & Consumers

| Topic                    | Producer           | Consumers                                  |
|--------------------------|--------------------|--------------------------------------------|
| `product.created`        | ProductService     | SearchService (index), InventoryService    |
| `product.updated`        | ProductService     | SearchService (reindex), CartService (price sync) |
| `order.placed`           | OrderService       | PaymentService, InventoryService, NotificationService |
| `order.paid`             | PaymentService     | OrderService, NotificationService, InventoryService |
| `order.shipped`          | OrderService       | NotificationService                        |
| `order.cancelled`        | OrderService       | InventoryService (restock), PaymentService (refund) |
| `cart.abandoned`         | CartService        | NotificationService                        |
| `user.registered`        | CustomerService    | NotificationService                        |
| `inventory.low`          | InventoryService   | NotificationService, SellerService         |
| `payment.failed`         | PaymentService     | NotificationService, OrderService          |

### Integration Event Convention

All events live in `ECommerce.Contracts/{ServiceName}/`:
```csharp
// Always use records, always include OccurredAt
public record ProductCreatedEvent(
    string ProductId,
    string Name,
    string CategoryId,
    decimal BasePrice,
    DateTimeOffset OccurredAt
) : IIntegrationEvent;
```

---

## 🔀 YARP Gateway

**Project:** `src/Gateway/ECommerce.Gateway`
**Port (local):** `5000`
**Config:** `appsettings.json` → `ReverseProxy` section

### Route Prefix Convention

| Prefix            | Routes to         | Internal address                  |
|-------------------|-------------------|-----------------------------------|
| `/api/products`   | ProductService    | `http://product-service:8080`     |
| `/api/orders`     | OrderService      | `http://order-service:8080`       |
| `/api/customers`  | CustomerService   | `http://customer-service:8080`    |
| `/api/cart`       | CartService       | `http://cart-service:8080`        |
| `/api/search`     | SearchService     | `http://search-service:8080`      |
| `/api/payments`   | PaymentService    | `http://payment-service:8080`     |
| `/api/inventory`  | InventoryService  | `http://inventory-service:8080`   |

Gateway is also responsible for:
- JWT validation (shared secret / JWKS from CustomerService)
- Rate limiting per route
- Request correlation ID injection (`X-Correlation-Id`)

---

## 🔐 Authentication & Authorization

- **CustomerService** issues JWT tokens (ASP.NET Core Identity + custom JWT)
- All other services validate JWT using shared `JwtBearer` config from SharedKernel
- Config key: `Jwt__Secret`, `Jwt__Issuer`, `Jwt__Audience`
- Roles: `buyer`, `seller`, `admin`
- Endpoints use `.RequireAuthorization("buyer")` / `.RequireAuthorization("seller")` etc.

---

## 🧰 NuGet Packages — Global Conventions

| Package                          | Usage                              |
|----------------------------------|------------------------------------|
| FluentValidation                 | Validators per slice               |
| Swashbuckle.AspNetCore           | Swagger UI (all services)          |
| Serilog                          | Structured logging → Elasticsearch |
| OpenTelemetry                    | Distributed tracing → Jaeger       |
| Polly                            | Retry, circuit breaker             |
| Confluent.Kafka                  | Kafka producer/consumer            |
| MongoDB.Driver                   | ProductService, ReviewService      |
| Microsoft.EntityFrameworkCore    | SQL services                       |
| StackExchange.Redis              | CartService                        |
| Elastic.Clients.Elasticsearch    | SearchService                      |
| Stripe.net                       | PaymentService                     |
| Yarp.ReverseProxy                | Gateway                            |

---

## 🐳 Local Dev — Docker Compose Ports

| Service              | Port    |
|----------------------|---------|
| Gateway (YARP)       | 5000    |
| ProductService       | 5001    |
| OrderService         | 5002    |
| CustomerService      | 5003    |
| CartService          | 5004    |
| SearchService        | 5005    |
| PaymentService       | 5006    |
| NotificationService  | 5007    |
| InventoryService     | 5008    |
| MongoDB              | 27017   |
| PostgreSQL           | 5432    |
| Redis                | 6379    |
| Kafka                | 9092    |
| Elasticsearch        | 9200    |
| Kafka UI (`--profile tools`)      | 8090  |
| Mongo Express (`--profile tools`) | 8091  |
| Seq — log UI (`--profile tools`)  | 5342  |
| Jaeger — trace UI (`--profile tools`) | 16686 |
| Jaeger — OTLP gRPC ingestion      | 4317  |
| Prometheus (`--profile tools`)    | 9090  |
| Grafana — metrics UI (`--profile tools`) | 3000 |

---

## 📋 Coding Conventions

- **Naming:** PascalCase for classes, camelCase for local vars, `_camelCase` for private fields
- **Async:** All handlers and endpoints are fully async, always accept `CancellationToken ct`
- **No exceptions for business logic** — use `Result<T>.Failure(Error.New("CODE", "message"))`
- **No static classes** except extension methods in `ServiceExtensions.cs`
- **No direct DB access in endpoints** — always go through custom Query/Handler for testability and separation of concerns
- **Validators always run** — register pipeline behavior `ValidationBehavior<,>` but not mediatr, use custom behavior that works with vertical slice handlers
- **Idempotency** — PaymentService and OrderService endpoints require `Idempotency-Key` header
- **Correlation IDs** — all logs must include `CorrelationId` from `X-Correlation-Id` header
- **UTC everywhere** — always use `DateTimeOffset.UtcNow`, never `DateTime.Now`
- **No magic strings** — use `const` or `static readonly` for topic names, policy names, claim types
- **Internal handlers** — handlers/endpoints are `internal sealed class`; add `<InternalsVisibleTo Include="ECommerce.{Service}.Tests" />` in the service `.csproj` for test access

---

## 🗺️ Build Order (Phases)

```
Phase 1 — Foundation
  SharedKernel → Contracts → Gateway (YARP)

Phase 2 — Core Commerce
  ProductService (MongoDB) → SearchService (ES sync via Kafka)
  CustomerService (auth/JWT) → CartService (Redis)

Phase 3 — Transactional
  OrderService (PostgreSQL) → PaymentService (Stripe) → InventoryService

Phase 4 — Async / Notifications
  NotificationService (Kafka consumer) → abandoned cart → email flows

Phase 5 — Growth Features
  PromotionsService → ReviewService → SellerService → LoyaltyService
```

---

## 🔍 Observability Stack

| Concern            | Tool                          | Config key              |
|--------------------|-------------------------------|-------------------------|
| Structured logging | Serilog → Elasticsearch       | `Serilog__*`            |
| Distributed tracing| OpenTelemetry → Jaeger        | `Jaeger__AgentHost`     |
| Metrics            | Prometheus + Grafana          | `/metrics` endpoint     |
| Log UI             | Kibana                        | `http://localhost:5601` |
| Trace UI           | Jaeger                        | `http://localhost:16686`|

Every service calls two methods from `ECommerce.SharedKernel.Observability`:
```csharp
// before Build()
builder.AddObservability("service-name");  // Serilog + OTel tracing + metrics

// after Build()
app.MapObservability();  // exposes /metrics for Prometheus
```

Config keys (defaults point to Docker service names; overridden in appsettings.Development.json to localhost):
| Key | Docker default | Local dev (IDE) |
|-----|---|---|
| `Seq:ServerUrl` | `http://seq:5341` | `http://localhost:5341` |
| `Otlp:Endpoint` | `http://jaeger:4317` | `http://localhost:4317` |

---

## ✅ Definition of Done — per slice

- [ ] Command/Query + Handler implemented
- [ ] FluentValidation validator added
- [ ] Endpoint mapped via `IEndpoint`
- [ ] Unit test for handler (mocked dependencies)
- [ ] Integration test for endpoint (WebApplicationFactory)
- [ ] Kafka event published (if state-changing operation)
- [ ] OpenAPI documented (`.WithName()`, `.WithSummary()`, `.Produces<>()`)
- [ ] Logs include `CorrelationId` and relevant entity IDs
