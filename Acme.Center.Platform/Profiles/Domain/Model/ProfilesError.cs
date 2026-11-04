namespace Acme.Center.Platform.Profiles.Domain.Model;

public enum ProfilesError
{
    None,
    ProfileNotFound,
    EmailAlreadyRegistered,
    InvalidProfileData,
    OperationCancelled,
    DatabaseError,
    InternalServerError
}