namespace mcLaunch.Build.Steps;

public class BuildMcLaunchLinuxArm64Step : BuildMcLaunchStep
{
    public override string Platform => "Linux";
    public override string PlatformRuntimeIdentifier => "linux-arm64";
}