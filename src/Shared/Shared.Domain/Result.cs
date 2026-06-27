namespace Shared.Domain;

/// <summary>
/// Represents the result of an operation with success/failure information
/// </summary>
public class Result
{
    public bool IsSuccess { get; protected set; }
    public string Message { get; protected set; } = string.Empty;
    public List<string> Errors { get; protected set; } = [];

    protected Result() { }

    public static Result Success(string message = "Operation completed successfully")
    {
        return new Result { IsSuccess = true, Message = message };
    }

    public static Result Failure(string message, params string[] errors)
    {
        return new Result
        {
            IsSuccess = false,
            Message = message,
            Errors = [.. errors]
        };
    }

    public static Result Failure(params string[] errors)
    {
        return new Result
        {
            IsSuccess = false,
            Message = "Operation failed",
            Errors = [.. errors]
        };
    }
}

/// <summary>
/// Represents the result of an operation with a value
/// </summary>
public class Result<T> : Result
{
    public T? Data { get; protected set; }

    public static Result<T> Success(T data, string message = "Operation completed successfully")
    {
        return new Result<T> { IsSuccess = true, Data = data, Message = message };
    }

    public new static Result<T> Failure(string message, params string[] errors)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = message,
            Errors = [.. errors]
        };
    }

    public new static Result<T> Failure(params string[] errors)
    {
        return new Result<T>
        {
            IsSuccess = false,
            Message = "Operation failed",
            Errors = [.. errors]
        };
    }
}
