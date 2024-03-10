namespace mcLaunch.Build;

public abstract class BuildStepBase
{
    public abstract string Name { get; }

    public virtual bool IsSupportedOnThisPlatform()
    {
        return true;
    }

    public abstract Task<BuildResult> RunAsync(BuildSystem system);
}