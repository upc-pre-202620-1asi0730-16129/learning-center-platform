namespace Acme.Center.Platform.Publishing.Interfaces.Rest.Resources;

/// <summary>
///     Tutorial resource for REST API
/// </summary>
/// <param name="Id">
///     The unique identifier of the tutorial
/// </param>
/// <param name="Title">
///     The title of the tutorial
/// </param>
/// <param name="Summary">
///     The summary of the tutorial
/// </param>
/// <param name="Category">
///     The category of the tutorial
/// </param>
/// <param name="Status">
///     The status of the tutorial
/// </param>
public record TutorialResource(int Id, string Title, string Summary, CategoryResource Category, string Status);