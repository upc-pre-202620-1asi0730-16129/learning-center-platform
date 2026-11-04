using Acme.Center.Platform.Publishing.Domain.Model.Commands;
using Acme.Center.Platform.Publishing.Interfaces.Rest.Resources;

// Added for ArgumentNullException

namespace Acme.Center.Platform.Publishing.Interfaces.Rest.Transform;

/// <summary>
///     Assembler responsible for transforming a <see cref="CreateTutorialResource" /> into a
///     <see cref="CreateTutorialCommand" />.
/// </summary>
public static class CreateTutorialCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts a <see cref="CreateTutorialResource" /> to a <see cref="CreateTutorialCommand" />.
    /// </summary>
    /// <param name="resource">
    ///     The <see cref="CreateTutorialResource" /> containing the data for creating a tutorial. Must not be null.
    /// </param>
    /// <returns>
    ///     A new <see cref="CreateTutorialCommand" /> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the input <paramref name="resource" /> is null.</exception>
    public static CreateTutorialCommand ToCommandFromResource(CreateTutorialResource resource)
    {
        if (resource == null)
            throw new ArgumentNullException(nameof(resource),
                "CreateTutorialResource cannot be null when converting to command.");
        return new CreateTutorialCommand(resource.Title, resource.Summary, resource.CategoryId);
    }
}