namespace Acme.Center.Platform.Publishing.Domain.Model.Commands;

/// <summary>
///     Command to create a category.
/// </summary>
/// <param name="Name">
///     The name of the category to create.
/// </param>
public record CreateCategoryCommand(string Name);