namespace Cacahuete.MinecraftLib.Core;

public class Result
{
    public static Result Error(string message) => new(errorMessage: message);
    
    public bool IsError { get; }
    public string? ErrorMessage { get; }

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
}

public class Result<T> : Result
{
    public static Result<T> Error(string message) => new(errorMessage: message);
    
    public T? Data { get; }

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
}