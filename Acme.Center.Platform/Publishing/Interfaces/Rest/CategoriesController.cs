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
[Tags("Available Category Endpoints.")]
public class CategoriesController(
    ICategoryCommandService categoryCommandService,
    ICategoryQueryService categoryQueryService,
    IStringLocalizer<ErrorMessages> errorLocalizer, // Renamed for clarity
    ProblemDetailsFactory problemDetailsFactory) // Inject ProblemDetailsFactory
    : ControllerBase
{
    private readonly IStringLocalizer<ErrorMessages> _errorLocalizer = errorLocalizer;
    private readonly ProblemDetailsFactory _problemDetailsFactory = problemDetailsFactory;

    [HttpGet("{categoryId:int}")]
    [EndpointSummary("Get all categories")]
    [EndpointDescription("Get all categories")]
    [ProducesResponseType(typeof(CategoryResource), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(int categoryId, CancellationToken cancellationToken)
    {
        var getCategoryByIdQuery = new GetCategoryByIdQuery(categoryId);
        var category = await categoryQueryService.Handle(getCategoryByIdQuery, cancellationToken);

        return PublishingActionResultAssembler.ToActionResultFromGetCategoryByIdResult(
            this,
            category,
            _errorLocalizer,
            _problemDetailsFactory,
            foundCategory => Ok(CategoryResourceFromEntityAssembler.ToResourceFromEntity(foundCategory))
        );
    }

    [HttpPost]
    [EndpointSummary("Create a new category")]
    [EndpointDescription("Create a new category")]
    [ProducesResponseType(typeof(CategoryResource), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryResource resource,
        CancellationToken cancellationToken)
    {
        var createCategoryCommand = CreateCategoryCommandFromResourceAssembler.ToCommandFromResource(resource);
        var result = await categoryCommandService.Handle(createCategoryCommand, cancellationToken);

        return PublishingActionResultAssembler.ToActionResultFromCreateCategoryResult(
            this,
            result,
            _errorLocalizer,
            _problemDetailsFactory,
            createdCategory => CreatedAtAction(nameof(GetCategoryById), new { categoryId = createdCategory.Id },
                CategoryResourceFromEntityAssembler.ToResourceFromEntity(createdCategory))
        );
    }

    [HttpGet]
    [EndpointSummary("Get all categories")]
    [EndpointDescription("Get all categories")]
    [ProducesResponseType(typeof(IEnumerable<CategoryResource>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllCategories(CancellationToken cancellationToken)
    {
        var getAllCategoriesQuery = new GetAllCategoriesQuery();
        var categories = await categoryQueryService.Handle(getAllCategoriesQuery, cancellationToken);
        var categoryResources = categories.Select(CategoryResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(categoryResources);
    }
}