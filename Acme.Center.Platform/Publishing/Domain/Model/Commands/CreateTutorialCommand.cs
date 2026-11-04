using Acme.Center.Platform.Publishing.Domain.Model.ValueObjects;

namespace Acme.Center.Platform.Publishing.Domain.Model.Commands;

/// <summary>
///     Command to create a tutorial.
/// </summary>
/// <param name="Title">
///     The title of the tutorial to create.
/// </param>
/// <param name="Summary">
///     The summary of the tutorial to create.
/// </param>
/// <param name="CategoryId">
///     The ID of the category for the tutorial.
/// </param>
public record CreateTutorialCommand(string Title, string Summary, CategoryId CategoryId)
{
    public CreateTutorialCommand(string title, string summary, int categoryId)
        : this(title, summary, new CategoryId(categoryId))
    {
    }
}