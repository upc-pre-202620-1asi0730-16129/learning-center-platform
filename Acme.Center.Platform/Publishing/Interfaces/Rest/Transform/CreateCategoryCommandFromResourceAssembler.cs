using Acme.Center.Platform.Publishing.Domain.Model.Commands;
using Acme.Center.Platform.Publishing.Interfaces.Rest.Resources;

// Added for ArgumentNullException

namespace Acme.Center.Platform.Publishing.Interfaces.Rest.Transform;

/// <summary>
///     Assembler responsible for transforming a <see cref="CreateCategoryResource" /> into a
///     <see cref="CreateCategoryCommand" />.
/// </summary>
public static class CreateCategoryCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts a <see cref="CreateCategoryResource" /> to a <see cref="CreateCategoryCommand" />.
    /// </summary>
    /// <param name="resource">
    ///     The <see cref="CreateCategoryResource" /> containing the data for creating a category. Must not be null.
    /// </param>
    /// <returns>
    ///     A new <see cref="CreateCategoryCommand" /> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the input <paramref name="resource" /> is null.</exception>
    public static CreateCategoryCommand ToCommandFromResource(CreateCategoryResource resource)
    {
        if (resource == null)
            throw new ArgumentNullException(nameof(resource),
                "CreateCategoryResource cannot be null when converting to command.");
        return new CreateCategoryCommand(resource.Name);
    }
}