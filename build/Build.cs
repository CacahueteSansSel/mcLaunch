using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Serilog;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[ShutdownDotNetAfterServerBuild]
class Build : NukeBuild
{
    const string FrameworkVersion = "net8.0";

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    [Solution] readonly Solution Solution;

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

    Target WriteCommitId => _ => _
        .Executes(() =>
        {
            string filename = $"{Solution.GetProject("mcLaunch").Directory / "resources" / "commit"}";

            File.WriteAllText(filename, GitRepository.Commit[..7]);
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
        .DependsOn(KillPreviewerProcesses, WriteCommitId)
        .Executes(() =>
        {
            string outputDirPath = Solution.Directory / "output";
            if (!Directory.Exists(outputDirPath + "/windows")) Directory.CreateDirectory(outputDirPath + "/windows");
            if (!Directory.Exists(outputDirPath + "/linux")) Directory.CreateDirectory(outputDirPath + "/linux");
            if (!Directory.Exists(outputDirPath + "/macos")) Directory.CreateDirectory(outputDirPath + "/macos");

            // Windows
            Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                WorkingDirectory = Solution.GetProject("mcLaunch.MinecraftGuard")!.Directory,
                Arguments = $"publish -c Release -r win-x64 --sc -o \"{outputDirPath + "/windows"}\""
            })!.WaitForExit();
            Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                WorkingDirectory = Solution.GetProject("mcLaunch")!.Directory,
                Arguments = $"publish -c Release -r win-x64 --sc -o \"{outputDirPath + "/windows"}\""
            })!.WaitForExit();

            // macOS
            BuildMacOSBundle(outputDirPath, "arm64"); // Apple Silicon
            BuildMacOSBundle(outputDirPath, "x64"); // Intel

            // Linux
            Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                WorkingDirectory = Solution.GetProject("mcLaunch.MinecraftGuard")!.Directory,
                Arguments = $"publish -c Release -r linux-x64 --sc -o \"{outputDirPath + "/linux"}\""
            })!.WaitForExit();
            Process.Start(new ProcessStartInfo
            {
                FileName = "dotnet",
                WorkingDirectory = Solution.GetProject("mcLaunch")!.Directory,
                Arguments = $"publish -c Release -r linux-x64 --sc -o \"{outputDirPath + "/linux"}\""
            })!.WaitForExit();

            Log.Information("Zipping windows build...");
            if (File.Exists($"{outputDirPath}/mcLaunch-windows.zip"))
                File.Delete($"{outputDirPath}/mcLaunch-windows.zip");
            ZipFile.CreateFromDirectory($"{outputDirPath}/windows", $"{outputDirPath}/mcLaunch-windows.zip");

            Log.Information("Zipping macos build...");
            if (File.Exists($"{outputDirPath}/mcLaunch-macos.zip")) File.Delete($"{outputDirPath}/mcLaunch-macos.zip");
            ZipFile.CreateFromDirectory($"{outputDirPath}/macos", $"{outputDirPath}/mcLaunch-macos.zip");

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

    /// Support plugins are available for:
    /// - JetBrains ReSharper        https://nuke.build/resharper
    /// - JetBrains Rider            https://nuke.build/rider
    /// - Microsoft VisualStudio     https://nuke.build/visualstudio
    /// - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Publish);

    void BuildMacOSBundle(string outputDir, string arch = "arm64")
    {
        string path = Solution.Directory / "output" / "macos" / arch / "mcLaunch.app";
        Directory.CreateDirectory(path);
        Directory.CreateDirectory($"{path}/Contents/MacOS");
        Directory.CreateDirectory($"{path}/Contents/Resources");
        
        Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = Solution.GetProject("mcLaunch.MinecraftGuard")!.Directory,
            Arguments = $"publish -c Release -r osx-arm64 -p:PublishSingleFile=true --sc -o \"{path}/Contents/MacOS\""
        })!.WaitForExit();

        Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = Solution.GetProject("mcLaunch")!.Directory,
            Arguments = $"publish -c Release -r osx-{arch} -p:PublishSingleFile=true --sc -o \"{path}/Contents/MacOS\""
        })!.WaitForExit();

        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode($"{path}/Contents/MacOS/mcLaunch",
                UnixFileMode.UserExecute | UnixFileMode.OtherExecute | UnixFileMode.GroupExecute
                | UnixFileMode.UserWrite | UnixFileMode.UserRead | UnixFileMode.GroupRead | UnixFileMode.OtherRead);
            
            File.SetUnixFileMode($"{path}/Contents/MacOS/mcLaunch.MinecraftGuard",
                UnixFileMode.UserExecute | UnixFileMode.OtherExecute | UnixFileMode.GroupExecute
                | UnixFileMode.UserWrite | UnixFileMode.UserRead | UnixFileMode.GroupRead | UnixFileMode.OtherRead);
        }

        File.Copy(Solution.GetProject("mcLaunch")!.Directory / "resources" / "Info.plist",
            $"{path}/Contents/Info.plist", true);
        File.Copy(Solution.GetProject("mcLaunch")!.Directory / "resources" / "Icons.icns",
            $"{path}/Contents/Resources/mcLaunch.icns", true);
    }
}