namespace mcLaunch.Build.Steps;

public class BuildMcLaunchWindows64Step : BuildMcLaunchStep
{
    public override string Platform => "Windows";
    public override string PlatformRuntimeIdentifier => "win-x64";
}