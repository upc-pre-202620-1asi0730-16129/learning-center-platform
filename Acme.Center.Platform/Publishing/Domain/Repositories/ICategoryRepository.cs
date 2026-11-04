using Acme.Center.Platform.Publishing.Domain.Model.Entities;
using Acme.Center.Platform.Shared.Domain.Repositories;

namespace Acme.Center.Platform.Publishing.Domain.Repositories;

/// <summary>
///     Represents the category repository in the ACME Learning Center Platform.
/// </summary>
public interface ICategoryRepository : IBaseRepository<Category>
{
}