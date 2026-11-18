using Acme.Center.Platform.Iam.Domain.Model.Aggregates;
using Acme.Center.Platform.Iam.Domain.Model.ValueObjects;
using Acme.Center.Platform.Shared.Domain.Repositories;

namespace Acme.Center.Platform.Iam.Domain.Repositories;

/**
 * <summary>
 *     The user repository
 * </summary>
 * <remarks>
 *     This repository is used to manage users
 * </remarks>
 */
public interface IUserRepository : IBaseRepository<User>
{
    /**
     * <summary>
     *     Find a user by id
     * </summary>
     * <param name="username">The username to search</param>
     * <param name="cancellationToken">The cancellation token</param>
     * <returns>The user</returns>
     */
    Task<User?> FindByUsernameAsync(string username, CancellationToken cancellationToken);

    /**
     * <summary>
     *     Find a user by username without change tracking
     * </summary>
     * <param name="username">The username to search</param>
     * <param name="cancellationToken">The cancellation token</param>
     * <returns>The user</returns>
     */
    Task<User?> FindByUsernameAsNoTrackingAsync(string username, CancellationToken cancellationToken);

    /**
     * <summary>
     *     Find a user by id without change tracking
     * </summary>
     * <param name="id">The user id to search</param>
     * <param name="cancellationToken">The cancellation token</param>
     * <returns>The user</returns>
     */
    Task<User?> FindByIdAsNoTrackingAsync(UserId id, CancellationToken cancellationToken);

    /**
     * <summary>
     *     Check if a user exists by username
     * </summary>
     * <param name="username">The username to search</param>
     * <param name="cancellationToken">The cancellation token</param>
     * <returns>True if the user exists, false otherwise</returns>
     */
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken);
}