using Acme.Center.Platform.Publishing.Domain.Model.Aggregates;
using Acme.Center.Platform.Publishing.Interfaces.Rest.Resources;
using Humanizer;

// Added for InvalidOperationException

namespace Acme.Center.Platform.Publishing.Interfaces.Rest.Transform;

/// <summary>
///     Assembler responsible for transforming a <see cref="Tutorial" /> domain aggregate into a
///     <see cref="TutorialResource" /> for REST representation.
/// </summary>
public static class TutorialResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts a <see cref="Tutorial" /> domain aggregate to its <see cref="TutorialResource" /> representation.
    /// </summary>
    /// <param name="entity">
    ///     The <see cref="Tutorial" /> aggregate to convert. Must not be null.
    /// </param>
    /// <returns>
    ///     A <see cref="TutorialResource" /> object representing the provided tutorial.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the input <paramref name="entity" /> is null.</exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the <see cref="Tutorial.Category" /> navigation property is null,
    ///     indicating an improperly loaded aggregate or data inconsistency, as Category is expected to be present.
    /// </exception>
    public static TutorialResource ToResourceFromEntity(Tutorial entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity),
                "Tutorial entity cannot be null when converting to resource.");

        // Although Category is non-nullable in the entity, if it's not eagerly loaded, it can be null at runtime.
        // This check ensures we don't pass a null to a method expecting a non-nullable argument.
        if (entity.Category == null)
            throw new InvalidOperationException(
                $"Tutorial with ID {entity.Id} has a null Category. Ensure Category is properly loaded.");

        return new TutorialResource(
            entity.Id,
            entity.Title,
            entity.Summary,
            CategoryResourceFromEntityAssembler.ToResourceFromEntity(entity.Category),
            entity.Status.Humanize());
    }
}