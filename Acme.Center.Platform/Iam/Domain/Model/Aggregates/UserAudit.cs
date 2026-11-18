using Acme.Center.Platform.Shared.Domain.Model.Entities;

namespace Acme.Center.Platform.Iam.Domain.Model.Aggregates;

public partial class User : IAuditableEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}