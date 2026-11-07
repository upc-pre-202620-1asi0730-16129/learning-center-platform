namespace Acme.Center.Platform.Publishing.Domain.Model;

public enum PublishingError
{
    None,
    CategoryNotFound,
    TutorialNotFound,
    DuplicateTutorialTitle,
    OperationCancelled,
    DatabaseError,
    InternalServerError
}