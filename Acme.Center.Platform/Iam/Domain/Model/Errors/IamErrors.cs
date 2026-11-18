using Acme.Center.Platform.Shared.Domain.Model;
using Acme.Center.Platform.Shared.Domain.Model.Errors;

namespace Acme.Center.Platform.Iam.Domain.Model.Errors;

public static class IamErrors
{
    public static readonly Error InvalidCredentials =
        Error.Unauthorized("Iam.InvalidCredentials", "Invalid username or password.");

    public static readonly Error UsernameAlreadyTaken =
        Error.Conflict("Iam.UsernameAlreadyTaken", "The specified username is already taken.");

    public static readonly Error UserNotFound =
        Error.NotFound("Iam.UserNotFound", "The specified user was not found.");

    public static readonly Error UserCreationFailed =
        Error.Failure("Iam.UserCreationFailed", "An error occurred while creating the user.");
}
