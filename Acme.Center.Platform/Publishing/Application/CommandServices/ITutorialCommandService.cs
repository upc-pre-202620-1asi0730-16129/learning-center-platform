using Acme.Center.Platform.Publishing.Domain.Model.Aggregates;
using Acme.Center.Platform.Publishing.Domain.Model.Commands;
using Acme.Center.Platform.Shared.Application.Model;

namespace Acme.Center.Platform.Publishing.Application.CommandServices;

/// <summary>
///     Represents the tutorial command service in the ACME Learning Center Platform.
/// </summary>
public interface ITutorialCommandService
{
    /// <summary>
    ///     Handles the add video asset to tutorial command in the ACME Learning Center Platform.
    /// </summary>
    /// <param name="command">
    ///     The <see cref="AddVideoAssetToTutorialCommand" /> command to handle.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The updated <see cref="Tutorial" /> entity.
    /// </returns>
    Task<Result<Tutorial>> Handle(AddVideoAssetToTutorialCommand command, CancellationToken cancellationToken);

    /// <summary>
    ///     Handles the create tutorial command in the ACME Learning Center Platform.
    /// </summary>
    /// <param name="command">
    ///     The <see cref="CreateTutorialCommand" /> command to handle.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The created <see cref="Tutorial" /> entity.
    /// </returns>
    Task<Result<Tutorial>> Handle(CreateTutorialCommand command, CancellationToken cancellationToken);
}