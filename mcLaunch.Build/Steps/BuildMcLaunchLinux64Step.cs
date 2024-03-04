namespace mcLaunch.Build.Steps;

public class BuildMcLaunchLinux64Step : BuildMcLaunchStep
{
    public override string Platform => "Linux";
    public override string PlatformRuntimeIdentifier => "linux-x64";
}