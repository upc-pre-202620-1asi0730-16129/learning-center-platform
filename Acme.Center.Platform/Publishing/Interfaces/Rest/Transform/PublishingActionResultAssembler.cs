using Acme.Center.Platform.Publishing.Domain.Model;
using Acme.Center.Platform.Publishing.Domain.Model.Aggregates;
using Acme.Center.Platform.Publishing.Domain.Model.Entities;
using Acme.Center.Platform.Resources.Errors;
using Acme.Center.Platform.Shared.Application.Model;
using Acme.Center.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

// For PublishingError
// For ProblemDetailsFactory

// For StatusCodes

namespace Acme.Center.Platform.Publishing.Interfaces.Rest.Transform;

public static class PublishingActionResultAssembler
{
    // --- Helper for transforming PublishingError to StatusCode ---
    private static int ToStatusCodeFromPublishingError(PublishingError error)
    {
        return error switch
        {
            PublishingError.CategoryNotFound => StatusCodes.Status404NotFound,
            PublishingError.TutorialNotFound => StatusCodes.Status404NotFound,
            PublishingError.DuplicateTutorialTitle => StatusCodes.Status409Conflict,
            PublishingError.OperationCancelled => StatusCodes.Status409Conflict,
            PublishingError.DatabaseError => StatusCodes.Status500InternalServerError,
            PublishingError.InternalServerError => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest // Default
        };
    }

    // --- Specific Assembler Methods ---

    public static IActionResult ToActionResultFromCreateCategoryResult(
        ControllerBase controller,
        Result<Category> result,
        IStringLocalizer<ErrorMessages> errorLocalizer,
        ProblemDetailsFactory problemDetailsFactory,
        Func<Category, IActionResult> successAction)
    {
        if (result.IsSuccess) return successAction(result.Value!);

        var statusCode = ToStatusCodeFromPublishingError((PublishingError)result.Error!);
        return problemDetailsFactory.CreateProblemDetails(controller, statusCode, result.Error, result.Message);
    }

    public static IActionResult ToActionResultFromGetCategoryByIdResult(
        ControllerBase controller,
        Category? category, // Direct category object, not a Result
        IStringLocalizer<ErrorMessages> errorLocalizer,
        ProblemDetailsFactory problemDetailsFactory,
        Func<Category, IActionResult> successAction)
    {
        if (category is null)
            return problemDetailsFactory.CreateProblemDetails(
                controller,
                ToStatusCodeFromPublishingError(PublishingError.CategoryNotFound),
                PublishingError.CategoryNotFound,
                errorLocalizer[nameof(PublishingError.CategoryNotFound)]
            );
        return successAction(category);
    }

    public static IActionResult ToActionResultFromCreateTutorialResult(
        ControllerBase controller,
        Result<Tutorial> result,
        IStringLocalizer<ErrorMessages> errorLocalizer,
        ProblemDetailsFactory problemDetailsFactory,
        Func<Tutorial, IActionResult> successAction)
    {
        if (result.IsSuccess) return successAction(result.Value!);

        var statusCode = ToStatusCodeFromPublishingError((PublishingError)result.Error!);
        return problemDetailsFactory.CreateProblemDetails(controller, statusCode, result.Error, result.Message);
    }

    public static IActionResult ToActionResultFromAddVideoAssetToTutorialResult(
        ControllerBase controller,
        Result<Tutorial> result,
        IStringLocalizer<ErrorMessages> errorLocalizer,
        ProblemDetailsFactory problemDetailsFactory,
        Func<Tutorial, IActionResult> successAction)
    {
        if (result.IsSuccess) return successAction(result.Value!);

        var statusCode = ToStatusCodeFromPublishingError((PublishingError)result.Error!);
        return problemDetailsFactory.CreateProblemDetails(controller, statusCode, result.Error, result.Message);
    }

    public static IActionResult ToActionResultFromGetTutorialByIdResult(
        ControllerBase controller,
        Tutorial? tutorial, // Direct tutorial object, not a Result
        IStringLocalizer<ErrorMessages> errorLocalizer,
        ProblemDetailsFactory problemDetailsFactory,
        Func<Tutorial, IActionResult> successAction)
    {
        if (tutorial is null)
            return problemDetailsFactory.CreateProblemDetails(
                controller,
                ToStatusCodeFromPublishingError(PublishingError.TutorialNotFound),
                PublishingError.TutorialNotFound,
                errorLocalizer[nameof(PublishingError.TutorialNotFound)]
            );
        return successAction(tutorial);
    }

    public static IActionResult ToActionResultFromGetAllTutorialsByCategoryIdResult(
        ControllerBase controller,
        IEnumerable<Tutorial> tutorials, // Direct collection, not a Result
        IStringLocalizer<ErrorMessages> errorLocalizer, // Still need localizer for potential future errors
        ProblemDetailsFactory problemDetailsFactory, // Still need factory for potential future errors
        Func<IEnumerable<Tutorial>, IActionResult> successAction)
    {
        // Assuming an empty collection is a valid success, not an error.
        // If a specific error should be returned for an empty collection,
        // that logic would need to be added here or in the query service.
        return successAction(tutorials);
    }
}