namespace mcLaunch.Build;

public class BuildResult
{
    public BuildResult()
    {
        IsError = false;
    }

    protected BuildResult(string errorMessage)
    {
        IsError = true;
        ErrorMessage = errorMessage;
    }

    public bool IsError { get; }
    public string? ErrorMessage { get; }

    public static BuildResult Error(string message)
    {
        return new BuildResult(message);
    }
}