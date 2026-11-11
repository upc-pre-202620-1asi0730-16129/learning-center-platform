using Acme.Center.Platform.Profiles.Domain.Model.Aggregates;
using Acme.Center.Platform.Profiles.Interfaces.Rest.Resources;

// Added for ArgumentNullException

namespace Acme.Center.Platform.Profiles.Interfaces.Rest.Transform;

/// <summary>
///     Assembler responsible for transforming a <see cref="Profile" /> aggregate into a <see cref="ProfileResource" />.
/// </summary>
public static class ProfileResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts a <see cref="Profile" /> aggregate to its <see cref="ProfileResource" /> representation.
    /// </summary>
    /// <param name="entity">
    ///     The <see cref="Profile" /> aggregate to convert. Must not be null.
    /// </param>
    /// <returns>
    ///     A <see cref="ProfileResource" /> object representing the provided profile.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the input <paramref name="entity" /> is null.</exception>
    public static ProfileResource ToResourceFromEntity(Profile entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity),
                "Profile entity cannot be null when converting to resource.");
        return new ProfileResource(entity.Id, entity.FullName, entity.EmailAddress, entity.StreetAddress);
    }
}