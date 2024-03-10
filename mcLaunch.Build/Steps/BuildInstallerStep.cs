using mcLaunch.Build.Core;

namespace mcLaunch.Build.Steps;

public abstract class BuildInstallerStep : BuildStepBase
{
    public abstract string Platform { get; }
    public abstract string PlatformRuntimeIdentifier { get; }
    public abstract string InstallerExtension { get; }

    public override string Name => $"Build mcLaunch installer for {Platform} ({PlatformRuntimeIdentifier})";

    public override async Task<BuildResult> RunAsync(BuildSystem system)
    {
        Project? installer = system.GetProject("mcLaunch.Installer");
        if (installer == null) return BuildResult.Error("mcLaunch is not present");

        string outputDirectory =
            $"{system.SolutionDirectory}/output/{Platform.ToLower()}/installer-{PlatformRuntimeIdentifier}";
        string releasesDirectory = $"{system.SolutionDirectory}/output/releases";
        Directory.CreateDirectory(outputDirectory);
        Directory.CreateDirectory(releasesDirectory);

        BuildResult result = await installer.PublishAsync(PlatformRuntimeIdentifier, outputDirectory);
        if (result.IsError) return result;

        File.Copy($"{outputDirectory}/mcLaunch.Installer{InstallerExtension}",
            $"{releasesDirectory}/mcLaunch-Installer-{PlatformRuntimeIdentifier}{InstallerExtension}");

        return new BuildResult();
    }
}