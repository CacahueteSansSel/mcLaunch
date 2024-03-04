namespace mcLaunch.Build.Steps;

public class BuildMcLaunchMacOS64Step : BuildMcLaunchStep
{
    public override string Platform => "macOS";
    public override string PlatformRuntimeIdentifier => "osx-x64";
    public override bool GenerateZipFile => false;
}