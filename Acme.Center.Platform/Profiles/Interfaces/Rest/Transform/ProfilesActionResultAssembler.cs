using Acme.Center.Platform.Profiles.Domain.Model;
using Acme.Center.Platform.Profiles.Domain.Model.Aggregates;
using Acme.Center.Platform.Resources.Errors;
using Acme.Center.Platform.Shared.Application.Model;
using Acme.Center.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

// For ProfilesError
// For ProblemDetailsFactory

// For StatusCodes

namespace Acme.Center.Platform.Profiles.Interfaces.Rest.Transform;

public static class ProfilesActionResultAssembler
{
    // --- Helper for transforming ProfilesError to StatusCode ---
    private static int ToStatusCodeFromProfilesError(ProfilesError error)
    {
        return error switch
        {
            ProfilesError.ProfileNotFound => StatusCodes.Status404NotFound,
            ProfilesError.EmailAlreadyRegistered => StatusCodes.Status409Conflict,
            ProfilesError.InvalidProfileData => StatusCodes.Status400BadRequest,
            ProfilesError.OperationCancelled => StatusCodes.Status409Conflict,
            ProfilesError.DatabaseError => StatusCodes.Status500InternalServerError,
            ProfilesError.InternalServerError => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest // Default
        };
    }

    // --- Specific Assembler Methods ---

    public static IActionResult ToActionResultFromCreateProfileResult(
        ControllerBase controller,
        Result<Profile> result,
        IStringLocalizer<ErrorMessages> errorLocalizer,
        ProblemDetailsFactory problemDetailsFactory,
        Func<Profile, IActionResult> successAction)
    {
        if (result.IsSuccess) return successAction(result.Value!);

        var statusCode = ToStatusCodeFromProfilesError((ProfilesError)result.Error!);
        return problemDetailsFactory.CreateProblemDetails(controller, statusCode, result.Error, result.Message);
    }

    public static IActionResult ToActionResultFromGetProfileByIdResult(
        ControllerBase controller,
        Profile? profile, // Direct profile object, not a Result
        IStringLocalizer<ErrorMessages> errorLocalizer,
        ProblemDetailsFactory problemDetailsFactory,
        Func<Profile, IActionResult> successAction)
    {
        if (profile is null)
            return problemDetailsFactory.CreateProblemDetails(
                controller,
                ToStatusCodeFromProfilesError(ProfilesError.ProfileNotFound),
                ProfilesError.ProfileNotFound,
                errorLocalizer[nameof(ProfilesError.ProfileNotFound)]
            );
        return successAction(profile);
    }
}