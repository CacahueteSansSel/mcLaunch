namespace mcLaunch.Build.Steps;

public class CreateMacosBundle64Step : CreateMacosBundleStep
{
    public override string PlatformRuntimeIdentifier => "osx-x64";
}