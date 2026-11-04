namespace Acme.Center.Platform.Shared.Domain.Repositories;

/// <summary>
///     Unit of work interface for all repositories
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    ///     Save changes to the repository
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns></returns>
    Task CompleteAsync(CancellationToken cancellationToken = default);
}