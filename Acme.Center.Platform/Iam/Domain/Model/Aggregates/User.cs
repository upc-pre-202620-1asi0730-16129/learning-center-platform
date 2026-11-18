using System.Text.Json.Serialization;
using Acme.Center.Platform.Iam.Domain.Model.Events;
using Acme.Center.Platform.Iam.Domain.Model.ValueObjects;
using Acme.Center.Platform.Shared.Domain.Model;
using Acme.Center.Platform.Shared.Domain.Model.Aggregates.Internal;

namespace Acme.Center.Platform.Iam.Domain.Model.Aggregates;

/**
 * <summary>
 *     The user aggregate
 * </summary>
 * <remarks>
 *     This class is used to represent a user
 * </remarks>
 */
public partial class User : AggregateRoot
{
    public User() : this(string.Empty, string.Empty)
    {
    }

    public User(string username, string passwordHash)
    {
        Username = username;
        PasswordHash = passwordHash;

        if (!string.IsNullOrWhiteSpace(username))
        {
            AddDomainEvent(new UserRegisteredEvent(Id, username));
        }
    }

    public UserId Id { get; }
    public string Username { get; private set; }

    [JsonIgnore] public string PasswordHash { get; private set; }

    /**
     * <summary>
     *     Update the username
     * </summary>
     * <param name="username">The new username</param>
     * <returns>The updated user</returns>
     */
    public User UpdateUsername(string username)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        Username = username;
        return this;
    }

    /**
     * <summary>
     *     Update the password hash
     * </summary>
     * <param name="passwordHash">The new password hash</param>
     * <returns>The updated user</returns>
     */
    public User UpdatePasswordHash(string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        PasswordHash = passwordHash;
        return this;
    }
}