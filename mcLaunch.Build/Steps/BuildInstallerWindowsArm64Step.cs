namespace mcLaunch.Build.Steps;

public class BuildInstallerWindowsArm64Step : BuildInstallerStep
{
    public override string Platform => "Windows";
    public override string PlatformRuntimeIdentifier => "win-arm64";
    public override string InstallerExtension => ".exe";
}