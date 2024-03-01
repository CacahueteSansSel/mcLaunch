namespace mcLaunch.Build.Steps;

public class BuildMcLaunchMacOSArm64Step : BuildMcLaunchStep
{
    public override string Platform => "macOS";
    public override string PlatformRuntimeIdentifier => "osx-arm64";
    public override bool GenerateZipFile => false;
}