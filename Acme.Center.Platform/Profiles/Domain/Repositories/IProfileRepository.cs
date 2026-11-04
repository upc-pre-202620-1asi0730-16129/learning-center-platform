using Acme.Center.Platform.Profiles.Domain.Model.Aggregates;
using Acme.Center.Platform.Profiles.Domain.Model.ValueObjects;
using Acme.Center.Platform.Shared.Domain.Repositories;

namespace Acme.Center.Platform.Profiles.Domain.Repositories;

/// <summary>
///     Profile repository interface
/// </summary>
public interface IProfileRepository : IBaseRepository<Profile>
{
    /// <summary>
    ///     Find a profile by email
    /// </summary>
    /// <param name="email">
    ///     The <see cref="EmailAddress" /> email address to search for
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The <see cref="Profile" /> if found, otherwise null
    /// </returns>
    Task<Profile?> FindProfileByEmailAsync(EmailAddress email, CancellationToken cancellationToken);

    /// <summary>
    ///     Find a profile by email without change tracking
    /// </summary>
    /// <param name="email">
    ///     The <see cref="EmailAddress" /> email address to search for
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The <see cref="Profile" /> if found, otherwise null
    /// </returns>
    Task<Profile?> FindProfileByEmailAsNoTrackingAsync(EmailAddress email, CancellationToken cancellationToken);

    /// <summary>
    ///     Find a profile by id without change tracking
    /// </summary>
    /// <param name="id">
    ///     The <see cref="ProfileId" /> to search for
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The <see cref="Profile" /> if found, otherwise null
    /// </returns>
    Task<Profile?> FindByIdAsNoTrackingAsync(ProfileId id, CancellationToken cancellationToken);
}