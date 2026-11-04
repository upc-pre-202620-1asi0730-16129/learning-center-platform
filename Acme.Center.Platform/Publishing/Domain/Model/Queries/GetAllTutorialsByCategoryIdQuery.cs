using Acme.Center.Platform.Publishing.Domain.Model.ValueObjects;

namespace Acme.Center.Platform.Publishing.Domain.Model.Queries;

/// <summary>
///     Represents a query to get all tutorials by category id in the ACME Learning Center Platform.
/// </summary>
/// <param name="CategoryId">
///     The id of the category to get tutorials for
/// </param>
public record GetAllTutorialsByCategoryIdQuery(CategoryId CategoryId)
{
    public GetAllTutorialsByCategoryIdQuery(int categoryId) : this(new CategoryId(categoryId))
    {
    }
}