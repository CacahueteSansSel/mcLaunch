namespace mcLaunch.Build.Steps;

public class BuildInstallerLinuxArm64Step : BuildInstallerStep
{
    public override string Platform => "Linux";
    public override string PlatformRuntimeIdentifier => "linux-arm64";
    public override string InstallerExtension => string.Empty;
}