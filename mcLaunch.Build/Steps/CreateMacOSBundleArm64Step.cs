namespace mcLaunch.Build.Steps;

public class CreateMacOSBundleArm64Step : CreateMacOSBundleStep
{
    public override string PlatformRuntimeIdentifier => "osx-arm64";
}