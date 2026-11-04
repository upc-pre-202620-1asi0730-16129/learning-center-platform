namespace Acme.Center.Platform.Shared.Domain.Model.Errors;

public enum EErrorType
{
    Failure = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Unauthorized = 4,
    Forbidden = 5,
    Unexpected = 6
}

/// <summary>
///     Represents a domain error.
/// </summary>
/// <param name="Code">The unique error code.</param>
/// <param name="Message">The error message.</param>
/// <param name="Type">The error classification type.</param>
public record Error(string Code, string Message, EErrorType Type = EErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty, EErrorType.Failure);
    public static readonly Error NullValue = new("Error.NullValue", "The specified result value is null.", EErrorType.Failure);

    public static Error NotFound(string code, string message) => new(code, message, EErrorType.NotFound);
    public static Error Conflict(string code, string message) => new(code, message, EErrorType.Conflict);
    public static Error Validation(string code, string message) => new(code, message, EErrorType.Validation);
    public static Error Failure(string code, string message) => new(code, message, EErrorType.Failure);
    public static Error Unauthorized(string code, string message) => new(code, message, EErrorType.Unauthorized);
    public static Error Forbidden(string code, string message) => new(code, message, EErrorType.Forbidden);
    public static Error Unexpected(string code, string message) => new(code, message, EErrorType.Unexpected);
}
