using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Cacahuete.MinecraftLib.Models.GitHub;

namespace mcLaunch.Installer.Core;

public class SoftwareInstaller
{
    public InstallerParameters Parameters { get; private set; }
    public event Action? OnExtractionStarted;

    public SoftwareInstaller(InstallerParameters parameters)
    {
        Parameters = parameters;
    }

    public async Task InstallAsync()
    {
        string platform = OperatingSystem.IsWindows()
            ? "windows"
            : (OperatingSystem.IsLinux() ? "linux" : "unknown");

        GitHubReleaseAsset platformAsset = Parameters.ReleaseToDownload.Assets
            .First(asset => asset.Name.ToLower() == $"mclaunch-{platform}.zip");

        using MemoryStream archiveStream = await Downloader.DownloadToMemoryAsync(platformAsset.DownloadUrl, (long)platformAsset.Size);
        
        OnExtractionStarted?.Invoke();

        if (!Directory.Exists(Parameters.TargetDirectory))
            Directory.CreateDirectory(Parameters.TargetDirectory);

        using ZipArchive archive = new(archiveStream, ZipArchiveMode.Read, false);
        archive.ExtractToDirectory(Parameters.TargetDirectory);
        
        MainWindow.Instance.Next();
    }
}