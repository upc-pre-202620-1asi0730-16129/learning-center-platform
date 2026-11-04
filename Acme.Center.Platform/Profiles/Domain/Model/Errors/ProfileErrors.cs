using Acme.Center.Platform.Shared.Domain.Model;
using Acme.Center.Platform.Shared.Domain.Model.Errors;

namespace Acme.Center.Platform.Profiles.Domain.Model.Errors;

public static class ProfileErrors
{
    public static readonly Error ProfileNotFound =
        Error.NotFound("Profiles.ProfileNotFound", "The specified profile was not found.");

    public static readonly Error EmailAlreadyRegistered =
        Error.Conflict("Profiles.EmailAlreadyRegistered", "A profile with this email address already exists.");

    public static readonly Error InvalidProfileData =
        Error.Validation("Profiles.InvalidProfileData", "The profile data provided is invalid.");

    public static readonly Error ProfileCreationFailed =
        Error.Failure("Profiles.ProfileCreationFailed", "An error occurred while creating the profile.");
}
