using Acme.Center.Platform.Profiles.Domain.Model.Aggregates;
using Acme.Center.Platform.Profiles.Domain.Model.Commands;
using Acme.Center.Platform.Shared.Application.Model;

namespace Acme.Center.Platform.Profiles.Application.CommandServices;

/// <summary>
///     Profile command service interface
/// </summary>
public interface IProfileCommandService
{
    /// <summary>
    ///     Handle create profile command
    /// </summary>
    /// <param name="command">
    ///     The <see cref="CreateProfileCommand" /> command
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The <see cref="Profile" /> object with the created profile
    /// </returns>
    Task<Result<Profile>> Handle(CreateProfileCommand command, CancellationToken cancellationToken);
}