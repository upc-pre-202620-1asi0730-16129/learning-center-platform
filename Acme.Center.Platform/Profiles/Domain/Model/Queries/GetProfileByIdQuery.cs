using Acme.Center.Platform.Profiles.Domain.Model.ValueObjects;

namespace Acme.Center.Platform.Profiles.Domain.Model.Queries;

public record GetProfileByIdQuery(ProfileId ProfileId)
{
    public GetProfileByIdQuery(int profileId) : this(new ProfileId(profileId))
    {
    }
}