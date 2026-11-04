namespace Acme.Center.Platform.Publishing.Interfaces.Rest.Resources;

/// <summary>
///     Category resource for REST API
/// </summary>
/// <param name="Id">
///     The unique identifier of the category
/// </param>
/// <param name="Name">
///     The name of the category
/// </param>
public record CategoryResource(int Id, string Name);