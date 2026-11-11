using Acme.Center.Platform.Profiles.Domain.Model.Commands;
using Acme.Center.Platform.Profiles.Interfaces.Rest.Resources;

// Added for ArgumentNullException

namespace Acme.Center.Platform.Profiles.Interfaces.Rest.Transform;

/// <summary>
///     Assembler responsible for transforming a <see cref="CreateProfileResource" /> into a
///     <see cref="CreateProfileCommand" />.
/// </summary>
public static class CreateProfileCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts a <see cref="CreateProfileResource" /> to a <see cref="CreateProfileCommand" />.
    /// </summary>
    /// <param name="resource">
    ///     The <see cref="CreateProfileResource" /> containing the data for creating a profile. Must not be null.
    /// </param>
    /// <returns>
    ///     A new <see cref="CreateProfileCommand" /> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the input <paramref name="resource" /> is null.</exception>
    public static CreateProfileCommand ToCommandFromResource(CreateProfileResource resource)
    {
        if (resource == null)
            throw new ArgumentNullException(nameof(resource),
                "CreateProfileResource cannot be null when converting to command.");
        return new CreateProfileCommand(
            resource.FirstName,
            resource.LastName,
            resource.Email,
            resource.Street,
            resource.Number,
            resource.City,
            resource.PostalCode,
            resource.Country
        );
    }
}