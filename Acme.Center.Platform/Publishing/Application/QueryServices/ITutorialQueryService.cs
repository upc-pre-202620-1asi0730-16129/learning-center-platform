using Acme.Center.Platform.Publishing.Domain.Model.Aggregates;
using Acme.Center.Platform.Publishing.Domain.Model.Queries;

namespace Acme.Center.Platform.Publishing.Application.QueryServices;

/// <summary>
///     Represents the tutorial query service in the ACME Learning Center Platform.
/// </summary>
public interface ITutorialQueryService
{
    /// <summary>
    ///     Handles the get tutorial by id query in the ACME Learning Center Platform.
    /// </summary>
    /// <param name="query">
    ///     The <see cref="GetTutorialByIdQuery" /> query to handle.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The <see cref="Tutorial" /> entity.
    /// </returns>
    Task<Tutorial?> Handle(GetTutorialByIdQuery query, CancellationToken cancellationToken);

    /// <summary>
    ///     Handles the get all tutorials query in the ACME Learning Center Platform.
    /// </summary>
    /// <param name="query">
    ///     The <see cref="GetAllTutorialsQuery" /> query to handle.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A collection of all tutorials in the platform.
    /// </returns>
    Task<IEnumerable<Tutorial>> Handle(GetAllTutorialsQuery query, CancellationToken cancellationToken);

    /// <summary>
    ///     Handles the get all tutorials by category id query in the ACME Learning Center Platform.
    /// </summary>
    /// <param name="query">
    ///     The <see cref="GetAllTutorialsByCategoryIdQuery" /> query to handle.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     A collection of tutorials that belong to the category.
    /// </returns>
    Task<IEnumerable<Tutorial>> Handle(GetAllTutorialsByCategoryIdQuery query, CancellationToken cancellationToken);
}