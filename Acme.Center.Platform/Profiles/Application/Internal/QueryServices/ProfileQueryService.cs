using Acme.Center.Platform.Profiles.Application.QueryServices;
using Acme.Center.Platform.Profiles.Domain.Model.Aggregates;
using Acme.Center.Platform.Profiles.Domain.Model.Queries;
using Acme.Center.Platform.Profiles.Domain.Repositories;

namespace Acme.Center.Platform.Profiles.Application.Internal.QueryServices;

/// <summary>
///     Profile query service
/// </summary>
/// <param name="profileRepository">
///     Profile repository
/// </param>
public class ProfileQueryService(IProfileRepository profileRepository) : IProfileQueryService
{
    /// <inheritdoc />
    public async Task<IEnumerable<Profile>> Handle(GetAllProfilesQuery query, CancellationToken cancellationToken)
    {
        return await profileRepository.ListAsNoTrackingAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Profile?> Handle(GetProfileByEmailQuery query, CancellationToken cancellationToken)
    {
        return await profileRepository.FindProfileByEmailAsNoTrackingAsync(query.Email, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Profile?> Handle(GetProfileByIdQuery query, CancellationToken cancellationToken)
    {
        return await profileRepository.FindByIdAsNoTrackingAsync(query.ProfileId, cancellationToken);
    }
}