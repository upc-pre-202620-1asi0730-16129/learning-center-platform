using Acme.Center.Platform.Profiles.Domain.Model.Aggregates;
using Acme.Center.Platform.Profiles.Domain.Model.Queries;

namespace Acme.Center.Platform.Profiles.Application.QueryServices;

/// <summary>
///     Profile query service
/// </summary>
public interface IProfileQueryService
{
    /// <summary>
    ///     Handle get all profiles
    /// </summary>
    /// <param name="query">
    ///     The <see cref="GetAllProfilesQuery" /> query
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A list of <see cref="Profile" /> objects
    /// </returns>
    Task<IEnumerable<Profile>> Handle(GetAllProfilesQuery query, CancellationToken cancellationToken);

    /// <summary>
    ///     Handle get profile by email
    /// </summary>
    /// <param name="query">
    ///     The <see cref="GetProfileByEmailQuery" /> query
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A <see cref="Profile" /> object or null
    /// </returns>
    Task<Profile?> Handle(GetProfileByEmailQuery query, CancellationToken cancellationToken);

    /// <summary>
    ///     Handle get profile by id
    /// </summary>
    /// <param name="query">
    ///     The <see cref="GetProfileByIdQuery" /> query
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     <
    ///         A
    ///     <see cref="Profile" /> object or null
    ///     /returns>
    Task<Profile?> Handle(GetProfileByIdQuery query, CancellationToken cancellationToken);
}