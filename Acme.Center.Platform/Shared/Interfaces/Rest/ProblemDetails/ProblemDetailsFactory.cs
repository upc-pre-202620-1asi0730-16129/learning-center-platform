using Acme.Center.Platform.Resources.Errors;
using Acme.Center.Platform.Resources.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
// For base ProblemDetailsFactory
// For ErrorMessages
// For Shared.Commons

// For StatusCodes

namespace Acme.Center.Platform.Shared.Interfaces.Rest.ProblemDetails;

public class ProblemDetailsFactory
{
    private readonly Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory
        _aspNetCoreProblemDetailsFactory; // Corrected type and name

    private readonly IStringLocalizer<CommonMessages> _commonLocalizer; // Corrected to Commons
    private readonly IStringLocalizer<ErrorMessages> _errorLocalizer;

    public ProblemDetailsFactory(
        IStringLocalizer<ErrorMessages> errorLocalizer,
        IStringLocalizer<CommonMessages> commonLocalizer, // Corrected to Commons
        Microsoft.AspNetCore.Mvc.Infrastructure.ProblemDetailsFactory
            aspNetCoreProblemDetailsFactory) // Corrected injected type
    {
        _errorLocalizer = errorLocalizer;
        _commonLocalizer = commonLocalizer;
        _aspNetCoreProblemDetailsFactory = aspNetCoreProblemDetailsFactory; // Corrected assignment
    }

    public IActionResult CreateProblemDetails(
        ControllerBase controller,
        int statusCode,
        Enum? errorEnum, // The specific error enum (IamError, ProfilesError, etc.)
        string detailMessage) // The localized message from the application service
    {
        // Leverage the base ProblemDetailsFactory for initial creation
        var problemDetails = _aspNetCoreProblemDetailsFactory.CreateProblemDetails( // Corrected usage
            controller.HttpContext,
            statusCode,
            errorEnum != null ? _errorLocalizer[$"{errorEnum}"] : _commonLocalizer["GenericError"],
            detail: detailMessage
        );

        // Ensure problemDetails is not null (shouldn't be with default factory)
        if (problemDetails == null)
        {
            problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = statusCode,
                Title = errorEnum != null ? _errorLocalizer[$"{errorEnum}"] : _commonLocalizer["GenericError"],
                Detail = detailMessage,
                Instance = controller.HttpContext.Request.Path
            };
        }
        else
        {
            problemDetails.Title =
                errorEnum != null ? _errorLocalizer[$"{errorEnum}"] : _commonLocalizer["GenericError"];
            problemDetails.Detail = detailMessage;
            problemDetails.Instance = controller.HttpContext.Request.Path;
        }

        return controller.StatusCode(statusCode, problemDetails);
    }

    // Overload for when there's no specific error enum, just a generic message
    public IActionResult CreateProblemDetails(
        ControllerBase controller,
        int statusCode,
        string titleKey, // Key for localized title
        string detailKey, // Key for localized detail
        params object[] detailArgs)
    {
        var problemDetails = _aspNetCoreProblemDetailsFactory.CreateProblemDetails( // Corrected usage
            controller.HttpContext,
            statusCode,
            _commonLocalizer[titleKey],
            detail: _errorLocalizer[detailKey, detailArgs],
            instance: controller.HttpContext.Request.Path
        );
        return controller.StatusCode(statusCode, problemDetails);
    }
}