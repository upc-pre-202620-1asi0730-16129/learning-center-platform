using Acme.Center.Platform.Iam.Domain.Model.ValueObjects;

namespace Acme.Center.Platform.Iam.Domain.Model.Queries;

/**
 * <summary>
 *     The get user by id query
 * </summary>
 * <remarks>
 *     This query object includes the user id to search
 * </remarks>
 */
public record GetUserByIdQuery(UserId Id)
{
    public GetUserByIdQuery(int id) : this(new UserId(id))
    {
    }
}