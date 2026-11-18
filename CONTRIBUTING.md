# Contributing to Learning Center Platform

Thank you for contributing to the **Learning Center Platform**! We welcome improvements, bug fixes, and suggestions. Please review this guide to understand our engineering practices and workflow.

---

## 1. Development Principles

We adhere strictly to modern .NET development best practices:
- **C# 14 & .NET 10 Standards:** Primary constructors, collection expressions (`[.. items]`), file-scoped namespaces, pattern matching, and nullable reference types enabled.
- **Domain-Driven Design (DDD):**
  - Strict bounded contexts (`Iam`, `Profiles`, `Publishing`, `Shared`).
  - Entities and aggregate roots inherit from `AggregateRoot` and protect their domain invariants.
  - Strongly-typed identifier record structs (e.g. `UserId`, `TutorialId`) implementing `IEntityId<int>` to prevent primitive obsession.
- **CQRS (Command Query Responsibility Segregation):**
  - Clear separation between Commands (write side) and Queries (read side).
  - Cross-cutting concerns managed through Cortex Mediator behaviors (`LoggingCommandBehavior`, `TransactionCommandBehavior`, `ExceptionHandlingCommandBehavior`).
  - Read queries should use `.AsNoTracking()` and split query execution where appropriate.
- **Functional Result Pattern:**
  - Return `Result<T>` and strongly-typed `Error` records instead of throwing exceptions for expected business logic errors.

---

## 2. Conventional Commits

All commits must follow the [Conventional Commits](https://www.conventionalcommits.org/en/v1.0.0/) specification:

```
<type>(<scope>): <short description>

[optional body]

[optional footer(s)]
```

### Commit Types
- `feat`: A new feature or endpoint.
- `fix`: A bug fix.
- `refactor`: Code restructuring without changing external behavior or fixing bugs.
- `perf`: A code change that improves execution performance or reduces allocations.
- `test`: Adding missing tests or correcting existing tests.
- `docs`: Documentation-only changes.
- `chore`: Maintenance tasks, dependency updates, or configuration changes.

### Common Scopes
- `iam`: Identity & Access Management bounded context.
- `profiles`: Profiles bounded context.
- `publishing`: Publishing bounded context (tutorials, categories, assets).
- `shared`: Shared kernel, pipeline behaviors, or domain abstractions.
- `openapi`: OpenAPI and Scalar documentation configuration.
- `persistence`: Entity Framework Core configurations and repositories.

---

## 3. Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Docker](https://www.docker.com/) (for local MySQL database container)
- JetBrains Rider or your preferred .NET IDE

### Local Environment Setup
1. Clone the repository:
   ```bash
   git clone https://github.com/acme/learning-center-platform.git
   cd learning-center-platform
   ```

2. Start the MySQL database:
   ```bash
   docker compose up -d
   ```

3. Build and run the solution:
   ```bash
   dotnet build
   dotnet run --project Acme.Center.Platform
   ```

4. Access the interactive API reference:
   - Scalar UI: `http://localhost:5000/scalar/v1`
   - OpenAPI document: `http://localhost:5000/openapi/v1.json`

---

## 4. Code Style & Quality Checklist

Before submitting a Pull Request, please ensure:
1. **Build Verification:** Solution compiles without warnings or errors (`dotnet build`).
2. **Architecture Conformity:**
   - New aggregates and entities are placed in the appropriate bounded context.
   - Aggregate roots manage their domain events using `AddDomainEvent`.
   - Repositories utilize `StronglyTypedIdConverter` for strongly-typed identifier keys.
   - REST controllers map domain results via `.Match()` or problem details assemblers.
3. **Documentation:**
   - Update `docs/user-stories.md` if adding or changing REST endpoints.
   - Update `docs/class-diagram.puml` if modifying entities, interfaces, or relationships.
   - Add Architecture Decision Records in `docs/adrs.md` for major technical decisions.
   - Update `CHANGELOG.md` under `[Unreleased]` or target version.

---

## 5. Pull Request Process

1. Create a feature branch from `main`:
   ```bash
   git checkout -b feat/your-feature-name
   ```
2. Commit your changes following conventional commit syntax.
3. Push to your branch and open a Pull Request with a clear description of changes, motivation, and verification steps.
4. Ensure all CI pipeline checks pass.
