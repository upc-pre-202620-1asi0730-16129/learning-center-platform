using Acme.Center.Platform.Publishing.Domain.Model.Commands;
using Acme.Center.Platform.Publishing.Interfaces.Rest.Resources;

// Added for ArgumentNullException

namespace Acme.Center.Platform.Publishing.Interfaces.Rest.Transform;

/// <summary>
///     Assembler responsible for transforming an <see cref="AddVideoAssetToTutorialResource" /> into an
///     <see cref="AddVideoAssetToTutorialCommand" />.
/// </summary>
public static class AddVideoAssetToTutorialCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts an <see cref="AddVideoAssetToTutorialResource" /> to an <see cref="AddVideoAssetToTutorialCommand" />.
    /// </summary>
    /// <param name="resource">
    ///     The <see cref="AddVideoAssetToTutorialResource" /> containing the video asset data. Must not be null.
    /// </param>
    /// <param name="tutorialId">
    ///     The unique identifier of the tutorial to which the video asset will be added.
    /// </param>
    /// <returns>
    ///     A new <see cref="AddVideoAssetToTutorialCommand" /> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the input <paramref name="resource" /> is null.</exception>
    public static AddVideoAssetToTutorialCommand ToCommandFromResource(
        AddVideoAssetToTutorialResource resource, int tutorialId)
    {
        if (resource == null)
            throw new ArgumentNullException(nameof(resource),
                "AddVideoAssetToTutorialResource cannot be null when converting to command.");
        return new AddVideoAssetToTutorialCommand(resource.VideoUrl, tutorialId);
    }
}