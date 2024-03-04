using System.IO.Compression;
using mcLaunch.Build.Core;

namespace mcLaunch.Build.Steps;

public abstract class CreateMacOSBundleStep : BuildStepBase
{
    public abstract string PlatformRuntimeIdentifier { get; }
    public override string Name => $"Create macOS bundle ({PlatformRuntimeIdentifier})";

    public override bool IsSupportedOnThisPlatform() => !OperatingSystem.IsWindows();
    public override async Task<BuildResult> RunAsync(BuildSystem system)
    {
        Project? mcLaunch = system.GetProject("mcLaunch");
        if (mcLaunch == null) return BuildResult.Error("mcLaunch is not present");
        
        string outputDirectory = $"{system.SolutionDirectory}/output/macos/{PlatformRuntimeIdentifier}";
        string releasesDirectory = $"{system.SolutionDirectory}/output/releases";
        string bundlePath = $"{system.SolutionDirectory}/output/macos/{PlatformRuntimeIdentifier}/bundle";
        string bundleFullPath = $"{bundlePath}/mcLaunch.app";
        Directory.CreateDirectory(releasesDirectory);
        
        Directory.CreateDirectory(bundleFullPath);
        Directory.CreateDirectory($"{bundleFullPath}/Contents/MacOS");
        Directory.CreateDirectory($"{bundleFullPath}/Contents/Resources");
        
        FileSystemUtilities.CopyDirectory(outputDirectory, $"{bundleFullPath}/Contents/MacOS");

        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode($"{bundleFullPath}/Contents/MacOS/mcLaunch",
                UnixFileMode.UserExecute | UnixFileMode.OtherExecute | UnixFileMode.GroupExecute
                | UnixFileMode.UserWrite | UnixFileMode.UserRead | UnixFileMode.GroupRead | UnixFileMode.OtherRead);
            
            File.SetUnixFileMode($"{bundleFullPath}/Contents/MacOS/mcLaunch.MinecraftGuard",
                UnixFileMode.UserExecute | UnixFileMode.OtherExecute | UnixFileMode.GroupExecute
                | UnixFileMode.UserWrite | UnixFileMode.UserRead | UnixFileMode.GroupRead | UnixFileMode.OtherRead);
        }
        
        File.Copy($"{mcLaunch.Folder}/resources/Info.plist",
            $"{bundleFullPath}/Contents/Info.plist", true);
        File.Copy($"{mcLaunch.Folder}/resources/Icons.icns", 
            $"{bundleFullPath}/Contents/Resources/mcLaunch.icns", true);

        string finalZipFilePath = $"{releasesDirectory}/mcLaunch-{PlatformRuntimeIdentifier}.zip";
        if (File.Exists(finalZipFilePath)) File.Delete(finalZipFilePath);
        ZipFile.CreateFromDirectory(bundlePath, finalZipFilePath);

        return new BuildResult();
    }
}