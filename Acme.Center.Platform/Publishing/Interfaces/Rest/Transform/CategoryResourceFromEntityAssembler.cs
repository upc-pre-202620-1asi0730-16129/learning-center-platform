using Acme.Center.Platform.Publishing.Domain.Model.Entities;
using Acme.Center.Platform.Publishing.Interfaces.Rest.Resources;

// Added for ArgumentNullException

namespace Acme.Center.Platform.Publishing.Interfaces.Rest.Transform;

/// <summary>
///     Assembler responsible for transforming a <see cref="Category" /> domain entity into a
///     <see cref="CategoryResource" /> for REST representation.
/// </summary>
public static class CategoryResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts a <see cref="Category" /> domain entity to its <see cref="CategoryResource" /> representation.
    /// </summary>
    /// <param name="entity">
    ///     The <see cref="Category" /> entity to convert. Must not be null.
    /// </param>
    /// <returns>
    ///     A <see cref="CategoryResource" /> object representing the provided category.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the input <paramref name="entity" /> is null.</exception>
    public static CategoryResource ToResourceFromEntity(Category entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity),
                "Category entity cannot be null when converting to resource.");
        return new CategoryResource(entity.Id, entity.Name);
    }
}