using DomainErrorModel = Acme.Center.Platform.Shared.Domain.Model.Errors.Error;

namespace Acme.Center.Platform.Shared.Application.Model;

/// <summary>
///     Generic Result class for Command Handlers in the Application Layer.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public class Result<T>
{
    protected Result(bool isSuccess, T? value, string message, Enum? errorEnum, DomainErrorModel? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Message = message;
        Error = errorEnum;
        DomainError = error ?? (isSuccess ? DomainErrorModel.None : DomainErrorModel.Failure(errorEnum?.ToString() ?? "Error", message));
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string Message { get; }
    public Enum? Error { get; }
    public DomainErrorModel DomainError { get; }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, string.Empty, null, DomainErrorModel.None);
    }

    public static Result<T> Failure(Enum error, string message)
    {
        return new Result<T>(false, default, message, error, DomainErrorModel.Failure(error.ToString(), message));
    }

    public static Result<T> Failure(DomainErrorModel error)
    {
        return new Result<T>(false, default, error.Message, null, error);
    }

    public static Result<T> Failure(DomainErrorModel error, Enum errorEnum)
    {
        return new Result<T>(false, default, error.Message, errorEnum, error);
    }

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<DomainErrorModel, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value!) : onFailure(DomainError);
}

/// <summary>
///     Non-generic Result class for Command Handlers.
/// </summary>
public class Result : Result<object>
{
    private Result(bool isSuccess, string message, Enum? errorEnum, DomainErrorModel? error)
        : base(isSuccess, null, message, errorEnum, error)
    {
    }

    public static Result Success()
    {
        return new Result(true, string.Empty, null, DomainErrorModel.None);
    }

    public new static Result Failure(Enum error, string message)
    {
        return new Result(false, message, error, DomainErrorModel.Failure(error.ToString(), message));
    }

    public new static Result Failure(DomainErrorModel error)
    {
        return new Result(false, error.Message, null, error);
    }

    public static Result Failure(DomainErrorModel error, Enum errorEnum)
    {
        return new Result(false, error.Message, errorEnum, error);
    }
}
