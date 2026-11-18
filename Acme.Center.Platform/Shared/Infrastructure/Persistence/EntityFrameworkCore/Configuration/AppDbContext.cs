using Acme.Center.Platform.Iam.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Acme.Center.Platform.Profiles.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Acme.Center.Platform.Publishing.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Acme.Center.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration.Extensions;
using Acme.Center.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Acme.Center.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

/// <summary>
///     Application database context for the Learning Center Platform
/// </summary>
/// <param name="options">
///     The options for the database context
/// </param>
public class AppDbContext(DbContextOptions options) : DbContext(options)
{
    /// <inheritdoc />
    protected override void OnConfiguring(DbContextOptionsBuilder builder)
    {
        // Apply audit timestamp interceptor for all IAuditableEntity implementations
        builder.AddInterceptors(new AuditableEntityInterceptor());
        base.OnConfiguring(builder);
    }

    /// <summary>
    ///     On creating the database model
    /// </summary>
    /// <remarks>
    ///     This method is used to create the database model for the application.
    /// </remarks>
    /// <param name="builder">
    ///     The model builder for the database context
    /// </param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Publishing Context
        builder.ApplyPublishingConfiguration();

        // Profiles Context
        builder.ApplyProfilesConfiguration();

        // IAM Context
        builder.ApplyIamConfiguration();

        // General Naming Convention for the database objects
        builder.UseSnakeCaseNamingConvention();
    }
}