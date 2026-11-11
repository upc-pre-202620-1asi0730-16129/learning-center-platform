using Acme.Center.Platform.Profiles.Domain.Model.Aggregates;
using Acme.Center.Platform.Profiles.Domain.Model.ValueObjects;
using Acme.Center.Platform.Profiles.Domain.Repositories;
using Acme.Center.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;
using Acme.Center.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Acme.Center.Platform.Profiles.Infrastructure.Persistence.EntityFrameworkCore.Repositories;

/// <summary>
///     Profile repository implementation
/// </summary>
/// <param name="context">
///     The database context
/// </param>
public class ProfileRepository(AppDbContext context)
    : BaseRepository<Profile>(context), IProfileRepository
{
    /// <inheritdoc />
    public async Task<Profile?> FindProfileByEmailAsync(EmailAddress email, CancellationToken cancellationToken)
    {
        return await Context.Set<Profile>().FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Profile?> FindProfileByEmailAsNoTrackingAsync(EmailAddress email, CancellationToken cancellationToken)
    {
        return await Context.Set<Profile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Email == email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Profile?> FindByIdAsNoTrackingAsync(ProfileId id, CancellationToken cancellationToken)
    {
        return await Context.Set<Profile>()
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}