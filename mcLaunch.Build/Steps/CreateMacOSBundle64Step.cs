namespace mcLaunch.Build.Steps;

public class CreateMacOSBundle64Step : CreateMacOSBundleStep
{
    public override string PlatformRuntimeIdentifier => "osx-x64";
}