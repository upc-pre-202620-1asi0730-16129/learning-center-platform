using Acme.Center.Platform.Shared.Domain.Model;
using Acme.Center.Platform.Shared.Domain.Model.Errors;

namespace Acme.Center.Platform.Publishing.Domain.Model.Errors;

public static class PublishingErrors
{
    public static readonly Error CategoryCreationFailed =
        Error.Failure("Publishing.CategoryCreationFailed", "An error occurred while creating the category.");

    public static readonly Error TutorialNotFound =
        Error.NotFound("Publishing.TutorialNotFound", "The specified tutorial was not found.");

    public static readonly Error CategoryNotFound =
        Error.NotFound("Publishing.CategoryNotFound", "The specified category was not found.");

    public static readonly Error DuplicateTutorialTitle =
        Error.Conflict("Publishing.DuplicateTutorialTitle", "A tutorial with the same title already exists.");

    public static Error DuplicateTutorialTitleWithTitle(string title) =>
        Error.Conflict("Publishing.DuplicateTutorialTitle", $"A tutorial with the title '{title}' already exists.");
}
