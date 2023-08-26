using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    const string FrameworkVersion = "net6.0";

    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    AbsolutePath TestsDirectory => RootDirectory / "tests";
    AbsolutePath OutputDirectory => RootDirectory / "output";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target KillPreviewerProcesses => _ => _
        .Executes(() =>
        {
            // WARNING: this kills every other instance of dotnet running
            // this should be used with caution

            Process[] previewers = Process.GetProcessesByName("dotnet")
                .Where(p => p.Id != Process.GetCurrentProcess().Id).ToArray();

            foreach (Process p in previewers) p.Kill();
        });

    Target Publish => _ => _
        .DependsOn(Restore, KillPreviewerProcesses)
        .Executes(() =>
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                WorkingDirectory = Solution.GetProject("mcLaunch.MinecraftGuard").Directory,
                Arguments = "publish -c Release -r win-x64"
            }).WaitForExit();

            Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                WorkingDirectory = Solution.GetProject("mcLaunch.MinecraftGuard").Directory,
                Arguments = "publish -c Release -r linux-x64"
            }).WaitForExit();

            Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                WorkingDirectory = Solution.GetProject("mcLaunch").Directory,
                Arguments = "publish -c Release -r win-x64"
            }).WaitForExit();

            Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                WorkingDirectory = Solution.GetProject("mcLaunch").Directory,
                Arguments = "publish -c Release -r linux-x64"
            }).WaitForExit();

            string outputDirPath = Solution.Directory / "output";
            if (!Directory.Exists(outputDirPath + "/windows")) Directory.CreateDirectory(outputDirPath + "/windows");
            if (!Directory.Exists(outputDirPath + "/linux")) Directory.CreateDirectory(outputDirPath + "/linux");

            CopyDirectoryRecursively(
                $"{Solution.GetProject("mcLaunch").Directory}/bin/Release/{FrameworkVersion}/win-x64/publish",
                $"{outputDirPath}/windows", 
                DirectoryExistsPolicy.Merge,
                FileExistsPolicy.OverwriteIfNewer
            );
            
            CopyDirectoryRecursively(
                $"{Solution.GetProject("mcLaunch").Directory}/bin/Release/{FrameworkVersion}/linux-x64/publish",
                $"{outputDirPath}/linux", 
                DirectoryExistsPolicy.Merge,
                FileExistsPolicy.OverwriteIfNewer
            );
            
            CopyDirectoryRecursively(
                $"{Solution.GetProject("mcLaunch.MinecraftGuard").Directory}/bin/Release/net7.0/win-x64/publish",
                $"{outputDirPath}/windows", 
                DirectoryExistsPolicy.Merge,
                FileExistsPolicy.OverwriteIfNewer
            );
            
            CopyDirectoryRecursively(
                $"{Solution.GetProject("mcLaunch.MinecraftGuard").Directory}/bin/Release/net7.0/linux-x64/publish",
                $"{outputDirPath}/linux", 
                DirectoryExistsPolicy.Merge,
                FileExistsPolicy.OverwriteIfNewer
            );
            
            Log.Information("Zipping windows build...");
            if (File.Exists($"{outputDirPath}/mcLaunch-windows.zip")) File.Delete($"{outputDirPath}/mcLaunch-windows.zip");
            ZipFile.CreateFromDirectory($"{outputDirPath}/windows", $"{outputDirPath}/mcLaunch-windows.zip");
            
            Log.Information("Zipping linux build...");
            if (File.Exists($"{outputDirPath}/mcLaunch-linux.zip")) File.Delete($"{outputDirPath}/mcLaunch-linux.zip");
            ZipFile.CreateFromDirectory($"{outputDirPath}/linux", $"{outputDirPath}/mcLaunch-linux.zip");
        });

    Target BuildInstaller => _ => _
        .DependsOn(Restore, KillPreviewerProcesses)
        .Executes(() =>
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                WorkingDirectory = Solution.GetProject("mcLaunch.Installer").Directory,
                Arguments = "publish -c Release -r win-x64 --self-contained"
            }).WaitForExit();
        });

    Target Compile => _ => _
        .DependsOn(Restore, KillPreviewerProcesses)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });
}