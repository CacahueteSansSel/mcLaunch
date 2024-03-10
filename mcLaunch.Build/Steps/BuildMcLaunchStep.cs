using System.IO.Compression;
using mcLaunch.Build.Core;

namespace mcLaunch.Build.Steps;

public abstract class BuildMcLaunchStep : BuildStepBase
{
    public abstract string Platform { get; }
    public abstract string PlatformRuntimeIdentifier { get; }
    public virtual bool GenerateZipFile => true;

    public override string Name => $"Build mcLaunch for {Platform} ({PlatformRuntimeIdentifier})";

    public override async Task<BuildResult> RunAsync(BuildSystem system)
    {
        Project? mcLaunch = system.GetProject("mcLaunch");
        Project? mcGuard = system.GetProject("mcLaunch.MinecraftGuard");
        if (mcLaunch == null) return BuildResult.Error("mcLaunch is not present");
        if (mcGuard == null) return BuildResult.Error("mcLaunch.MinecraftGuard is not present");

        string outputDirectory = $"{system.SolutionDirectory}/output/{Platform.ToLower()}/{PlatformRuntimeIdentifier}";
        string releasesDirectory = $"{system.SolutionDirectory}/output/releases";
        Directory.CreateDirectory(outputDirectory);
        Directory.CreateDirectory(releasesDirectory);

        BuildResult result = await mcLaunch.PublishAsync(PlatformRuntimeIdentifier, outputDirectory);
        if (result.IsError) return result;

        result = await mcGuard.PublishAsync(PlatformRuntimeIdentifier, outputDirectory);
        if (result.IsError) return result;

        if (!GenerateZipFile) return new BuildResult();

        string archiveFilename = $"{releasesDirectory}/mcLaunch-{PlatformRuntimeIdentifier}.zip";
        if (File.Exists(archiveFilename)) File.Delete(archiveFilename);
        ZipFile.CreateFromDirectory(outputDirectory, archiveFilename);

        return new BuildResult();
    }
}