namespace mcLaunch.Build.Steps;

public class CreateMacosBundleArm64Step : CreateMacosBundleStep
{
    public override string PlatformRuntimeIdentifier => "osx-arm64";
}