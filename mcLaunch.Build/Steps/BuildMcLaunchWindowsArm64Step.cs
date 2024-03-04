namespace mcLaunch.Build.Steps;

public class BuildMcLaunchWindowsArm64Step : BuildMcLaunchStep
{
    public override string Platform => "Windows";
    public override string PlatformRuntimeIdentifier => "win-arm64";
}