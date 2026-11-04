using Acme.Center.Platform.Resources.Errors;
using Acme.Center.Platform.Resources.Shared;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Acme.Center.Platform.Shared.Infrastructure.Pipeline.Middleware.Components;

/// <summary>
///     Global exception handler implementing ASP.NET Core <see cref="IExceptionHandler" />.
/// </summary>
/// <remarks>
///     Catches unhandled exceptions and writes RFC 9457 compliant Problem Details responses.
/// </remarks>
public class GlobalExceptionHandler(
    ILogger<GlobalExceptionHandler> logger,
    IProblemDetailsService problemDetailsService,
    IStringLocalizer<ErrorMessages> errorLocalizer,
    IStringLocalizer<CommonMessages> commonLocalizer)
    : IExceptionHandler
{
    /**
     * <summary>
     *     Tries to handle the exception asynchronously and write a problem details response.
     * </summary>
     * <param name="httpContext">The HTTP context for the current request.</param>
     * <param name="exception">The unhandled exception.</param>
     * <param name="cancellationToken">The cancellation token.</param>
     * <returns>A value task containing true if the exception was handled, false otherwise.</returns>
     */
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is OperationCanceledException opEx)
        {
            logger.LogWarning(opEx, "Request was cancelled: {Message}", opEx.Message);
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = errorLocalizer["OperationCancelled"],
                Detail = errorLocalizer["OperationCancelled"],
                Instance = httpContext.Request.Path
            };

            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails,
                Exception = exception
            });
        }

        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        var genericProblemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = commonLocalizer["InternalServerError"],
            Detail = errorLocalizer["GenericError"],
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = genericProblemDetails,
            Exception = exception
        });
    }
}
