using Acme.Center.Platform.Shared.Domain.Model.Entities;

namespace Acme.Center.Platform.Profiles.Domain.Model.Aggregates;

public partial class Profile : IAuditableEntity
{
    public DateTimeOffset? CreatedAt { get; set; }
    public DateTimeOffset? UpdatedAt { get; set; }
}