using System.Net.Mime;
using Acme.Center.Platform.Publishing.Application.QueryServices;
using Acme.Center.Platform.Publishing.Domain.Model.Queries;
using Acme.Center.Platform.Publishing.Interfaces.Rest.Resources;
using Acme.Center.Platform.Publishing.Interfaces.Rest.Transform;
using Acme.Center.Platform.Resources.Errors;
using Acme.Center.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
// Corrected using directive
// For ProblemDetailsFactory
// Needed for the assembler

// For PublishingError enum

namespace Acme.Center.Platform.Publishing.Interfaces.Rest;

[ApiController]
[Route("api/v1/categories/{categoryId:int}/tutorials")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Categories")]
public class CategoryTutorialsController(
    ITutorialQueryService tutorialQueryService,
    ProblemDetailsFactory problemDetailsFactory, // Inject ProblemDetailsFactory
    IStringLocalizer<ErrorMessages> errorLocalizer) // Inject ErrorMessages localizer
    : ControllerBase
{
    private readonly IStringLocalizer<ErrorMessages> _errorLocalizer = errorLocalizer;
    private readonly ProblemDetailsFactory _problemDetailsFactory = problemDetailsFactory;

    /// <summary>
    ///     Get all tutorials by category id
    /// </summary>
    /// <param name="categoryId">
    ///     The category id
    /// </param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>
    ///     The list of <see cref="TutorialResource" /> tutorials
    /// </returns>
    [HttpGet]
    [EndpointSummary("Get all tutorials by category id")]
    [EndpointDescription("Get all tutorials by category id")]
    [ProducesResponseType(typeof(IEnumerable<TutorialResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTutorialsByCategoryId(int categoryId, CancellationToken cancellationToken)
    {
        var getTutorialsByCategoryIdQuery = new GetAllTutorialsByCategoryIdQuery(categoryId);
        var tutorials = await tutorialQueryService.Handle(getTutorialsByCategoryIdQuery, cancellationToken);

        return PublishingActionResultAssembler.ToActionResultFromGetAllTutorialsByCategoryIdResult(
            this,
            tutorials,
            _errorLocalizer, // Pass localizer
            _problemDetailsFactory, // Pass factory
            foundTutorials => Ok(foundTutorials.Select(TutorialResourceFromEntityAssembler.ToResourceFromEntity))
        );
    }
}