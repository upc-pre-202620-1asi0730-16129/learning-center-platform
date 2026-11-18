using Acme.Center.Platform.Iam.Domain.Model.Commands;
using Acme.Center.Platform.Iam.Interfaces.Rest.Resources;

// Added for ArgumentNullException

namespace Acme.Center.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>
///     Assembler responsible for transforming a <see cref="SignInResource" /> into a <see cref="SignInCommand" />.
/// </summary>
public static class SignInCommandFromResourceAssembler
{
    /// <summary>
    ///     Converts a <see cref="SignInResource" /> to a <see cref="SignInCommand" />.
    /// </summary>
    /// <param name="resource">
    ///     The <see cref="SignInResource" /> containing the sign-in data. Must not be null.
    /// </param>
    /// <returns>
    ///     A new <see cref="SignInCommand" /> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown if the input <paramref name="resource" /> is null.</exception>
    public static SignInCommand ToCommandFromResource(SignInResource resource)
    {
        if (resource == null)
            throw new ArgumentNullException(nameof(resource),
                "SignInResource cannot be null when converting to command.");
        return new SignInCommand(resource.Username, resource.Password);
    }
}