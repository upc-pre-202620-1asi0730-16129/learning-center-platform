# Learning Center Platform - Study Guide

## Table of Contents
1. [Overview](#1-overview)
2. [Architecture Patterns](#2-architecture-patterns)
3. [Domain-Driven Design (DDD)](#3-domain-driven-design-ddd)
4. [CQRS Pattern](#4-cqrs-pattern)
5. [.NET Framework Features](#5-net-framework-features)
6. [C# Language Features](#6-c-language-features)
7. [Entity Framework Core](#7-entity-framework-core)
8. [Security & Authentication](#8-security-and-authentication)
9. [API Design](#9-api-design)
10. [Testing & Quality](#10-testing-and-quality)
11. [DevOps & Deployment](#11-devops-and-deployment)
12. [References](#12-references)

---

## 1. Overview

The **Learning Center Platform** is a production-quality RESTful API built with **.NET 10** and **C# 14**. It serves as a reference implementation demonstrating modern software engineering practices for web applications.

### Technology Stack
- **Framework**: .NET 10 (ASP.NET Core)
- **Language**: C# 14
- **Database**: MySQL via Entity Framework Core
- **Authentication**: JSON Web Tokens (JWT) with BCrypt password hashing
- **Containerization**: Docker with multi-stage builds
- **API Documentation**: OpenAPI with Scalar
- **Mediator**: Cortex Mediator for command/query handling and event publishing

### Key Architectural Approaches
- **Domain-Driven Design (DDD)** with Bounded Contexts
- **Clean Architecture** with layered separation
- **CQRS** (Command Query Responsibility Segregation)
- **Mediator Pattern** with pipeline behaviors
- **Repository Pattern** over Entity Framework Core
- **Unit of Work** for transaction management

### Bounded Contexts

The application is divided into four bounded contexts, each with its own domain model, application services, and infrastructure:

| Context | Responsibility | Key Entities |
|---------|----------------|--------------|
| **Shared** | Common infrastructure, base classes, and cross-cutting concerns | AggregateRoot, Result, Error |
| **IAM** | Identity and Access Management: users, authentication, tokens | User, UserId |
| **Profiles** | User profiles and personal information | Profile, PersonName, EmailAddress |
| **Publishing** | Educational content management | Tutorial, Category |

Each bounded context follows a consistent internal structure:

```
BoundedContext/
├── Application/
│   ├── Acl/                  # Anti-Corruption Layer implementations
│   ├── CommandServices/      # Command service interfaces
│   ├── QueryServices/        # Query service interfaces
│   ├── Internal/             # Internal implementations
│   │   ├── CommandServices/
│   │   ├── QueryServices/
│   │   ├── EventHandlers/
│   │   └── OutboundServices/
├── Domain/
│   ├── Model/
│   │   ├── Aggregates/       # Aggregate roots
│   │   ├── Commands/         # Command definitions
│   │   ├── Queries/          # Query definitions
│   │   ├── Events/           # Domain events
│   │   ├── ValueObjects/     # Value objects
│   │   └── Errors/           # Error catalogs
│   └── Repositories/         # Repository interfaces
├── Infrastructure/
│   ├── Persistence/          # EF Core repositories and configuration
│   ├── Pipeline/             # Middleware components
│   ├── Hashing/              # Password hashing
│   └── Tokens/               # JWT token services
├── Interfaces/
│   ├── Rest/                 # REST controllers and resources
│   │   ├── Resources/        # DTOs
│   │   └── Transform/        # Assemblers
│   └── Acl/                  # ACL interfaces
└── Resources/                # Localized messages
```

---

## 2. Architecture Patterns

### 2.1 Clean Architecture

**Concept**: Clean Architecture separates concerns into concentric layers, with the domain at the center and infrastructure on the outside. The Dependency Rule states that source code dependencies must point inward — the domain layer knows nothing about the infrastructure or presentation layers.

**In the Codebase**: Each bounded context follows a four-layer structure:

```
Interfaces/ (Presentation Layer)
    ↓ depends on
Application/ (Application Layer)
    ↓ depends on
Domain/ (Domain Layer - Core Business Logic)
    ↑ independent of
Infrastructure/ (Infrastructure Layer)
```

The domain layer contains no references to ASP.NET Core, Entity Framework, or any external framework. Infrastructure implements interfaces defined in the domain layer.

**Key Files**:
- `Iam/Interfaces/Rest/UsersController.cs` — Presentation layer
- `Iam/Application/CommandServices/IUserCommandService.cs` — Application layer interface
- `Iam/Application/Internal/CommandServices/UserCommandService.cs` — Application layer implementation
- `Iam/Domain/Model/Aggregates/User.cs` — Domain layer
- `Iam/Infrastructure/Persistence/EntityFrameworkCore/Repositories/UserRepository.cs` — Infrastructure layer

**Why It Matters**: This separation allows you to change the database, UI framework, or external services without affecting business logic. The domain model remains pure and testable in isolation.

### 2.2 Repository Pattern

**Concept**: The Repository pattern abstracts data access behind a collection-like interface, decoupling domain logic from persistence details. The domain defines the interface; infrastructure provides the implementation.

**In the Codebase**:

```csharp
// Domain defines the interface
public interface IUserRepository : IBaseRepository<User>
{
    Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken);
}

// Infrastructure implements it using EF Core
public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    public async Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return await Context.Set<User>()
            .FirstOrDefaultAsync(user => user.Username.Equals(username), cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken)
    {
        return await Context.Set<User>().AnyAsync(user => user.Username.Equals(username), cancellationToken);
    }
}
```

The generic base repository provides common CRUD operations:

```csharp
public interface IBaseRepository<TEntity>
{
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TEntity?> FindByIdAsNoTrackingAsync(int id, CancellationToken cancellationToken = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
    Task<IEnumerable<TEntity>> ListAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<TEntity>> ListAsNoTrackingAsync(CancellationToken cancellationToken = default);
}
```

**Key Files**:
- `Shared/Domain/Repositories/IBaseRepository.cs`
- `Shared/Infrastructure/Persistence/EntityFrameworkCore/Repositories/BaseRepository.cs`
- `Iam/Domain/Repositories/IUserRepository.cs`
- `Iam/Infrastructure/Persistence/EntityFrameworkCore/Repositories/UserRepository.cs`

**Why It Matters**: You can swap databases without changing business logic. Tests can use mock repositories. The domain remains independent of EF Core.

### 2.3 Unit of Work Pattern

**Concept**: Unit of Work maintains a list of affected objects and coordinates changes to the database as a single transaction, ensuring atomicity.

**In the Codebase**:

```csharp
public interface IUnitOfWork
{
    Task CompleteAsync(CancellationToken cancellationToken = default);
}

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
```

The Unit of Work is also triggered automatically via a mediator pipeline behavior:

```csharp
public class TransactionCommandBehavior<TCommand>(
    IUnitOfWork unitOfWork,
    ILogger<TransactionCommandBehavior<TCommand>> logger)
    : ICommandPipelineBehavior<TCommand> where TCommand : ICommand
{
    public async Task Handle(TCommand command, CommandHandlerDelegate next, CancellationToken ct)
    {
        await next();
        logger.LogDebug("Committing unit of work for {CommandType}", typeof(TCommand).Name);
        await unitOfWork.CompleteAsync(ct);
    }
}
```

**Key Files**:
- `Shared/Domain/Repositories/IUnitOfWork.cs`
- `Shared/Infrastructure/Persistence/EntityFrameworkCore/Repositories/UnitOfWork.cs`
- `Shared/Infrastructure/Mediator/Cortex/Configuration/TransactionCommandBehavior.cs`

**Why It Matters**: Ensures atomicity — either all changes succeed or none do. The pipeline behavior makes transaction management automatic and consistent.

### 2.4 Dependency Injection

**Concept**: DI is a design pattern where objects receive their dependencies from external sources rather than creating them. ASP.NET Core has built-in DI support.

**In the Codebase** (`Program.cs`):

```csharp
// Register interfaces with implementations
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUserCommandService, UserCommandService>();
builder.Services.AddScoped<IUserQueryService, UserQueryService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IHashingService, HashingService>();

// Register mediator pipeline behaviors
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(LoggingCommandBehavior<>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(ExceptionHandlingCommandBehavior<>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(TransactionCommandBehavior<>));

// Controllers receive dependencies via constructor
public class UsersController(
    IUserCommandService userCommandService,
    IUserQueryService userQueryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
```

**Key Files**:
- `Program.cs` (lines 120–161)

**Why It Matters**: Makes code testable, flexible, and follows the Dependency Inversion Principle. Dependencies are declared explicitly, making the code easier to understand and maintain.

### 2.5 Middleware Pattern

**Concept**: Middleware components form a pipeline that processes HTTP requests and responses. Each middleware can short-circuit the pipeline or pass control to the next component.

**In the Codebase**:

```csharp
public class RequestAuthorizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IUserQueryService userQueryService,
        ITokenService tokenService)
    {
        // Skip authorization if endpoint is decorated with [AllowAnonymous]
        var allowAnonymous = context.Request.HttpContext.GetEndpoint()?.Metadata
            .Any(m => m.GetType() == typeof(AllowAnonymousAttribute)) ?? false;

        if (allowAnonymous)
        {
            await next(context);
            return;
        }

        // Extract and validate JWT token
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            var userId = await tokenService.ValidateToken(token);
            if (userId != null)
            {
                var getUserByIdQuery = new GetUserByIdQuery(userId.Value);
                var user = await userQueryService.Handle(getUserByIdQuery, context.RequestAborted);
                context.Items["User"] = user;
            }
        }

        await next(context);
    }
}
```

The middleware pipeline in `Program.cs`:

```csharp
app.UseExceptionHandler();
app.UseRequestLocalization(localizationOptions);
app.UseCors("AllowAllPolicy");
app.UseRequestAuthorization();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
```

**Key Files**:
- `Iam/Infrastructure/Pipeline/Middleware/Components/RequestAuthorizationMiddleware.cs`
- `Shared/Infrastructure/Pipeline/Middleware/Components/GlobalExceptionHandler.cs`

**Why It Matters**: Allows cross-cutting concerns (authentication, logging, error handling, CORS) to be separated from business logic and applied consistently.

### 2.6 Assembler Pattern

**Concept**: Assemblers transform data between layers, converting between Resources (DTOs), Commands/Queries, and Entities. This keeps domain models clean from DTO concerns.

**In the Codebase**:

```csharp
// Resource → Command transformation
public static class SignInCommandFromResourceAssembler
{
    public static SignInCommand ToCommandFromResource(SignInResource resource)
    {
        if (resource == null)
            throw new ArgumentNullException(nameof(resource),
                "SignInResource cannot be null when converting to command.");
        return new SignInCommand(resource.Username, resource.Password);
    }
}
```

Result-to-ActionResult transformation with switch expressions:

```csharp
public static class IamActionResultAssembler
{
    private static int ToStatusCodeFromIamError(IamError error)
    {
        return error switch
        {
            IamError.InvalidCredentials => StatusCodes.Status400BadRequest,
            IamError.UsernameAlreadyTaken => StatusCodes.Status409Conflict,
            IamError.OperationCancelled => StatusCodes.Status409Conflict,
            IamError.DatabaseError => StatusCodes.Status500InternalServerError,
            IamError.InternalServerError => StatusCodes.Status500InternalServerError,
            IamError.ExternalServiceError => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status400BadRequest
        };
    }

    public static IActionResult ToActionResultFromSignUpResult(
        ControllerBase controller,
        Result<User> result,
        IStringLocalizer<ErrorMessages> errorLocalizer,
        ProblemDetailsFactory problemDetailsFactory,
        Func<User, IActionResult> successAction)
    {
        if (result.IsSuccess) return successAction(result.Value!);
        var statusCode = ToStatusCodeFromIamError((IamError)result.Error!);
        return problemDetailsFactory.CreateProblemDetails(controller, statusCode, result.Error, result.Message);
    }
}
```

**Key Files**:
- `Iam/Interfaces/Rest/Transform/SignInCommandFromResourceAssembler.cs`
- `Iam/Interfaces/Rest/Transform/SignUpCommandFromResourceAssembler.cs`
- `Iam/Interfaces/Rest/Transform/UserResourceFromEntityAssembler.cs`
- `Iam/Interfaces/Rest/Transform/IamActionResultAssembler.cs`

**Why It Matters**: Keeps domain models clean from DTO concerns and vice versa. Makes the transformation logic explicit and testable.

### 2.7 Mediator Pattern with Pipeline Behaviors

**Concept**: The Mediator pattern centralizes request handling. Pipeline behaviors intercept requests for cross-cutting concerns like logging, exception handling, and transactions.

**In the Codebase**:

```csharp
// Logging behavior — logs before and after command execution
public class LoggingCommandBehavior<TCommand>(
    ILogger<LoggingCommandBehavior<TCommand>> logger)
    : ICommandPipelineBehavior<TCommand> where TCommand : ICommand
{
    public async Task Handle(TCommand command, CommandHandlerDelegate next, CancellationToken ct)
    {
        var commandName = typeof(TCommand).Name;
        logger.LogInformation("Executing command {CommandName}", commandName);
        await next();
        logger.LogInformation("Executed command {CommandName}", commandName);
    }
}

// Exception handling behavior — catches and logs exceptions
public class ExceptionHandlingCommandBehavior<TCommand>(
    ILogger<ExceptionHandlingCommandBehavior<TCommand>> logger)
    : ICommandPipelineBehavior<TCommand> where TCommand : ICommand
{
    public async Task Handle(TCommand command, CommandHandlerDelegate next, CancellationToken ct)
    {
        try
        {
            await next();
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("Command {CommandName} was cancelled.", typeof(TCommand).Name);
            throw;
        }
        catch (DbUpdateException ex)
        {
            logger.LogError(ex, "Database update error during command {CommandName}.", typeof(TCommand).Name);
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled error during command {CommandName}.", typeof(TCommand).Name);
            throw;
        }
    }
}
```

**Key Files**:
- `Shared/Infrastructure/Mediator/Cortex/Configuration/LoggingCommandBehavior.cs`
- `Shared/Infrastructure/Mediator/Cortex/Configuration/ExceptionHandlingCommandBehavior.cs`
- `Shared/Infrastructure/Mediator/Cortex/Configuration/TransactionCommandBehavior.cs`

**Why It Matters**: Centralizes cross-cutting concerns. Adding a new behavior (e.g., validation, caching) applies to all commands without modifying individual handlers.

### 2.8 Service Layer Pattern

**Concept**: The Service Layer defines an application's boundary, providing operations that encapsulate business logic and coordinate domain objects.

**In the Codebase**:

```csharp
public class UserCommandService(
    IUserRepository userRepository,
    ITokenService tokenService,
    IHashingService hashingService,
    IUnitOfWork unitOfWork)
    : IUserCommandService
{
    public async Task<Result<(User user, string token)>> Handle(SignInCommand command,
        CancellationToken cancellationToken)
    {
        var user = await userRepository.FindByUsernameAsync(command.Username, cancellationToken);

        if (user == null || !hashingService.VerifyPassword(command.Password, user.PasswordHash))
            return Result<(User user, string token)>.Failure(IamErrors.InvalidCredentials, IamError.InvalidCredentials);

        var token = tokenService.GenerateToken(user);
        return Result<(User user, string token)>.Success((user, token));
    }

    public async Task<Result<User>> Handle(SignUpCommand command, CancellationToken cancellationToken)
    {
        if (await userRepository.ExistsByUsernameAsync(command.Username, cancellationToken))
            return Result<User>.Failure(IamErrors.UsernameAlreadyTaken, IamError.UsernameAlreadyTaken);

        var hashedPassword = hashingService.HashPassword(command.Password);
        var user = new User(command.Username, hashedPassword);

        await userRepository.AddAsync(user, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<User>.Success(user);
    }
}
```

**Key Files**:
- `Iam/Application/CommandServices/IUserCommandService.cs`
- `Iam/Application/Internal/CommandServices/UserCommandService.cs`

**Why It Matters**: Orchestrates domain objects and infrastructure services. Keeps controllers thin and focused on HTTP concerns.

---

## 3. Domain-Driven Design (DDD)

### 3.1 Bounded Contexts

**Concept**: A Bounded Context defines the boundaries within which a domain model applies. Each context has its own ubiquitous language, models, and rules. This prevents model confusion as the system grows.

**In the Codebase**: The application is partitioned into four bounded contexts:

- **IAM Context**: Users, authentication, JWT tokens, password hashing
- **Profiles Context**: Personal information, names, addresses
- **Publishing Context**: Tutorials, categories, educational content
- **Shared Context**: Common infrastructure, base classes, cross-cutting concerns

Each context has its own folder structure, domain model, and error catalog.

**Key Files**:
- `Iam/` directory — IAM bounded context
- `Profiles/` directory — Profiles bounded context
- `Publishing/` directory — Publishing bounded context
- `Shared/` directory — Shared kernel

**Why It Matters**: Prevents model confusion and allows teams to work independently on different contexts. Each context can evolve its model without affecting others.

### 3.2 Aggregate Roots

**Concept**: An Aggregate Root is the entry point to an aggregate — a cluster of objects treated as a single unit for data changes. All access to internal objects goes through the aggregate root, which enforces consistency rules.

**In the Codebase**:

```csharp
// Base class for all aggregate roots
public abstract class AggregateRoot : IAggregateRoot
{
    private readonly List<IEvent> _domainEvents = [];

    public IReadOnlyCollection<IEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

// User aggregate root
public partial class User : AggregateRoot
{
    public UserId Id { get; }
    public string Username { get; private set; }

    [JsonIgnore] public string PasswordHash { get; private set; }

    public User(string username, string passwordHash)
    {
        Username = username;
        PasswordHash = passwordHash;

        if (!string.IsNullOrWhiteSpace(username))
        {
            AddDomainEvent(new UserRegisteredEvent(Id, username));
        }
    }

    public User UpdateUsername(string username)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        Username = username;
        return this;
    }
}

// Tutorial aggregate root
public partial class Tutorial : AggregateRoot
{
    public TutorialId Id { get; }
    public string Title { get; private set; }
    public string Summary { get; private set; }
    public CategoryId CategoryId { get; private set; }

    public Tutorial(string title, string summary, CategoryId categoryId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(summary);
        Title = title;
        Summary = summary;
        CategoryId = categoryId;
        AddDomainEvent(new TutorialCreatedEvent(Id, Title, CategoryId));
    }
}
```

**Key Files**:
- `Shared/Domain/Model/Aggregates/IAggregateRoot.cs`
- `Shared/Domain/Model/Aggregates/Internal/AggregateRoot.cs`
- `Iam/Domain/Model/Aggregates/User.cs`
- `Publishing/Domain/Model/Aggregates/Tutorial.cs`
- `Publishing/Domain/Model/Entities/Category.cs`

**Why It Matters**: Ensures consistency within aggregates and controls access to internal objects. The aggregate root is the only way to modify the aggregate's state.

### 3.3 Value Objects

**Concept**: Value Objects are immutable objects identified by their attributes, not by identity. Two value objects with the same attributes are considered equal. They eliminate primitive obsession by encapsulating validation logic.

**In the Codebase**:

```csharp
public record PersonName(string FirstName, string LastName)
{
    public PersonName() : this(string.Empty, string.Empty)
    {
    }

    public PersonName(string firstName) : this(firstName, string.Empty)
    {
    }

    public string FullName => $"{FirstName} {LastName}";
}
```

**Key Files**:
- `Profiles/Domain/Model/ValueObjects/PersonName.cs`
- `Profiles/Domain/Model/ValueObjects/EmailAddress.cs`
- `Profiles/Domain/Model/ValueObjects/StreetAddress.cs`

**Why It Matters**: Eliminates primitive obsession and encapsulates validation logic. Value objects are immutable, making them safe to share across the domain.

### 3.4 Strongly-Typed IDs

**Concept**: Instead of using primitive types (`int`, `string`) for IDs, use dedicated types to prevent mixing IDs from different entities. This is a form of type safety that catches bugs at compile time.

**In the Codebase**:

```csharp
public readonly record struct UserId(int Value) : IEntityId<int>
{
    public static implicit operator int(UserId id) => id.Value;
    public static implicit operator UserId(int value) => new(value);

    public override string ToString() => Value.ToString();
}
```

EF Core value converter for database mapping:

```csharp
public class StronglyTypedIdConverter<TId, TValue>(
    Func<TId, TValue> convertToProvider,
    Func<TValue, TId> convertFromProvider)
    : ValueConverter<TId, TValue>(
        id => convertToProvider(id),
        val => convertFromProvider(val))
    where TValue : struct
{
    public StronglyTypedIdConverter(Func<TValue, TId> factory)
        : this(id => ((IEntityId<TValue>)id!).Value, factory)
    {
    }
}

// Usage in entity configuration
builder.Entity<User>().Property(u => u.Id)
    .HasConversion(new StronglyTypedIdConverter<UserId, int>(id => id.Value, value => new UserId(value)))
    .IsRequired()
    .ValueGeneratedOnAdd();
```

**Key Files**:
- `Iam/Domain/Model/ValueObjects/UserId.cs`
- `Profiles/Domain/Model/ValueObjects/ProfileId.cs`
- `Publishing/Domain/Model/ValueObjects/TutorialId.cs`
- `Shared/Infrastructure/Persistence/EntityFrameworkCore/Configuration/ValueConverters/StronglyTypedIdConverter.cs`

**Why It Matters**: Prevents bugs like passing a `UserId` where a `ProfileId` is expected. The compiler catches these errors at compile time.

### 3.5 Domain Events

**Concept**: Domain Events represent something significant that happened in the domain. They enable loose coupling between bounded contexts by allowing one context to react to events from another without direct dependencies.

**In the Codebase**:

```csharp
// Event interface extending mediator notification
public interface IEvent : INotification
{
}

// Domain event definition
public record UserRegisteredEvent(UserId UserId, string Username) : IEvent;

// Raised in the aggregate root
public User(string username, string passwordHash)
{
    Username = username;
    PasswordHash = passwordHash;

    if (!string.IsNullOrWhiteSpace(username))
    {
        AddDomainEvent(new UserRegisteredEvent(Id, username));
    }
}

// EF Core interceptor dispatches events after save
public sealed class DispatchDomainEventsInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    private async Task DispatchDomainEvents(DbContext? context, CancellationToken cancellationToken = default)
    {
        if (context is null) return;

        var aggregateRoots = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var aggregateRoot in aggregateRoots)
        {
            aggregateRoot.ClearDomainEvents();
        }

        foreach (var domainEvent in domainEvents)
        {
            await mediator.PublishAsync(domainEvent, cancellationToken);
        }
    }
}

// Event handler in a different bounded context
public class UserRegisteredEventHandler(ILogger<UserRegisteredEventHandler> logger)
    : IEventHandler<UserRegisteredEvent>
{
    public Task Handle(UserRegisteredEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing UserRegisteredEvent for User ID {UserId} ({Username}) in Profiles context",
            domainEvent.UserId.Value, domainEvent.Username);
        return Task.CompletedTask;
    }
}
```

**Key Files**:
- `Shared/Domain/Model/Events/IEvent.cs`
- `Iam/Domain/Model/Events/UserRegisteredEvent.cs`
- `Shared/Infrastructure/Persistence/EntityFrameworkCore/Interceptors/DispatchDomainEventsInterceptor.cs`
- `Profiles/Application/Internal/EventHandlers/UserRegisteredEventHandler.cs`

**Why It Matters**: Enables reactive systems and cross-context communication without tight coupling. The Profiles context can react to user registration without depending on the IAM context's internal implementation.

### 3.6 Anti-Corruption Layer (ACL)

**Concept**: An ACL protects your bounded context from external models by providing a translation layer. It isolates your domain from changes in external systems.

**In the Codebase**:

```csharp
// ACL interface — defines what other contexts can access
public interface IIamContextFacade
{
    Task<int> CreateUser(string username, string password, CancellationToken cancellationToken);
    Task<int> FetchUserIdByUsername(string username, CancellationToken cancellationToken);
    Task<string> FetchUsernameByUserId(int userId, CancellationToken cancellationToken);
}

// ACL implementation — uses command/query services internally
public class IamContextFacade(IUserCommandService userCommandService, IUserQueryService userQueryService)
    : IIamContextFacade
{
    public async Task<int> FetchUserIdByUsername(string username, CancellationToken cancellationToken)
    {
        var getUserByUsernameQuery = new GetUserByUsernameQuery(username);
        var result = await userQueryService.Handle(getUserByUsernameQuery, cancellationToken);
        return result?.Id ?? 0;
    }

    public async Task<string> FetchUsernameByUserId(int userId, CancellationToken cancellationToken)
    {
        var getUserByIdQuery = new GetUserByIdQuery(userId);
        var result = await userQueryService.Handle(getUserByIdQuery, cancellationToken);
        return result?.Username ?? string.Empty;
    }
}
```

**Key Files**:
- `Iam/Interfaces/Acl/IIamContextFacade.cs`
- `Iam/Application/Acl/IamContextFacade.cs`

**Why It Matters**: Prevents external changes from corrupting your domain model. Other contexts interact with IAM through a stable interface, not through internal domain objects.

### 3.7 Error Catalogs

**Concept**: Error catalogs centralize error definitions per bounded context, making error handling consistent and discoverable.

**In the Codebase**:

```csharp
// Error type enum
public enum EErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
    Unexpected = 6
}

// Error record with factory methods
public record Error(string Code, string Message, EErrorType Type = EErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty, EErrorType.Failure);

    public static Error NotFound(string code, string message) => new(code, message, EErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, EErrorType.Conflict);
    public static Error Validation(string code, string message) => new(code, message, EErrorType.Validation);
    public static Error Unauthorized(string code, string message) => new(code, message, EErrorType.Unauthorized);
}

// IAM-specific error catalog
public static class IamErrors
{
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Iam.InvalidCredentials", "Invalid username or password.");

    public static readonly Error UsernameAlreadyTaken =
        Error.Conflict("Iam.UsernameAlreadyTaken", "The specified username is already taken.");

    public static readonly Error UserNotFound =
        Error.NotFound("Iam.UserNotFound", "The specified user was not found.");
}

// IAM-specific error enum for status code mapping
public enum IamError
{
    None,
    UserNotFound,
    UsernameAlreadyTaken,
    InvalidCredentials,
    OperationCancelled,
    DatabaseError,
    InternalServerError,
    ExternalServiceError
}
```

**Key Files**:
- `Shared/Domain/Model/Errors/Error.cs`
- `Iam/Domain/Model/Errors/IamErrors.cs`
- `Iam/Domain/Model/IamError.cs`

**Why It Matters**: Centralizes error definitions, making them easy to find and maintain. Error types map to HTTP status codes for consistent API responses.

---

## 4. CQRS Pattern

### 4.1 Command Query Responsibility Segregation

**Concept**: CQRS separates read and write operations into different models. Commands modify state; Queries retrieve data. This separation allows optimizing reads and writes independently.

**In the Codebase**:

```csharp
// Commands (Write operations) — return Result<T>
public interface IUserCommandService
{
    Task<Result<(User user, string token)>> Handle(SignInCommand command, CancellationToken cancellationToken);
    Task<Result<User>> Handle(SignUpCommand command, CancellationToken cancellationToken);
}

// Queries (Read operations) — return entities directly
public interface IUserQueryService
{
    Task<User?> Handle(GetUserByIdQuery query, CancellationToken cancellationToken);
    Task<IEnumerable<User>> Handle(GetAllUsersQuery query, CancellationToken cancellationToken);
    Task<User?> Handle(GetUserByUsernameQuery query, CancellationToken cancellationToken);
}
```

**Key Files**:
- `Iam/Application/CommandServices/IUserCommandService.cs`
- `Iam/Application/QueryServices/IUserQueryService.cs`
- `Iam/Application/Internal/CommandServices/UserCommandService.cs`
- `Iam/Application/Internal/QueryServices/UserQueryService.cs`

**Why It Matters**: Commands focus on business rules and validation; queries focus on performance. This separation makes the codebase easier to understand and maintain.

### 4.2 Commands & Queries as Records

**Concept**: Commands and Queries are simple, immutable data carriers that express intent. Records provide value-based equality and immutability by default.

**In the Codebase**:

```csharp
public record SignInCommand(string Username, string Password);
public record SignUpCommand(string Username, string Password);

public record GetUserByIdQuery(UserId Id)
{
    public GetUserByIdQuery(int id) : this(new UserId(id))
    {
    }
}

public record GetAllUsersQuery;
public record GetUserByUsernameQuery(string Username);
```

**Key Files**:
- `Iam/Domain/Model/Commands/SignInCommand.cs`
- `Iam/Domain/Model/Commands/SignUpCommand.cs`
- `Iam/Domain/Model/Queries/GetUserByIdQuery.cs`
- `Iam/Domain/Model/Queries/GetAllUsersQuery.cs`

**Why It Matters**: Makes the intent clear and keeps data immutable. Records reduce boilerplate and provide value-based equality.

---

## 5. .NET Framework Features

### 5.1 ASP.NET Core Web API

**Concept**: ASP.NET Core is a cross-platform framework for building web APIs and services. It provides routing, model binding, validation, and a middleware pipeline.

**In the Codebase**:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddControllers(options => options.Conventions.Add(new KebabCaseRouteNamingConvention()))
    .AddDataAnnotationsLocalization();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();
app.MapControllers();
app.Run();
```

**Key Files**:
- `Program.cs`

**Why It Matters**: Provides routing, model binding, validation, and middleware pipeline. Kebab-case routing convention produces RESTful URLs like `/api/v1/users` instead of `/api/v1/Users`.

### 5.2 Generic Host & Minimal Hosting

**Concept**: The generic host provides a unified way to configure services and middleware. Minimal hosting simplifies application startup with a fluent builder pattern.

**In the Codebase**:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(...);
builder.Services.AddScoped<IUserRepository, UserRepository>();
var app = builder.Build();
```

**Key Files**:
- `Program.cs`

**Why It Matters**: Simplifies application startup and configuration. No more `Startup.cs` — everything is in `Program.cs`.

### 5.3 Configuration System

**Concept**: .NET Configuration provides a flexible way to manage settings from multiple sources: JSON files, environment variables, Azure Key Vault, and more.

**In the Codebase**:

```csharp
// Read connection string with environment variable expansion
var connectionStringTemplate = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionString = Environment.ExpandEnvironmentVariables(connectionStringTemplate);

// Bind to strongly-typed settings
builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));
```

**Key Files**:
- `Iam/Infrastructure/Tokens/Jwt/Configuration/TokenSettings.cs`
- `Program.cs` (lines 76–93)

**Why It Matters**: Keeps configuration external from code and supports environment-specific settings. Environment variable expansion allows secrets to be injected at runtime.

### 5.4 Localization

**Concept**: ASP.NET Core supports internationalization (i18n) for multi-language applications through resource files and the `IStringLocalizer` interface.

**In the Codebase**:

```csharp
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.AddSingleton<IStringLocalizer<ErrorMessages>, StringLocalizer<ErrorMessages>>();
builder.Services.AddSingleton<IStringLocalizer<IamMessages>, StringLocalizer<IamMessages>>();

var supportedCultures = new[] { "en", "es" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture(supportedCultures[0])
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);

app.UseRequestLocalization(localizationOptions);

// Use in controllers
private readonly IStringLocalizer<ErrorMessages> _errorLocalizer;
var message = _errorLocalizer["UserNotFound"];
```

**Key Files**:
- `Iam/Resources/IamMessages.cs`
- `Shared/Resources/Errors/ErrorMessages.cs`
- `Program.cs` (lines 95–108, 177–183)

**Why It Matters**: Enables applications to support multiple languages. Error messages are localized, making the API accessible to international users.

### 5.5 OpenAPI & Scalar

**Concept**: OpenAPI (formerly Swagger) provides interactive API documentation. Scalar is a modern, customizable API reference UI.

**In the Codebase**:

```csharp
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
```

**Key Files**:
- `Shared/Infrastructure/Interfaces/AspNetCore/OpenApi/BearerSecuritySchemeTransformer.cs`
- `Program.cs` (lines 114–118, 185–189)

**Why It Matters**: Automatically generates API documentation for consumers. Bearer security scheme allows testing authenticated endpoints directly from the UI.

### 5.6 CORS Policy

**Concept**: Cross-Origin Resource Sharing (CORS) controls which origins can access your API from browsers.

**In the Codebase**:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllPolicy",
        policy => policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

app.UseCors("AllowAllPolicy");
```

**Key Files**:
- `Program.cs` (lines 64–71, 192)

**Why It Matters**: Allows frontend applications from different origins to access the API. In production, restrict origins to known clients.

---

## 6. C# Language Features

### 6.1 Records

**Concept**: Records are immutable reference types with value-based equality. They are perfect for DTOs, commands, queries, and domain events.

**In the Codebase**:

```csharp
public record SignInCommand(string Username, string Password);
public record UserRegisteredEvent(UserId UserId, string Username) : IEvent;
public record PersonName(string FirstName, string LastName);
public record Error(string Code, string Message, EErrorType Type = EErrorType.Failure);
```

**Key Files**:
- `Iam/Domain/Model/Commands/SignInCommand.cs`
- `Iam/Domain/Model/Events/UserRegisteredEvent.cs`
- `Profiles/Domain/Model/ValueObjects/PersonName.cs`
- `Shared/Domain/Model/Errors/Error.cs`

**Why It Matters**: Reduces boilerplate code and ensures immutability by default. Value-based equality makes records ideal for data carriers.

### 6.2 Primary Constructors

**Concept**: Primary constructors allow you to declare constructor parameters directly in the class or record declaration, reducing boilerplate.

**In the Codebase**:

```csharp
public class UserRepository(AppDbContext context) : BaseRepository<User>(context), IUserRepository
{
    // 'context' is automatically available as a readonly field
}

public class UsersController(
    IUserCommandService userCommandService,
    IUserQueryService userQueryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    // Services are automatically injected
}

public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    public async Task CompleteAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }
}
```

**Key Files**:
- `Iam/Infrastructure/Persistence/EntityFrameworkCore/Repositories/UserRepository.cs`
- `Iam/Interfaces/Rest/UsersController.cs`
- `Shared/Infrastructure/Persistence/EntityFrameworkCore/Repositories/UnitOfWork.cs`

**Why It Matters**: Reduces boilerplate and makes dependency injection cleaner. No need for separate field declarations and constructor bodies.

### 6.3 Pattern Matching

**Concept**: Pattern matching allows you to check types and extract values in a concise way. Switch expressions provide a more expressive alternative to if-else chains.

**In the Codebase**:

```csharp
// Switch expression for error-to-status-code mapping
private static int ToStatusCodeFromIamError(IamError error)
{
    return error switch
    {
        IamError.InvalidCredentials => StatusCodes.Status400BadRequest,
        IamError.UsernameAlreadyTaken => StatusCodes.Status409Conflict,
        IamError.OperationCancelled => StatusCodes.Status409Conflict,
        IamError.DatabaseError => StatusCodes.Status500InternalServerError,
        IamError.InternalServerError => StatusCodes.Status500InternalServerError,
        IamError.ExternalServiceError => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status400BadRequest
    };
}

// Property pattern matching in Result handling
return result switch
{
    { IsSuccess: true, Value: var user } => Ok(user),
    { IsSuccess: false, Error: var error } => BadRequest(error),
    _ => StatusCode(500)
};

// Pattern matching in interceptors
if (entry.State is EntityState.Added or EntityState.Modified)
    entry.Entity.UpdatedAt = now;
```

**Key Files**:
- `Iam/Interfaces/Rest/Transform/IamActionResultAssembler.cs`
- `Shared/Infrastructure/Persistence/EntityFrameworkCore/Interceptors/AuditableEntityInterceptor.cs`

**Why It Matters**: Makes conditional logic more readable and expressive. Switch expressions eliminate boilerplate and make the intent clear.

### 6.4 Nullable Reference Types

**Concept**: Nullable reference types help prevent null reference exceptions at compile time by distinguishing between nullable and non-nullable references.

**In the Codebase**:

```csharp
public Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken)
{
    // Return type User? indicates the method can return null
    return await Context.Set<User>()
        .FirstOrDefaultAsync(user => user.Username.Equals(username), cancellationToken);
}

// Null-coalescing operator for default values
return result?.Id ?? 0;
return result?.Username ?? string.Empty;

// Null-conditional assignment
if (entry.State == EntityState.Added)
    entry.Entity.CreatedAt ??= now;
```

**Key Files**:
- Throughout the codebase with `#nullable enable`

**Why It Matters**: Catches potential null reference errors at compile time instead of runtime. Makes the code's nullability intent explicit.

### 6.5 File-Scoped Namespaces

**Concept**: File-scoped namespaces eliminate nesting and reduce indentation by declaring the namespace at the file level.

**In the Codebase**:

```csharp
namespace Acme.Center.Platform.Iam.Domain.Model.Aggregates;

// No need for closing braces for the namespace
public class User : AggregateRoot
{
    // ...
}
```

**Key Files**:
- All C# files in the project

**Why It Matters**: Cleaner code with less indentation. Reduces visual noise and makes the file structure clearer.

### 6.6 Collection Expressions

**Concept**: Collection expressions provide a unified syntax for creating arrays, spans, and lists using square brackets.

**In the Codebase**:

```csharp
private readonly List<IEvent> _domainEvents = [];
```

**Key Files**:
- `Shared/Domain/Model/Aggregates/Internal/AggregateRoot.cs`

**Why It Matters**: Simplifies collection initialization syntax. The `[]` syntax works for arrays, spans, and lists.

### 6.7 Expression-Bodied Members

**Concept**: Expression-bodied members provide a concise syntax for methods, properties, and operators using the `=>` arrow.

**In the Codebase**:

```csharp
public string FullName => $"{FirstName} {LastName}";

public static implicit operator int(UserId id) => id.Value;
public static implicit operator UserId(int value) => new(value);

public override string ToString() => Value.ToString();

public static Error NotFound(string code, string message) => new(code, message, EErrorType.NotFound);
```

**Key Files**:
- `Profiles/Domain/Model/ValueObjects/PersonName.cs`
- `Iam/Domain/Model/ValueObjects/UserId.cs`
- `Shared/Domain/Model/Errors/Error.cs`

**Why It Matters**: Makes simple methods and properties more concise and readable.

### 6.8 Static Classes

**Concept**: Static classes cannot be instantiated and contain only static members. They are ideal for utility functions and catalogs.

**In the Codebase**:

```csharp
public static class SignInCommandFromResourceAssembler
{
    public static SignInCommand ToCommandFromResource(SignInResource resource)
    {
        if (resource == null)
            throw new ArgumentNullException(nameof(resource));
        return new SignInCommand(resource.Username, resource.Password);
    }
}

public static class IamErrors
{
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Iam.InvalidCredentials", "Invalid username or password.");
}
```

**Key Files**:
- `Iam/Interfaces/Rest/Transform/SignInCommandFromResourceAssembler.cs`
- `Iam/Domain/Model/Errors/IamErrors.cs`

**Why It Matters**: Makes it clear that the class is a utility or catalog, not an instance. Prevents accidental instantiation.

### 6.9 ArgumentException.ThrowIfNullOrWhiteSpace

**Concept**: A static helper method that throws `ArgumentException` if a string is null, empty, or whitespace.

**In the Codebase**:

```csharp
public User UpdateUsername(string username)
{
    ArgumentException.ThrowIfNullOrWhiteSpace(username);
    Username = username;
    return this;
}
```

**Key Files**:
- `Iam/Domain/Model/Aggregates/User.cs`
- `Publishing/Domain/Model/Aggregates/Tutorial.cs`

**Why It Matters**: Replaces verbose null/empty checks with a single, clear method call.

### 6.10 Tuple Types

**Concept**: Tuples allow returning multiple values from a method without creating a dedicated class.

**In the Codebase**:

```csharp
public async Task<Result<(User user, string token)>> Handle(SignInCommand command,
    CancellationToken cancellationToken)
{
    var user = await userRepository.FindByUsernameAsync(command.Username, cancellationToken);
    // ...
    var token = tokenService.GenerateToken(user);
    return Result<(User user, string token)>.Success((user, token));
}
```

**Key Files**:
- `Iam/Application/Internal/CommandServices/UserCommandService.cs`

**Why It Matters**: Simplifies returning multiple related values without creating a dedicated DTO class.

### 6.11 Using Aliases

**Concept**: Using aliases provide short names for long type names, improving readability.

**In the Codebase**:

```csharp
using BCryptNet = BCrypt.Net.BCrypt;
using ProblemDetailsFactory = Acme.Center.Platform.Shared.Interfaces.Rest.ProblemDetails.ProblemDetailsFactory;
using DomainErrorModel = Acme.Center.Platform.Shared.Domain.Model.Errors.Error;
```

**Key Files**:
- `Iam/Infrastructure/Hashing/BCrypt/Services/HashingService.cs`
- `Program.cs`
- `Shared/Application/Model/Result.cs`

**Why It Matters**: Improves readability when type names are long or ambiguous. Prevents naming conflicts.

---

## 7. Entity Framework Core

### 7.1 DbContext Configuration

**Concept**: DbContext is the main class that coordinates EF Core functionality for a given data model. It manages entity connections, change tracking, and queries.

**In the Codebase**:

```csharp
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        builder.AddInterceptors(new AuditableEntityInterceptor());
        base.OnConfiguring(builder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Per-context configuration via extension methods
        builder.ApplyPublishingConfiguration();
        builder.ApplyProfilesConfiguration();
        builder.ApplyIamConfiguration();

        // Snake_case naming convention for database objects
        builder.UseSnakeCaseNamingConvention();
    }
}
```

Registration in `Program.cs`:

```csharp
builder.Services.AddDbContext<AppDbContext>((serviceProvider, options) =>
{
    var connectionStringTemplate = builder.Configuration.GetConnectionString("DefaultConnection");
    var connectionString = Environment.ExpandEnvironmentVariables(connectionStringTemplate);

    options.UseMySQL(connectionString)
        .UseLoggerFactory(serviceProvider.GetRequiredService<ILoggerFactory>())
        .AddInterceptors(serviceProvider.GetRequiredService<DispatchDomainEventsInterceptor>())
        .EnableDetailedErrors();

    if (builder.Environment.IsDevelopment())
        options.EnableSensitiveDataLogging();
});
```

**Key Files**:
- `Shared/Infrastructure/Persistence/EntityFrameworkCore/Configuration/AppDbContext.cs`
- `Program.cs` (lines 76–93)

**Why It Matters**: Central point for database configuration. Per-context extension methods keep configuration organized and modular.

### 7.2 Fluent API Configuration

**Concept**: Fluent API provides a more expressive way to configure entities compared to data annotations. It keeps entity classes clean.

**In the Codebase**:

```csharp
public static class ModelBuilderExtensions
{
    public static void ApplyIamConfiguration(this ModelBuilder builder)
    {
        builder.Entity<User>().HasKey(u => u.Id);
        builder.Entity<User>().Property(u => u.Id)
            .HasConversion(new StronglyTypedIdConverter<UserId, int>(id => id.Value, value => new UserId(value)))
            .IsRequired()
            .ValueGeneratedOnAdd();
        builder.Entity<User>().Property(u => u.Username).IsRequired();
        builder.Entity<User>().Property(u => u.PasswordHash).IsRequired();
    }
}
```

**Key Files**:
- `Iam/Infrastructure/Persistence/EntityFrameworkCore/Configuration/Extensions/ModelBuilderExtensions.cs`

**Why It Matters**: Keeps entity classes clean and provides more configuration options than data annotations. Extension methods per context keep configuration organized.

### 7.3 Value Converters

**Concept**: Value converters transform values when reading from or writing to the database, enabling strongly-typed IDs while storing primitives.

**In the Codebase**:

```csharp
public class StronglyTypedIdConverter<TId, TValue>(
    Func<TId, TValue> convertToProvider,
    Func<TValue, TId> convertFromProvider)
    : ValueConverter<TId, TValue>(
        id => convertToProvider(id),
        val => convertFromProvider(val))
    where TValue : struct
{
    public StronglyTypedIdConverter(Func<TValue, TId> factory)
        : this(id => ((IEntityId<TValue>)id!).Value, factory)
    {
    }
}
```

**Key Files**:
- `Shared/Infrastructure/Persistence/EntityFrameworkCore/Configuration/ValueConverters/StronglyTypedIdConverter.cs`

**Why It Matters**: Enables strongly-typed IDs while storing primitives in the database. The converter handles the transformation automatically.

### 7.4 Interceptors

**Concept**: Interceptors allow you to hook into EF Core operations like SaveChanges, enabling cross-cutting concerns like auditing and event dispatching.

**In the Codebase**:

```csharp
// Domain events interceptor
public sealed class DispatchDomainEventsInterceptor(IMediator mediator) : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        await DispatchDomainEvents(eventData.Context, cancellationToken);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private async Task DispatchDomainEvents(DbContext? context, CancellationToken cancellationToken = default)
    {
        if (context is null) return;

        var aggregateRoots = context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = aggregateRoots
            .SelectMany(e => e.DomainEvents)
            .ToList();

        foreach (var aggregateRoot in aggregateRoots)
            aggregateRoot.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
            await mediator.PublishAsync(domainEvent, cancellationToken);
    }
}

// Audit timestamp interceptor
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private static void ApplyAuditTimestamps(DbContext? context)
    {
        if (context is null) return;

        var now = DateTimeOffset.UtcNow;

        foreach (var entry in context.ChangeTracker.Entries<IAuditableEntity>())
        {
            if (entry.State is EntityState.Added or EntityState.Modified)
                entry.Entity.UpdatedAt = now;
            if (entry.State == EntityState.Added)
                entry.Entity.CreatedAt ??= now;
        }
    }
}
```

**Key Files**:
- `Shared/Infrastructure/Persistence/EntityFrameworkCore/Interceptors/DispatchDomainEventsInterceptor.cs`
- `Shared/Infrastructure/Persistence/EntityFrameworkCore/Interceptors/AuditableEntityInterceptor.cs`

**Why It Matters**: Enables cross-cutting concerns like auditing and event dispatching without polluting business logic. Interceptors are transparent to the application code.

### 7.5 AsNoTracking for Queries

**Concept**: `AsNoTracking` improves query performance by not tracking entity changes. Use it for read-only queries.

**In the Codebase**:

```csharp
public async Task<User?> FindByUsernameAsNoTrackingAsync(string username, CancellationToken cancellationToken)
{
    return await Context.Set<User>()
        .AsNoTracking()
        .FirstOrDefaultAsync(user => user.Username.Equals(username), cancellationToken);
}

public async Task<IEnumerable<TEntity>> ListAsNoTrackingAsync(CancellationToken cancellationToken = default)
{
    return await Context.Set<TEntity>()
        .AsNoTracking()
        .ToListAsync(cancellationToken);
}
```

**Key Files**:
- `Iam/Infrastructure/Persistence/EntityFrameworkCore/Repositories/UserRepository.cs`
- `Shared/Infrastructure/Persistence/EntityFrameworkCore/Repositories/BaseRepository.cs`

**Why It Matters**: Significantly improves read performance for query operations. No change tracking overhead for read-only scenarios.

### 7.6 EF Core Migrations

**Concept**: Migrations track database schema changes over time, enabling version-controlled database evolution.

**In the Codebase**:

```csharp
// Applied on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}
```

**Key Files**:
- `Migrations/` directory
- `Program.cs` (lines 166–172)

**Why It Matters**: Enables version-controlled database schema changes. Migrations are applied automatically on startup, ensuring the database is always up to date.

---

## 8. Security & Authentication

### 8.1 JWT Authentication

**Concept**: JSON Web Tokens (JWT) provide stateless authentication. The server issues a token after successful authentication, and the client includes it in subsequent requests.

**In the Codebase**:

```csharp
public class TokenService(IOptions<TokenSettings> tokenSettings) : ITokenService
{
    private readonly TokenSettings _tokenSettings = tokenSettings.Value;

    public string GenerateToken(User user)
    {
        var secret = _tokenSettings.Secret;
        var key = Encoding.ASCII.GetBytes(secret);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Sid, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };
        var tokenHandler = new JsonWebTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return token;
    }

    public async Task<int?> ValidateToken(string token)
    {
        if (string.IsNullOrEmpty(token))
            return null;

        var tokenHandler = new JsonWebTokenHandler();
        var key = Encoding.ASCII.GetBytes(_tokenSettings.Secret);

        try
        {
            var tokenValidationResult = await tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            });

            var jwtToken = (JsonWebToken)tokenValidationResult.SecurityToken;
            var userId = int.Parse(jwtToken.Claims.First(claim => claim.Type == ClaimTypes.Sid).Value);
            return userId;
        }
        catch
        {
            return null;
        }
    }
}
```

**Key Files**:
- `Iam/Infrastructure/Tokens/Jwt/Services/TokenService.cs`
- `Iam/Infrastructure/Tokens/Jwt/Configuration/TokenSettings.cs`

**Why It Matters**: Enables stateless, scalable authentication. No server-side session storage required.

### 8.2 Password Hashing

**Concept**: Passwords should never be stored in plain text. BCrypt is a strong, adaptive hashing algorithm that includes a salt.

**In the Codebase**:

```csharp
using BCryptNet = BCrypt.Net.BCrypt;

public class HashingService : IHashingService
{
    public string HashPassword(string password)
    {
        return BCryptNet.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        return BCryptNet.Verify(password, passwordHash);
    }
}
```

**Key Files**:
- `Iam/Infrastructure/Hashing/BCrypt/Services/HashingService.cs`

**Why It Matters**: Protects user credentials even if the database is compromised. BCrypt is adaptive — you can increase the work factor as hardware improves.

### 8.3 Authorization Middleware

**Concept**: Custom middleware validates JWT tokens and sets user context for downstream processing.

**In the Codebase**:

```csharp
public class RequestAuthorizationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(
        HttpContext context,
        IUserQueryService userQueryService,
        ITokenService tokenService)
    {
        // Skip authorization if endpoint is decorated with [AllowAnonymous]
        var allowAnonymous = context.Request.HttpContext.GetEndpoint()?.Metadata
            .Any(m => m.GetType() == typeof(AllowAnonymousAttribute)) ?? false;

        if (allowAnonymous)
        {
            await next(context);
            return;
        }

        // Extract token from Authorization header
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            var userId = await tokenService.ValidateToken(token);
            if (userId != null)
            {
                var getUserByIdQuery = new GetUserByIdQuery(userId.Value);
                var user = await userQueryService.Handle(getUserByIdQuery, context.RequestAborted);
                context.Items["User"] = user;
            }
        }

        await next(context);
    }
}
```

**Key Files**:
- `Iam/Infrastructure/Pipeline/Middleware/Components/RequestAuthorizationMiddleware.cs`
- `Iam/Infrastructure/Pipeline/Middleware/Attributes/AllowAnonymousAttribute.cs`
- `Iam/Infrastructure/Pipeline/Middleware/Attributes/AuthorizeAttribute.cs`

**Why It Matters**: Provides centralized authentication and authorization logic. `[AllowAnonymous]` attribute allows public endpoints.

### 8.4 HTTPS Redirection

**Concept**: HTTPS ensures encrypted communication between client and server.

**In the Codebase**:

```csharp
app.UseHttpsRedirection();
```

**Key Files**:
- `Program.cs` (line 197)

**Why It Matters**: Protects data in transit. All HTTP requests are automatically redirected to HTTPS.

---

## 9. API Design

### 9.1 RESTful Endpoints

**Concept**: RESTful APIs use HTTP methods to perform CRUD operations on resources. Routes follow a consistent naming convention.

**In the Codebase**:

```csharp
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Available User endpoints")]
public class UsersController(
    IUserCommandService userCommandService,
    IUserQueryService userQueryService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    ProblemDetailsFactory problemDetailsFactory)
    : ControllerBase
{
    [HttpPost]
    [AllowAnonymous]
    [EndpointSummary("Create user")]
    [EndpointDescription("Registers a new user in the system")]
    [ProducesResponseType(typeof(UserResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUser([FromBody] SignUpResource signUpResource,
        CancellationToken cancellationToken)
    {
        var signUpCommand = SignUpCommandFromResourceAssembler.ToCommandFromResource(signUpResource);
        var result = await userCommandService.Handle(signUpCommand, cancellationToken);

        return IamActionResultAssembler.ToActionResultFromSignUpResult(
            this, result, _errorLocalizer, _problemDetailsFactory,
            user => CreatedAtAction(nameof(GetUserById), new { id = user.Id.Value },
                UserResourceFromEntityAssembler.ToResourceFromEntity(user))
        );
    }

    [HttpGet("{id}")]
    [EndpointSummary("Get a user by its id")]
    [ProducesResponseType(typeof(UserResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserById(int id, CancellationToken cancellationToken)
    {
        var getUserByIdQuery = new GetUserByIdQuery(id);
        var user = await userQueryService.Handle(getUserByIdQuery, cancellationToken);

        return IamActionResultAssembler.ToActionResultFromGetUserByIdResult(
            this, user, _errorLocalizer, _problemDetailsFactory,
            foundUser => Ok(UserResourceFromEntityAssembler.ToResourceFromEntity(foundUser))
        );
    }

    [HttpGet]
    [EndpointSummary("Get all users")]
    [ProducesResponseType(typeof(IEnumerable<UserResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var getAllUsersQuery = new GetAllUsersQuery();
        var users = await userQueryService.Handle(getAllUsersQuery, cancellationToken);
        var userResources = users.Select(UserResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(userResources);
    }
}
```

**Key Files**:
- `Iam/Interfaces/Rest/UsersController.cs`
- `Iam/Interfaces/Rest/AuthenticationController.cs`

**Why It Matters**: Provides a standard, predictable API interface. Route versioning (`api/v1`) allows future API changes without breaking existing clients.

### 9.2 Kebab-Case Routing

**Concept**: Kebab-case routing produces RESTful URLs like `/api/v1/users` instead of `/api/v1/Users`.

**In the Codebase**:

```csharp
public class KebabCaseRouteNamingConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        foreach (var selector in controller.Selectors)
            selector.AttributeRouteModel = ReplaceControllerTemplate(selector, controller.ControllerName);

        foreach (var selector in controller.Actions.SelectMany(a => a.Selectors))
            selector.AttributeRouteModel = ReplaceControllerTemplate(selector, controller.ControllerName);
    }

    private static AttributeRouteModel? ReplaceControllerTemplate(SelectorModel selector, string name)
    {
        return selector.AttributeRouteModel != null
            ? new AttributeRouteModel
            {
                Template = selector.AttributeRouteModel.Template?.Replace("[controller]", name.ToKebabCase())
            }
            : null;
    }
}
```

**Key Files**:
- `Shared/Infrastructure/Interfaces/AspNetCore/Configuration/KebabCaseRouteNamingConvention.cs`

**Why It Matters**: Produces RESTful URLs that follow convention. No need to manually specify route templates for each controller.

### 9.3 Problem Details (RFC 9457)

**Concept**: Problem Details provides a standard format for error responses, making API errors consistent and machine-readable.

**In the Codebase**:

```csharp
public class ProblemDetailsFactory
{
    public IActionResult CreateProblemDetails(
        ControllerBase controller,
        int statusCode,
        Enum? errorEnum,
        string detailMessage)
    {
        var problemDetails = _aspNetCoreProblemDetailsFactory.CreateProblemDetails(
            controller.HttpContext,
            statusCode,
            errorEnum != null ? _errorLocalizer[$"{errorEnum}"] : _commonLocalizer["GenericError"],
            detail: detailMessage
        );

        problemDetails.Title =
            errorEnum != null ? _errorLocalizer[$"{errorEnum}"] : _commonLocalizer["GenericError"];
        problemDetails.Detail = detailMessage;
        problemDetails.Instance = controller.HttpContext.Request.Path;

        return controller.StatusCode(statusCode, problemDetails);
    }
}
```

Global exception handler:

```csharp
public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    IStringLocalizer<CommonMessages> commonLocalizer)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException opEx)
        {
            logger.LogWarning(opEx, "Request was cancelled: {Message}", opEx.Message);
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = errorLocalizer["OperationCancelled"],
                Detail = errorLocalizer["OperationCancelled"],
                Instance = httpContext.Request.Path
            };
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            });
        }

        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var genericProblemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = commonLocalizer["InternalServerError"],
            Detail = errorLocalizer["GenericError"],
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = genericProblemDetails,
            Exception = exception
        });
    }
}
```

**Key Files**:
- `Shared/Interfaces/Rest/ProblemDetails/ProblemDetailsFactory.cs`
- `Shared/Infrastructure/Pipeline/Middleware/Components/GlobalExceptionHandler.cs`

**Why It Matters**: Standardizes error responses for API consumers. Localized error messages make the API accessible to international users.

### 9.4 Result Pattern

**Concept**: The Result pattern explicitly represents success or failure without exceptions, making error handling predictable and testable.

**In the Codebase**:

```csharp
public class Result<T>
{
    protected Result(bool isSuccess, T? value, string message, Enum? errorEnum, DomainErrorModel? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Message = message;
        Error = errorEnum;
        DomainError = error ?? (isSuccess ? DomainErrorModel.None : DomainErrorModel.Failure(errorEnum?.ToString() ?? "Error", message));
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string Message { get; }
    public Enum? Error { get; }
    public DomainErrorModel DomainError { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, string.Empty, null, DomainErrorModel.None);
    }

    public static Result<T> Failure(Enum error, string message)
    {
        return new Result<T>(false, default, message, error, DomainErrorModel.Failure(error.ToString(), message));
    }

    public static Result<T> Failure(DomainErrorModel error)
    {
        return new Result<T>(false, default, error.Message, null, error);
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<DomainErrorModel, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(DomainError);
}
```

**Key Files**:
- `Shared/Application/Model/Result.cs`

**Why It Matters**: Makes error handling explicit and avoids exception-based control flow. The `Match` method provides functional-style handling.

### 9.5 API Versioning

**Concept**: API versioning allows evolving the API without breaking existing clients.

**In the Codebase**:

```csharp
[Route("api/v1/[controller]")]
```

**Key Files**:
- `Iam/Interfaces/Rest/UsersController.cs`
- `Iam/Interfaces/Rest/AuthenticationController.cs`

**Why It Matters**: Allows introducing breaking changes in new versions while maintaining backward compatibility.

### 9.6 Deprecation with [Obsolete]

**Concept**: The `[Obsolete]` attribute marks APIs as deprecated, warning developers to use alternatives.

**In the Codebase**:

```csharp
[Obsolete("Use UsersController (POST /api/v1/users) for registration and TokensController (POST /api/v1/tokens) for authentication.")]
public class AuthenticationController(...)
```

**Key Files**:
- `Iam/Interfaces/Rest/AuthenticationController.cs`

**Why It Matters**: Communicates API evolution to consumers. Helps migrate clients to new endpoints.

---

## 10. Testing & Quality

### 10.1 Mediator Pipeline Behaviors

**Concept**: Pipeline behaviors intercept commands/queries for cross-cutting concerns like logging, exception handling, and transactions.

**In the Codebase**: Three pipeline behaviors are registered:

```csharp
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(LoggingCommandBehavior<>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(ExceptionHandlingCommandBehavior<>));
builder.Services.AddScoped(typeof(ICommandPipelineBehavior<>), typeof(TransactionCommandBehavior<>));
```

Each behavior wraps the next, forming a pipeline:
1. **LoggingCommandBehavior**: Logs command execution start and completion
2. **ExceptionHandlingCommandBehavior**: Catches and logs exceptions (OperationCanceledException, DbUpdateException, general exceptions)
3. **TransactionCommandBehavior**: Commits the unit of work after successful execution

**Key Files**:
- `Shared/Infrastructure/Mediator/Cortex/Configuration/LoggingCommandBehavior.cs`
- `Shared/Infrastructure/Mediator/Cortex/Configuration/ExceptionHandlingCommandBehavior.cs`
- `Shared/Infrastructure/Mediator/Cortex/Configuration/TransactionCommandBehavior.cs`

**Why It Matters**: Centralizes cross-cutting concerns. Adding a new behavior applies to all commands without modifying individual handlers.

### 10.2 Code Organization

**Concept**: Consistent folder structure improves code navigability and makes the architecture visible.

**In the Codebase**:

```
BoundedContext/
├── Application/
│   ├── Acl/
│   ├── CommandServices/
│   ├── QueryServices/
│   └── Internal/
│       ├── CommandServices/
│       ├── QueryServices/
│       ├── EventHandlers/
│       └── OutboundServices/
├── Domain/
│   ├── Model/
│   │   ├── Aggregates/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   ├── Events/
│   │   ├── ValueObjects/
│   │   └── Errors/
│   └── Repositories/
├── Infrastructure/
│   ├── Persistence/
│   │   └── EntityFrameworkCore/
│   │       ├── Configuration/
│   │       ├── Repositories/
│   │       └── Interceptors/
│   ├── Pipeline/
│   │   └── Middleware/
│   ├── Hashing/
│   └── Tokens/
├── Interfaces/
│   ├── Rest/
│   │   ├── Resources/
│   │   └── Transform/
│   └── Acl/
└── Resources/
```

**Why It Matters**: Makes it easy to find related code and understand the architecture. The folder structure mirrors the Clean Architecture layers.

### 10.3 KDoc Documentation

**Concept**: XML documentation comments (KDoc) provide inline documentation for classes, methods, and parameters.

**In the Codebase**:

```csharp
/// <summary>
///     Base class for aggregate roots in Domain-Driven Design.
///     Encapsulates domain event collection and management.
/// </summary>
public abstract class AggregateRoot : IAggregateRoot
{
    /// <summary>
    ///     Records a domain event to be dispatched when changes are committed.
    /// </summary>
    /// <param name="domainEvent">The domain event to record.</param>
    protected void AddDomainEvent(IEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
```

**Key Files**:
- Throughout the codebase

**Why It Matters**: Provides inline documentation that IDEs can display. Makes the code self-documenting.

---

## 11. DevOps & Deployment

### 11.1 Docker Multi-Stage Build

**Concept**: Multi-stage builds create smaller, optimized images by separating build and runtime stages.

**In the Codebase**:

```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS builder
WORKDIR /app
COPY Acme.Center.Platform/*.csproj Acme.Center.Platform/
RUN dotnet restore ./Acme.Center.Platform
COPY . .
RUN dotnet publish ./Acme.Center.Platform -c Release -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=builder /app/out .
EXPOSE 80
ENTRYPOINT ["dotnet", "Acme.Center.Platform.dll"]
```

**Key Files**:
- `Dockerfile`

**Why It Matters**: Reduces image size and improves security by excluding build tools from the runtime image. The final image contains only the published application.

### 11.2 EF Core Migrations

**Concept**: Migrations track database schema changes over time, enabling version-controlled database evolution.

**In the Codebase**:

```csharp
// Applied on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}
```

**Key Files**:
- `Migrations/` directory
- `Program.cs` (lines 166–172)

**Why It Matters**: Enables version-controlled database schema changes. Migrations are applied automatically on startup.

### 11.3 Environment Variable Configuration

**Concept**: Environment variables allow configuration to be externalized from the codebase, supporting different environments.

**In the Codebase**:

```csharp
var connectionStringTemplate = builder.Configuration.GetConnectionString("DefaultConnection");
var connectionString = Environment.ExpandEnvironmentVariables(connectionStringTemplate);
```

**Key Files**:
- `Program.cs` (lines 78–84)

**Why It Matters**: Secrets and environment-specific settings are injected at runtime, not hardcoded. Supports development, staging, and production environments.

---

## 12. References

### Official Documentation
- [.NET Documentation](https://learn.microsoft.com/en-us/dotnet/) - Comprehensive .NET platform docs
- [ASP.NET Core Documentation](https://learn.microsoft.com/en-us/aspnet/core/) - Web API and hosting docs
- [Entity Framework Core Documentation](https://learn.microsoft.com/en-us/ef/core/) - Data access and ORM docs
- [C# Language Reference](https://learn.microsoft.com/en-us/dotnet/csharp/) - Language syntax and features

### Books
- **"Domain-Driven Design: Tackling Complexity in the Heart of Software"** by Eric Evans (2003) - The original DDD book ("Blue Book"); still the definitive reference
- **"Implementing Domain-Driven Design"** by Vaughn Vernon (2013) - Practical DDD guide ("Red Book") with running case study
- **"Domain-Driven Design Distilled"** by Vaughn Vernon (2016) - Concise introduction to DDD, ideal for beginners before tackling the longer texts
- **"Patterns of Enterprise Application Architecture"** by Martin Fowler (2002) - Comprehensive catalog of patterns for enterprise applications (Repository, Unit of Work, Data Mapper, Service Layer, etc.); foundational reference for the patterns used in this codebase
- **"Refactoring: Improving the Design of Existing Code"** by Martin Fowler (1999, 2nd ed. 2018) - Definitive guide to systematic code improvement with catalog of refactoring techniques
- **"Analysis Patterns: Reusable Object Models"** by Martin Fowler (1996) - Common domain model structures repeated across enterprise applications
- **"Domain-Specific Languages"** by Martin Fowler (2010) - How to design and implement custom languages for specific problem domains
- **"Clean Architecture: A Craftsman's Guide to Software Structure and Design"** by Robert C. Martin - Architecture principles and dependency rule
- **"C# 12 in a Nutshell: The Definitive Reference"** by Joseph Albahari - Complete, up-to-date C# reference covering modern features
- **"ASP.NET Core in Action"** by Andrew Lock - Practical guide to ASP.NET Core, minimal APIs, and modern patterns

### Online Resources
- [Microsoft Learn - .NET Training](https://learn.microsoft.com/en-us/training/dotnet/) - Official interactive learning paths for C# and .NET
- [Refactoring Guru - Design Patterns](https://refactoring.guru/design-patterns) - Visual guide to design patterns with code examples
- [Martin Fowler's Website](https://martinfowler.com) - Extensive articles on software architecture, refactoring, agile, microservices, and more
- [EAA Catalog - Patterns of Enterprise Application Architecture](https://martinfowler.com/eaaCatalog) - Brief summaries of all patterns from Fowler's PEAA book, including Repository, Unit of Work, Service Layer, and Data Transfer Object
- [DDD Community](https://dddcommunity.org/) - DDD resources, articles, and community discussions (note: archival content; best for historical context)
- [InfoQ DDD Mini-book](https://www.infoq.com/minibooks/domain-driven-design-quickly) - Free summary of Eric Evans' DDD book by Abel Avram and Floyd Marinescu
- [Domain Language DDD Reference](https://www.domainlanguage.com/ddd) - Free concise DDD pattern definitions and resources from Eric Evans' consultancy

### Key Concepts to Research
1. **Domain-Driven Design**: Bounded Contexts, Aggregates, Value Objects, Domain Events, Anti-Corruption Layer
2. **Clean Architecture**: Dependency Rule, Layered Architecture, Hexagonal Architecture, Ports & Adapters
3. **CQRS**: Command Query Responsibility Segregation, Event Sourcing, read/write model separation
4. **Design Patterns**: Repository, Unit of Work, Mediator, Middleware, Assembler, Service Layer
5. **REST API Design**: HTTP methods, status codes, HATEOAS, API versioning, Problem Details (RFC 9457)
6. **Security**: JWT authentication, OAuth2, BCrypt password hashing, authorization middleware, CORS
7. **Entity Framework Core**: DbContext, Fluent API, Value Converters, Interceptors, AsNoTracking, Migrations
8. **Testing**: Unit Testing, Integration Testing, Test Doubles, Mock Repositories
9. **DevOps**: Docker multi-stage builds, CI/CD pipelines, container orchestration, environment configuration
10. **C# Language Features**: Records, primary constructors, pattern matching, nullable reference types, file-scoped namespaces

### Exercises
1. **Add a new Bounded Context**: Create a new context (e.g., "Notifications") following the existing patterns, including domain model, application services, and infrastructure.
2. **Implement a new Command**: Add a command to update a user's password, following the CQRS pattern with proper validation and error handling.
3. **Add Validation Pipeline Behavior**: Implement a mediator pipeline behavior that validates commands before they reach the handler, using FluentValidation or custom validators.
4. **Write Unit Tests**: Create unit tests for a Command Service using mock repositories, testing both success and failure paths.
5. **Add Pagination**: Implement pagination for list endpoints (e.g., `GetAllUsersQuery`) with page size and offset parameters.
6. **Add Response Caching**: Implement response caching for query endpoints using ASP.NET Core's caching middleware or a distributed cache.
7. **Add Health Checks**: Implement health check endpoints that verify database connectivity and external service availability.
8. **Add Rate Limiting**: Implement rate limiting middleware to protect the API from abuse, using ASP.NET Core's built-in rate limiting.
9. **Implement a Domain Event Handler**: Add a handler for `TutorialCreatedEvent` that sends a notification or updates a search index.
10. **Refactor to Specification Pattern**: Implement the Specification pattern for complex query conditions, separating query logic from repository implementations.
