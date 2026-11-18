namespace Acme.Center.Platform.Iam.Interfaces.Acl;

public interface IIamContextFacade
{
    Task<int> CreateUser(string username, string password, CancellationToken cancellationToken);
    Task<int> FetchUserIdByUsername(string username, CancellationToken cancellationToken);
    Task<string> FetchUsernameByUserId(int userId, CancellationToken cancellationToken);
}