using System.Diagnostics;

namespace mcLaunch.Build.Core;

public class Project
{
    public Project(string folder)
    {
        Folder = Path.GetFullPath(folder);
        Name = Path.GetFileName(folder);
    }

    public string Name { get; }
    public string Folder { get; }

    public async Task<BuildResult> PublishAsync(string runtimeId, string outputDirPath,
        string configuration = "Release", bool selfContained = true)
    {
        string dotnetFilename = File.Exists("/usr/share/dotnet/dotnet") && OperatingSystem.IsLinux()
            ? "/usr/share/dotnet/dotnet"
            : "dotnet";

        Process? process = Process.Start(new ProcessStartInfo
        {
            FileName = dotnetFilename,
            WorkingDirectory = Folder,
            Arguments =
                $"publish -c {configuration} -r {runtimeId} {(selfContained ? "--sc" : "")} -o \"{outputDirPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true
        });

        string stdout = await process!.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        return process.ExitCode != 0
            ? BuildResult.Error(stdout + "\n" + stderr)
            : new BuildResult();
    }
}