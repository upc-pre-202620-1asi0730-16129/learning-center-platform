using Acme.Center.Platform.Profiles.Application.CommandServices;
using Acme.Center.Platform.Profiles.Domain.Model.Aggregates;
using Acme.Center.Platform.Profiles.Domain.Model.Commands;
using Acme.Center.Platform.Profiles.Domain.Repositories;
using Acme.Center.Platform.Shared.Application.Model;
using Acme.Center.Platform.Shared.Domain.Repositories;

namespace Acme.Center.Platform.Profiles.Application.Internal.CommandServices;

/// <summary>
///     Profile command service
/// </summary>
/// <param name="profileRepository">
///     Profile repository
/// </param>
/// <param name="unitOfWork">
///     Unit of work
/// </param>
public class ProfileCommandService(
    IProfileRepository profileRepository,
    IUnitOfWork unitOfWork)
    : IProfileCommandService
{
    /// <inheritdoc />
    public async Task<Result<Profile>> Handle(CreateProfileCommand command, CancellationToken cancellationToken)
    {
        var profile = new Profile(command);
        await profileRepository.AddAsync(profile, cancellationToken);
        await unitOfWork.CompleteAsync(cancellationToken);
        return Result<Profile>.Success(profile);
    }
}
