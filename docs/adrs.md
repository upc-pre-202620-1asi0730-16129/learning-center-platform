# Architecture Decision Records (ADRs)

This document records the key architectural decisions made for the **Learning Center Platform**, detailing the context, options considered, decisions, and consequences.

---

## ADR Index

1. [ADR-001: Adoption of Domain-Driven Design (DDD) and Tactical Patterns](#adr-001-adoption-of-domain-driven-design-ddd-and-tactical-patterns)
2. [ADR-002: Command Query Responsibility Segregation (CQRS) with Cortex Mediator](#adr-002-command-query-responsibility-segregation-cqrs-with-cortex-mediator)
3. [ADR-003: Modernized Functional Result Pattern and Discriminated Error Model](#adr-003-modernized-functional-result-pattern-and-discriminated-error-model)
4. [ADR-004: Strongly-Typed Identifiers with Zero-Allocation Record Structs](#adr-004-strongly-typed-identifiers-with-zero-allocation-record-structs)
5. [ADR-005: Query Optimization with Non-Tracking Reads and Eager Projections](#adr-005-query-optimization-with-non-tracking-reads-and-eager-projections)
6. [ADR-006: In-Memory Domain Event Accumulation and Interceptor-Based Transactional Dispatch](#adr-006-in-memory-domain-event-accumulation-and-interceptor-based-transactional-dispatch)
7. [ADR-007: Migration from Swashbuckle to Scalar API Reference and Built-in ASP.NET Core OpenAPI](#adr-007-migration-from-swashbuckle-to-scalar-api-reference-and-built-in-aspnet-core-openapi)
8. [ADR-008: RESTful IAM Design for User Creation and Token Authentication](#adr-008-restful-iam-design-for-user-creation-and-token-authentication)
9. [ADR-009: Built-in ASP.NET Core IExceptionHandler and RFC 9457 Problem Details](#adr-009-built-in-aspnet-core-iexceptionhandler-and-rfc-9457-problem-details)

---

## ADR-001: Adoption of Domain-Driven Design (DDD) and Tactical Patterns

- **Status:** Accepted
- **Deciders:** Architecture Team
- **Date:** 2026-10-25

### Context
The platform manages complex business domains (Publishing, Profiles, IAM) with rich invariant rules, relationships, and cross-context boundaries. Traditional monolithic architectures or flat CRUD models lead to anemic domain models, fragmented business rules, and tightly coupled modules.

### Decision
Structure the platform into explicit **Bounded Contexts** (`Publishing`, `Profiles`, `Iam`, `Shared`) following DDD principles:
- **Aggregates & Aggregate Roots:** Enforce transactional boundaries and encapsulate domain invariants (`Tutorial`, `Category`, `User`, `Profile`).
- **Value Objects:** Encapsulate immutable domain concepts (`PersonName`, `EmailAddress`, `StreetAddress`, `EPublishingStatus`).
- **Anti-Corruption Layers (ACL):** Context facades (`IIamContextFacade`, `IProfilesContextFacade`) prevent domain leakage across bounded contexts.

### Consequences
- **Positive:** Clear domain boundaries, testable business invariants, and modular maintainability.
- **Negative:** Increased initial scaffolding and mapping between REST resources, commands, and aggregates.

---

## ADR-002: Command Query Responsibility Segregation (CQRS) with Cortex Mediator

- **Status:** Accepted
- **Deciders:** Architecture Team
- **Date:** 2026-10-25

### Context
Mixing read and write operations inside monolithic service classes leads to boilerplate exception handling, manual transaction commits, and poor optimization for read-heavy workloads.

### Decision
Adopt CQRS across all bounded contexts:
- Split application services into distinct **Command Services** (e.g. `TutorialCommandService`) and **Query Services** (e.g. `TutorialQueryService`).
- Implement cross-cutting pipeline behaviors via `Cortex.Mediator`:
  - `LoggingCommandBehavior`: Structured command tracing.
  - `TransactionCommandBehavior`: Automated unit of work completion (`IUnitOfWork.CompleteAsync`).
  - `ExceptionHandlingCommandBehavior`: Centralized database concurrency and transient exception mapping.

### Consequences
- **Positive:** Eliminates repetitive `try/catch` and `unitOfWork.CompleteAsync()` blocks across command handlers; keeps query operations fast and side-effect free.
- **Negative:** Requires defining dedicated command and query records for each business operation.

---

## ADR-003: Modernized Functional Result Pattern and Discriminated Error Model

- **Status:** Accepted
- **Deciders:** Architecture Team
- **Date:** 2026-10-25

### Context
Throwing domain exceptions for control flow causes performance penalties and unhandled runtime crashes. Earlier versions used untyped enums or generic `Result` classes lacking error differentiation.

### Decision
Implement a strongly-typed functional `Result<T>` and `Error` model in `Shared.Domain.Model`:
- `Error` record with `ErrorType` classification (`Validation`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `Failure`).
- `Result<T>` supporting functional `.Match(onSuccess, onFailure)` transformations in controllers and action assemblers.

### Consequences
- **Positive:** Type-safe error handling without runtime exceptions for business rule violations; direct mapping to standard HTTP status codes.
- **Negative:** Callers must explicitly unpack results using `.Match` or pattern matching.

---

## ADR-004: Strongly-Typed Identifiers with Zero-Allocation Record Structs

- **Status:** Accepted
- **Deciders:** Architecture Team
- **Date:** 2026-10-25

### Context
Using primitive `int` identifiers across entity models causes primitive obsession, risking accidental parameter swapping (e.g. passing a `CategoryId` into a `TutorialId` parameter).

### Decision
Introduce strongly-typed identifier types implementing `IEntityId<int>` as `readonly record struct`:
- Types: `UserId`, `ProfileId`, `TutorialId`, `CategoryId`, `AssetId`.
- Seamless EF Core mapping via `StronglyTypedIdConverter<TId, int>`.
- REST controllers and assemblers continue exposing integer primitives in external JSON contracts for 100% backward compatibility.

### Consequences
- **Positive:** Compile-time type safety preventing ID mix-ups; zero GC allocation overhead (stack-allocated structs); clean EF Core relational persistence.
- **Negative:** Requires value converter registration in `ModelBuilderExtensions`.

---

## ADR-005: Query Optimization with Non-Tracking Reads and Eager Projections

- **Status:** Accepted
- **Deciders:** Architecture Team
- **Date:** 2026-10-25

### Context
By default, EF Core tracks all loaded entities in memory, which adds CPU and memory overhead during read-only catalogue and search operations.

### Decision
- Extend `IBaseRepository<TEntity>` with `ListAsNoTrackingAsync` and `FindByIdAsNoTrackingAsync`.
- Execute all read queries (`UserQueryService`, `ProfileQueryService`, `TutorialQueryService`, `CategoryQueryService`) with `.AsNoTracking()`.
- Utilize `.AsSplitQuery()` and explicit `.Include()` eager loading for multi-collection aggregate reads (`Tutorial.Assets`, `Tutorial.Category`).

### Consequences
- **Positive:** Significant reduction in memory allocations, eliminated change tracker overhead on read queries, prevented Cartesian explosion.
- **Negative:** Entities loaded via read queries cannot be modified and committed directly without re-attaching.

---

## ADR-006: In-Memory Domain Event Accumulation and Interceptor-Based Transactional Dispatch

- **Status:** Accepted
- **Deciders:** Architecture Team
- **Date:** 2026-10-25

### Context
Cross-context workflows (such as creating a profile or sending notifications when a user registers or a tutorial is published) should not create hard synchronous couplings between command services.

### Decision
- Implement `IAggregateRoot` and `AggregateRoot` base class to record domain events (`IEvent`) in memory (`AddDomainEvent`).
- Implement `DispatchDomainEventsInterceptor` (EF Core `SaveChangesInterceptor`) that intercepts `SavedChangesAsync` and publishes recorded domain events via `Cortex.Mediator` after persistence succeeds.
- Handlers (`UserRegisteredEventHandler`, `CategoryCreatedEventHandler`, `TutorialCreatedEventHandler`) process events asynchronously.

### Consequences
- **Positive:** Bounded contexts remain completely decoupled; domain events are only dispatched if database persistence succeeds.
- **Negative:** Events are dispatched in-process; high-volume distributed scenarios may eventually require an Outbox table.

---

## ADR-007: Migration from Swashbuckle to Scalar API Reference and Built-in ASP.NET Core OpenAPI

- **Status:** Accepted
- **Deciders:** Architecture Team
- **Date:** 2026-10-25

### Context
Swashbuckle is deprecated in .NET 9/.NET 10. `Scalar.AspNetCore` and Microsoft's native `Microsoft.AspNetCore.OpenApi` provide a modern, interactive, and high-performance API documentation UI.

### Decision
- Replace `Swashbuckle.AspNetCore` with `Microsoft.AspNetCore.OpenApi` and `Scalar.AspNetCore`.
- Configure `app.MapOpenApi()` and `app.MapScalarApiReference()`.
- Replace Swashbuckle attributes with standard ASP.NET Core metadata (`[Tags]`, `[EndpointSummary]`, `[EndpointDescription]`, `[ProducesResponseType]`).
- Implement `BearerSecuritySchemeTransformer` (`IOpenApiDocumentTransformer`) to automatically declare JWT Bearer authentication in the Scalar interface (`/scalar/v1`).

### Consequences
- **Positive:** Native framework integration, zero external Swagger generator dependencies, interactive Bearer authorization UI in Scalar.
- **Negative:** Endpoint metadata configuration relies on ASP.NET Core endpoint filters and transformers rather than Swagger annotations.

---

## ADR-008: RESTful IAM Design for User Creation and Token Authentication

- **Status:** Accepted
- **Deciders:** Architecture Team
- **Date:** 2026-10-25

### Context
The previous IAM implementation used RPC-style verb routes (`POST /api/v1/authentication/sign-up` and `POST /api/v1/authentication/sign-in`), which diverged from REST resource standards and OpenAPI conventions.

### Decision
- Re-architect user registration into `UsersController` (`POST /api/v1/users`), returning `201 Created`, a `Location` header, and `UserResource`.
- Introduce `TokensController` (`POST /api/v1/tokens`) treating authentication as the creation of a security token resource, returning `200 OK` with `AuthenticatedUserResource`.
- Mark legacy `AuthenticationController` endpoints as `[Obsolete]` for backward compatibility.

### Consequences
- **Positive:** Strict adherence to REST principles, predictable HTTP status codes, seamless integration with API gateways and OAuth2/token clients.
- **Negative:** Existing client apps must plan migration from `/api/v1/authentication/*` to `/api/v1/users` and `/api/v1/tokens`.

---

## ADR-009: Built-in ASP.NET Core IExceptionHandler and RFC 9457 Problem Details

- **Status:** Accepted
- **Deciders:** Architecture Team
- **Date:** 2026-10-25

### Context
Custom middleware (`GlobalExceptionHandlerMiddleware`) lacked native telemetry integration and required manual JSON serialization for unhandled errors.

### Decision
- Implement `GlobalExceptionHandler` adhering to ASP.NET Core's standard `IExceptionHandler` interface.
- Register via `builder.Services.AddExceptionHandler<GlobalExceptionHandler>()` and `app.UseExceptionHandler()`.
- Format unhandled exceptions and operation cancellations into RFC 9457 compliant `ProblemDetails` responses.

### Consequences
- **Positive:** Standardized error contracts across all endpoints, native telemetry integration, centralized cancellation handling.
- **Negative:** Deprecates custom middleware invocation methods.
