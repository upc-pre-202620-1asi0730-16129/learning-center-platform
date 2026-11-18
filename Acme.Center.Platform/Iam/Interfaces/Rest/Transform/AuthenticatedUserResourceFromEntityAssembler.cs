using Acme.Center.Platform.Iam.Domain.Model.Aggregates;
using Acme.Center.Platform.Iam.Interfaces.Rest.Resources;

// Added for ArgumentNullException

namespace Acme.Center.Platform.Iam.Interfaces.Rest.Transform;

/// <summary>
///     Assembler responsible for transforming a <see cref="User" /> aggregate and a JWT token into an
///     <see cref="AuthenticatedUserResource" />.
/// </summary>
public static class AuthenticatedUserResourceFromEntityAssembler
{
    /// <summary>
    ///     Converts a <see cref="User" /> aggregate and a JWT token to an <see cref="AuthenticatedUserResource" />.
    /// </summary>
    /// <param name="user">
    ///     The <see cref="User" /> aggregate representing the authenticated user. Must not be null.
    /// </param>
    /// <param name="token">
    ///     The JWT token generated for the authenticated user. Must not be null or empty.
    /// </param>
    /// <returns>
    ///     A new <see cref="AuthenticatedUserResource" /> instance.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown if <paramref name="user" /> is null or <paramref name="token" /> is
    ///     null.
    /// </exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="token" /> is empty.</exception>
    public static AuthenticatedUserResource ToResourceFromEntity(
        User user, string token)
    {
        if (user == null)
            throw new ArgumentNullException(nameof(user),
                "User aggregate cannot be null when creating authenticated user resource.");
        if (string.IsNullOrEmpty(token))
            throw new ArgumentException("Token cannot be null or empty when creating authenticated user resource.",
                nameof(token));
        return new AuthenticatedUserResource(user.Id, user.Username, token);
    }
}