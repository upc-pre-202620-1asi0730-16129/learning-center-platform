using Acme.Center.Platform.Publishing.Domain.Model.Commands;
using Acme.Center.Platform.Publishing.Domain.Model.Entities;
using Acme.Center.Platform.Shared.Application.Model;

namespace Acme.Center.Platform.Publishing.Application.CommandServices;

/// <summary>
///     Represents the category command service in the ACME Learning Center Platform.
/// </summary>
public interface ICategoryCommandService
{
    /// <summary>
    ///     Handles the create category command in the ACME Learning Center Platform.
    /// </summary>
    /// <param name="command">
    ///     The <see cref="CreateCategoryCommand" /> command to handle.
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The created <see cref="Category" /> entity.
    /// </returns>
    public Task<Result<Category>> Handle(CreateCategoryCommand command, CancellationToken cancellationToken);
}