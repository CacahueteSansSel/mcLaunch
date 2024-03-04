namespace mcLaunch.Build.Steps;

public class BuildInstallerLinux64Step : BuildInstallerStep
{
    public override string Platform => "Linux";
    public override string PlatformRuntimeIdentifier => "linux-x64";
    public override string InstallerExtension => string.Empty;
}