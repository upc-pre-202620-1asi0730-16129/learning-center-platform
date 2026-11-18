namespace Acme.Center.Platform.Iam.Interfaces.Rest.Resources;

public record AuthenticatedUserResource(int Id, string Username, string Token);