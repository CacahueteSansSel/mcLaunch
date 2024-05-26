namespace mcLaunch.Launchsite.Core;

public class Result
{
    public Result()
    {
        IsError = false;
    }

    public Result(Result otherResult)
    {
        IsError = otherResult.IsError;
        ErrorMessage = otherResult.ErrorMessage;
    }

    protected Result(string errorMessage)
    {
        IsError = true;
        ErrorMessage = errorMessage;
    }

    public bool IsError { get; }
    public string? ErrorMessage { get; }

    public static Result Error(string message) => new(message);
}

public class Result<T> : Result
{
    public Result(T? data)
    {
        Data = data;
    }

    public Result(Result otherResult) : base(otherResult)
    {
    }

    protected Result(string errorMessage) : base(errorMessage)
    {
    }

    public T? Data { get; }

    public static Result<T> Error(string message) => new(message);
}