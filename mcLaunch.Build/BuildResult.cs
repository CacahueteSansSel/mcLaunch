namespace mcLaunch.Build;

public class BuildResult
{
    public static BuildResult Error(string message) => new(errorMessage: message);
    
    public bool IsError { get; }
    public string? ErrorMessage { get; }

    public BuildResult()
    {
        IsError = false;
    }
    
    protected BuildResult(string errorMessage)
    {
        IsError = true;
        ErrorMessage = errorMessage;
    }
}