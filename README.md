# Learning Center Platform

[![Release](https://img.shields.io/badge/release-v1.1.0-blue.svg)](CHANGELOG.md)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-14.0-blue.svg)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE.md)

## Project Overview

The **Learning Center Platform** is a modern, modular, and scalable backend application designed to manage educational content, user profiles, and identity authentication. Built with **.NET 10** and **C# 14**, it adheres to **Domain-Driven Design (DDD)** tactical patterns and implements **Command Query Responsibility Segregation (CQRS)** with pipeline behaviors.

The platform is structured into decoupled **Bounded Contexts** (`Publishing`, `Profiles`, `IAM`, `Shared`), ensuring clear separation of concerns, high maintainability, and robust transactional consistency.

---

## Table of Contents

1. [Architecture Overview](#architecture-overview)
2. [Domain-Driven Design (DDD) Concepts](#domain-driven-design-ddd-concepts)
3. [Key Features & Best Practices](#key-features--best-practices)
4. [Bounded Contexts](#bounded-contexts)
   * [IAM (Identity & Access Management)](#iam-identity-and-access-management)
   * [Profiles](#profiles)
   * [Publishing](#publishing)
5. [Technologies Used](#technologies-used)
6. [Getting Started](#getting-started)
   * [Prerequisites](#prerequisites)
   * [Setup Instructions](#setup-instructions)
7. [Project Structure](#project-structure)
8. [Documentation](#documentation)
9. [Contributing & Changelog](#contributing--changelog)
10. [License](#license)

---

## Architecture Overview

The platform architecture follows **Domain-Driven Design (DDD)** and **Command Query Responsibility Segregation (CQRS)**:

* **Domain Layer**: Encapsulates core business entities, aggregates (`AggregateRoot`), value objects, domain events (`IEvent`), and repository contracts. Contains zero outward dependencies.
* **Application Layer**: Coordinates domain operations through explicit **Command Services** (write side) and **Query Services** (read side), mediated by `Cortex.Mediator` pipeline behaviors (logging, transaction unit-of-work commit, exception handling).
* **Infrastructure Layer**: Implements persistence via Entity Framework Core (MySQL), strongly-typed ID value converters, token services, and interceptor-based domain event dispatchers.
* **Interfaces Layer (Presentation)**: Exposes RESTful controllers, RFC 9457 `ProblemDetails` error responses, and resource assemblers.

---

## Domain-Driven Design (DDD) Concepts

* **Bounded Contexts**: Independent business domains (`IAM`, `Profiles`, `Publishing`, and `Shared`).
* **Aggregates & Invariant Protection**: Aggregates (`Tutorial`, `Category`, `User`, `Profile`) inherit from `AggregateRoot` and guard internal state consistency and collection access.
* **Strongly-Typed IDs**: Entities use stack-allocated, zero-allocation readonly record structs (`UserId`, `ProfileId`, `TutorialId`, `CategoryId`, `AssetId`) implementing `IEntityId<int>` to eliminate primitive obsession.
* **Domain Events**: Aggregates record domain events (`UserRegisteredEvent`, `TutorialCreatedEvent`, etc.) in memory, automatically dispatched via `DispatchDomainEventsInterceptor` and `Cortex.Mediator` upon successful transaction persistence.
* **Value Objects**: Immutable domain objects (`PersonName`, `EmailAddress`, `StreetAddress`, `EPublishingStatus`).
* **Anti-Corruption Layer (ACL)**: Facades (`IIamContextFacade`, `IProfilesContextFacade`) isolate cross-context communication.

---

## Key Features & Best Practices

* **CQRS with Mediator Pipeline Behaviors**:
  * `TransactionCommandBehavior`: Automatically commits changes via `IUnitOfWork.CompleteAsync()` on successful command execution.
  * `ExceptionHandlingCommandBehavior`: Centralizes database and concurrency exception mapping.
  * `LoggingCommandBehavior`: Structured execution tracing.
* **Query Performance Optimizations**:
  * Read services use `.AsNoTracking()` and `.AsSplitQuery()` to eliminate change-tracking overhead and prevent Cartesian joins.
* **Modernized Functional Result Pattern**:
  * Operations return `Result<T>` and strongly-typed `Error` records with `ErrorType` classification (`Validation`, `NotFound`, `Conflict`, `Unauthorized`, `Forbidden`, `Failure`), leveraging `.Match()` for clean controller responses.
* **Interactive Scalar OpenAPI Documentation**:
  * Built on `Microsoft.AspNetCore.OpenApi` and `Scalar.AspNetCore` at `/scalar/v1`, featuring automated JWT Bearer authorization support via `BearerSecuritySchemeTransformer`.
* **Standardized Error Handling**:
  * Integrated ASP.NET Core `IExceptionHandler` (`GlobalExceptionHandler`) emitting RFC 9457 `ProblemDetails`.
* **Internationalization (i18n)**:
  * Localized messages via `IStringLocalizer` and context-specific `.resx` resource files.

---

## Bounded Contexts

### IAM (Identity and Access Management)
* **Scope**: User registration, authentication, JWT token issuance, and password security.
* **Aggregates**: `User`
* **Strongly-Typed IDs**: `UserId`
* **Domain Events**: `UserRegisteredEvent`
* **RESTful Endpoints**:
  * `POST /api/v1/users` — User registration (returns `201 Created` with `UserResource`)
  * `POST /api/v1/tokens` — Token authentication (returns `200 OK` with `AuthenticatedUserResource`)
  * `GET /api/v1/users/{id}` — Fetch user by ID
  * `GET /api/v1/users` — List users

### Profiles
* **Scope**: Learner and instructor profile identities and address management.
* **Aggregates**: `Profile`
* **Strongly-Typed IDs**: `ProfileId`
* **Value Objects**: `PersonName`, `EmailAddress`, `StreetAddress`
* **Domain Events**: `ProfileCreatedEvent`
* **RESTful Endpoints**:
  * `POST /api/v1/profiles` — Create profile (`201 Created`)
  * `GET /api/v1/profiles/{id}` — Get profile by ID
  * `GET /api/v1/profiles` — List all profiles

### Publishing
* **Scope**: Educational tutorial authoring, category taxonomy, and multimedia asset management.
* **Aggregates & Entities**: `Tutorial`, `Category`, `Asset` (`VideoAsset`, `ImageAsset`, `ReadableContentAsset`)
* **Strongly-Typed IDs**: `TutorialId`, `CategoryId`, `AssetId`
* **Domain Events**: `TutorialCreatedEvent`, `TutorialPublishedEvent`, `CategoryCreatedEvent`
* **RESTful Endpoints**:
  * `POST /api/v1/tutorials` — Create tutorial (`201 Created`)
  * `GET /api/v1/tutorials/{id}` — Get tutorial by ID
  * `GET /api/v1/tutorials` — List tutorials
  * `POST /api/v1/tutorials/{id}/videos` — Attach video asset
  * `POST /api/v1/categories` — Create category (`201 Created`)
  * `GET /api/v1/categories/{id}` — Get category by ID
  * `GET /api/v1/categories` — List categories
  * `GET /api/v1/categories/{categoryId}/tutorials` — Tutorials by category

---

## Technologies Used

* **.NET 10** & **C# 14**: Core runtime and language.
* **ASP.NET Core**: Web API framework.
* **Entity Framework Core**: Relational database ORM (MySQL).
* **MySQL**: Primary database.
* **Cortex.Mediator**: In-process mediator and pipeline behaviors.
* **Scalar.AspNetCore** & **Microsoft.AspNetCore.OpenApi**: Interactive OpenAPI documentation.
* **BCrypt.Net-Next**: Password hashing.
* **System.IdentityModel.Tokens.Jwt**: JWT token generation and validation.
* **Microsoft.Extensions.Localization**: Multi-language localization support.

---

## Getting Started

### Prerequisites
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [Docker](https://www.docker.com/) (for MySQL container) or local MySQL instance
* JetBrains Rider or Visual Studio 2026 / VS Code

### Setup Instructions

1. **Clone the repository:**
   ```bash
   git clone https://github.com/upc-pre-202610-1asi0730-sandbox/learning-center-platform.git
   cd learning-center-platform
   ```

2. **Start MySQL database:**
   ```bash
   docker compose up -d
   ```

3. **Restore and build:**
   ```bash
   dotnet restore
   dotnet build
   ```

4. **Run the application:**
   ```bash
   dotnet run --project Acme.Center.Platform
   ```

5. **Explore Interactive API Reference:**
   * Scalar Documentation: [http://localhost:5000/scalar/v1](http://localhost:5000/scalar/v1)
   * OpenAPI Specification: [http://localhost:5000/openapi/v1.json](http://localhost:5000/openapi/v1.json)

---

## Project Structure

```
Acme.Center.Platform/
├── Iam/                     # IAM Bounded Context
│   ├── Application/         # Command/Query Services, ACL Facades
│   ├── Domain/              # User Aggregate, UserId, Events, Repositories
│   ├── Infrastructure/      # UserRepository, TokenService, Hashing
│   ├── Interfaces/Rest/     # UsersController, TokensController, Resources
│   └── Resources/           # Localization resource files
├── Profiles/                # Profiles Bounded Context
│   ├── Application/         # Profile Command/Query Services, Event Handlers
│   ├── Domain/              # Profile Aggregate, ProfileId, Value Objects
│   ├── Infrastructure/      # ProfileRepository
│   ├── Interfaces/Rest/     # ProfilesController, Resources
│   └── Resources/           # Localization resource files
├── Publishing/              # Publishing Bounded Context
│   ├── Application/         # Tutorial & Category Services, Event Handlers
│   ├── Domain/              # Tutorial, Category, Asset Aggregates, StronglyTyped IDs
│   ├── Infrastructure/      # TutorialRepository, CategoryRepository
│   ├── Interfaces/Rest/     # TutorialsController, CategoriesController
│   └── Resources/           # Localization resource files
├── Shared/                  # Shared Kernel & Infrastructure
│   ├── Application/         # Result<T>, Error models
│   ├── Domain/              # AggregateRoot, IEntityId, Base Repository interfaces
│   ├── Infrastructure/      # AppDbContext, Pipeline Behaviors, Interceptors
│   ├── Interfaces/          # GlobalExceptionHandler, ProblemDetailsFactory
│   └── Resources/           # Shared & Error localization files
├── Program.cs               # Startup configuration and DI registrations
└── appsettings.json         # Configuration settings
```

---

## Documentation

* **Architecture Decision Records (ADRs)**: [`docs/adrs.md`](docs/adrs.md)
* **Technical User Stories**: [`docs/user-stories.md`](docs/user-stories.md)
* **PlantUML Class Diagram**: [`docs/class-diagram.puml`](docs/class-diagram.puml)
* **Software Architecture DSL**: [`docs/software-architecture.dsl`](docs/software-architecture.dsl)

---

## Contributing & Changelog

* See [`CONTRIBUTING.md`](CONTRIBUTING.md) for contribution guidelines, commit standards, and workflow.
* See [`CHANGELOG.md`](CHANGELOG.md) for version release notes.

---

## License

This project is licensed under the **MIT License**. See [`LICENSE.md`](LICENSE.md) for details.
