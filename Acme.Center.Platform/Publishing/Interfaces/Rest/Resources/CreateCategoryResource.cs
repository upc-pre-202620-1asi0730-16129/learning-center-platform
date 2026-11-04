namespace Acme.Center.Platform.Publishing.Interfaces.Rest.Resources;

/// <summary>
///     Create category resource for REST API
/// </summary>
/// <param name="Name">
///     The name of the category
/// </param>
public record CreateCategoryResource(string Name);