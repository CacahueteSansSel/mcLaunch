using System.Diagnostics;

namespace mcLaunch.Build.Core;

public class Project
{
    public string Name { get; }
    public string Folder { get; }

    public Project(string folder)
    {
        Folder = Path.GetFullPath(folder);
        Name = Path.GetFileName(folder);
    }

    public async Task<BuildResult> PublishAsync(string runtimeId, string outputDirPath, 
        string configuration = "Release", bool selfContained = true)
    {
        Process? process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = Folder,
            Arguments =
                $"publish -c {configuration} -r {runtimeId} {(selfContained ? "--sc" : "")} -o \"{outputDirPath}\"",
            RedirectStandardOutput = true,
            RedirectStandardInput = true
        });

        await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        return process.ExitCode != 0
            ? BuildResult.Error(await process.StandardError.ReadToEndAsync())
            : new BuildResult();
    }
}