namespace mcLaunch.Build.Steps;

public class BuildInstallerWindows64Step : BuildInstallerStep
{
    public override string Platform => "Windows";
    public override string PlatformRuntimeIdentifier => "win-x64";
    public override string InstallerExtension => ".exe";
}