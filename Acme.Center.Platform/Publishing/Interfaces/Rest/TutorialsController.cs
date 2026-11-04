using System.Net.Mime;
using Acme.Center.Platform.Publishing.Application.CommandServices;
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

// For PublishingError enum

namespace Acme.Center.Platform.Publishing.Interfaces.Rest;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[Tags("Available Tutorial endpoints")]
public class TutorialsController(
    ITutorialQueryService tutorialQueryService,
    ITutorialCommandService tutorialCommandService,
    IStringLocalizer<ErrorMessages> errorLocalizer, // Renamed for clarity
    ProblemDetailsFactory problemDetailsFactory) // Inject ProblemDetailsFactory
    : ControllerBase
{
    private readonly IStringLocalizer<ErrorMessages> _errorLocalizer = errorLocalizer;
    private readonly ProblemDetailsFactory _problemDetailsFactory = problemDetailsFactory;

    [HttpGet("{tutorialId:int}")]
    [EndpointSummary("Get a tutorial by its id")]
    [EndpointDescription("Get a tutorial by its id")]
    [ProducesResponseType(typeof(TutorialResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTutorialById([FromRoute] int tutorialId, CancellationToken cancellationToken)
    {
        var getTutorialByIdQuery = new GetTutorialByIdQuery(tutorialId);
        var tutorial = await tutorialQueryService.Handle(getTutorialByIdQuery, cancellationToken);

        return PublishingActionResultAssembler.ToActionResultFromGetTutorialByIdResult(
            this,
            tutorial,
            _errorLocalizer,
            _problemDetailsFactory,
            foundTutorial => Ok(TutorialResourceFromEntityAssembler.ToResourceFromEntity(foundTutorial))
        );
    }

    [HttpPost]
    [EndpointSummary("Create a tutorial")]
    [EndpointDescription("Create a tutorial")]
    [ProducesResponseType(typeof(TutorialResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTutorial([FromBody] CreateTutorialResource resource,
        CancellationToken cancellationToken)
    {
        var createTutorialCommand = CreateTutorialCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await tutorialCommandService.Handle(createTutorialCommand, cancellationToken);

        return PublishingActionResultAssembler.ToActionResultFromCreateTutorialResult(
            this,
            result,
            _errorLocalizer,
            _problemDetailsFactory,
            createdTutorial => CreatedAtAction(nameof(GetTutorialById), new { tutorialId = createdTutorial.Id },
                TutorialResourceFromEntityAssembler.ToResourceFromEntity(createdTutorial))
        );
    }

    [HttpGet]
    [EndpointSummary("Get all tutorials")]
    [EndpointDescription("Get all tutorials")]
    [ProducesResponseType(typeof(IEnumerable<TutorialResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTutorials(CancellationToken cancellationToken)
    {
        var getAllTutorialsQuery = new GetAllTutorialsQuery();
        var tutorials = await tutorialQueryService.Handle(getAllTutorialsQuery, cancellationToken);
        var tutorialResources = tutorials.Select(TutorialResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(tutorialResources);
    }

    [HttpPost("{tutorialId:int}/videos")]
    [EndpointSummary("Add a video to a tutorial")]
    [EndpointDescription("Add a video to a tutorial")]
    [ProducesResponseType(typeof(TutorialResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddVideoToTutorial(
        [FromBody] AddVideoAssetToTutorialResource resource,
        [FromRoute] int tutorialId,
        CancellationToken cancellationToken)
    {
        var addVideoAssetToTutorialCommand =
            AddVideoAssetToTutorialCommandFromResourceAssembler.ToCommandFromResource(resource, tutorialId);
        var result = await tutorialCommandService.Handle(addVideoAssetToTutorialCommand, cancellationToken);

        return PublishingActionResultAssembler.ToActionResultFromAddVideoAssetToTutorialResult(
            this,
            result,
            _errorLocalizer,
            _problemDetailsFactory,
            updatedTutorial => CreatedAtAction(nameof(GetTutorialById), new { tutorialId = updatedTutorial.Id },
                TutorialResourceFromEntityAssembler.ToResourceFromEntity(updatedTutorial))
        );
    }
}