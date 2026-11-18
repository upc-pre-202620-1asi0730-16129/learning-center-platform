# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [1.2.0] - 2026-10-29

### Added
- **Study Guide Documentation:**
  - Added comprehensive study guide specification for codebase analysis in `docs/`.
  - Expanded study guide with detailed prompts and structure outlining architecture, patterns, and practices.
- **Domain-Driven Design (DDD) — AggregateRoot:**
  - Added `AggregateRoot` base class for managing in-memory domain event lifecycle (`IEvent`) across aggregate roots.

### Changed
- **Refactoring:**
  - Renamed `ErrorType` to `EErrorType` for consistency with enumeration naming conventions.
  - Moved `IEntityId` interface to `ValueObjects` namespace for improved domain model organization.
  - Renamed `Aggregate` to `Aggregates` for consistency across domain models.
  - Updated `DomainErrorModel` alias for improved clarity and consistency.
  - Reorganized using directives across multiple files for improved code clarity and structure.

### Fixed
- Nothing yet.

### Deprecated
- Nothing yet.

### Removed
- **Domain Model Simplification:**
  - Removed initial `AggregateRoot` class implementation prior to introducing the finalized version with proper domain event management.

### Security
- Nothing yet.

---

## [1.1.0] - 2026-10-26

### Added
- **OpenAPI & Scalar Reference:**
  - Migrated API documentation to `Microsoft.AspNetCore.OpenApi` and `Scalar.AspNetCore`.
  - Added `BearerSecuritySchemeTransformer` to enable interactive JWT Bearer authorization in the Scalar documentation UI (`/scalar/v1`).
- **RESTful IAM Endpoints:**
  - Added `POST /api/v1/users` in `UsersController` for RESTful user registration returning `201 Created` with `Location` header and `UserResource`.
  - Added `POST /api/v1/tokens` in `TokensController` for credential-based JWT Bearer token generation returning `200 OK` with `AuthenticatedUserResource`.
- **Domain-Driven Design (DDD) & Strongly-Typed IDs:**
  - Introduced zero-allocation strongly-typed identifier record structs (`UserId`, `ProfileId`, `TutorialId`, `CategoryId`, `AssetId`) implementing `IEntityId<int>`.
  - Added reusable `StronglyTypedIdConverter<TId, TValue>` for transparent Entity Framework Core persistence mapping.
  - Added `IAggregateRoot` and `AggregateRoot` base class managing in-memory domain event lifecycle (`IEvent`).
  - Added domain events: `UserRegisteredEvent`, `ProfileCreatedEvent`, `TutorialCreatedEvent`, `TutorialPublishedEvent`, and `CategoryCreatedEvent`.
  - Added `DispatchDomainEventsInterceptor` to automatically publish domain events via `Cortex.Mediator` upon successful EF Core transaction commit.
- **CQRS & Mediator Pipeline Behaviors:**
  - Added `TransactionCommandBehavior` to automatically handle `IUnitOfWork.CompleteAsync()` upon successful command execution.
  - Added `ExceptionHandlingCommandBehavior` for centralized database and concurrency exception translation.
  - Added `LoggingCommandBehavior` for structured tracing of command execution and latency.
- **Modernized Result & Error Pattern:**
  - Added discriminated `Error` model with `ErrorType` classification (`Validation`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `Failure`).
  - Modernized `Result<T>` with functional `.Match()` pattern handling for clean controller action results.
- **ASP.NET Core Built-in Exception Handling:**
  - Added `GlobalExceptionHandler` implementing `IExceptionHandler` with RFC 9457 compliant `ProblemDetails` output.
- **Architecture Documentation:**
  - Added Architecture Decision Records in `docs/adrs.md`.
  - Added `CONTRIBUTING.md` engineering guidelines.
  - Synchronized `docs/user-stories.md` and `docs/class-diagram.puml` with the latest architecture.

### Changed
- Refactored `UserCommandService`, `ProfileCommandService`, `TutorialCommandService`, and `CategoryCommandService` to eliminate redundant `try/catch` and manual `CompleteAsync()` boilerplate by leveraging Cortex mediator pipeline behaviors.
- Optimized query execution across `UserQueryService`, `ProfileQueryService`, `TutorialQueryService`, and `CategoryQueryService` with `.AsNoTracking()` and split query configuration (`.AsSplitQuery()`).
- Updated `RequestAuthorizationMiddleware` to safely handle optional and anonymous authentication flows without throwing unhandled exceptions.

### Deprecated
- `AuthenticationController` (`POST /api/v1/authentication/sign-in`, `POST /api/v1/authentication/sign-up`) marked `[Obsolete]` in favor of `TokensController` (`POST /api/v1/tokens`) and `UsersController` (`POST /api/v1/users`).
- `GlobalExceptionHandlerMiddleware` and `UseGlobalExceptionHandler()` marked `[Obsolete]` in favor of ASP.NET Core `IExceptionHandler` and `app.UseExceptionHandler()`.

---

## [1.0.0] - 2026-10-25

### Added
- Initial release of Learning Center Platform with ASP.NET Core (.NET 10).
- Core bounded contexts: IAM, Profiles, Publishing, and Shared kernel.
- MySQL database persistence with Entity Framework Core and automatic schema migration.
- OpenAPI specification with Swashbuckle and initial RESTful controllers.
