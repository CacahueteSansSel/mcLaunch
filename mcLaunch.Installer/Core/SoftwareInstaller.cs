using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using mcLaunch.GitHub.Models;
using mcLaunch.Installer.Win32;
using mcLaunch.Launchsite.Core;
using Microsoft.Win32;

namespace mcLaunch.Installer.Core;

public class SoftwareInstaller
{
    public SoftwareInstaller(InstallerParameters parameters)
    {
        Parameters = parameters;
    }

    public InstallerParameters Parameters { get; }
    public bool CopyInstaller { get; set; } = true;
    public event Action? OnExtractionStarted;

    private async Task RunPostInstallTasks()
    {
        if (!OperatingSystem.IsWindows()) return;

        string appPath = $"{Parameters.TargetDirectory}/mcLaunch.exe";

        if (Parameters.PlaceShortcutOnDesktop)
        {
            string shortcutPath =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}{Path.DirectorySeparatorChar}mcLaunch.lnk";

            Shortcut.Create(shortcutPath, appPath, "", Parameters.TargetDirectory, "mcLaunch Minecraft Launcher", "",
                appPath);
        }

        if (Parameters.RegisterInApplicationList)
        {
            // Source code from : https://stackoverflow.com/questions/68230927/how-do-i-make-my-program-show-up-in-apps-and-features
            // 1st answer

            RegistryKey regKey = Registry.LocalMachine
                .OpenSubKey("SOFTWARE")
                .OpenSubKey("Microsoft")
                .OpenSubKey("Windows")
                .OpenSubKey("CurrentVersion")
                .OpenSubKey("Uninstall", true)
                .CreateSubKey("mcLaunch");

            regKey.SetValue("DisplayIcon", appPath);
            regKey.SetValue("DisplayName", "mcLaunch");
            regKey.SetValue("DisplayVersion", Parameters.ReleaseToDownload.Name);
            regKey.SetValue("Publisher", "Cacahuète Dev & Contributors");
            regKey.SetValue("UninstallString", $"{Parameters.TargetDirectory}/uninstall.exe");

            regKey.Close();
        }
    }

    private GitHubReleaseAsset? FindMcLaunchForPlatform()
    {
        string platform = OperatingSystem.IsWindows()
            ? "windows"
            : OperatingSystem.IsLinux()
                ? "linux"
                : "unknown";
        string newerPlatform = Utilities.GetMcLaunchPlatformIdentifier();
        string arch = Utilities.GetArchitecture();
        
        return Parameters.ReleaseToDownload.Assets
            .FirstOrDefault(asset => asset.Name.ToLower() == $"mclaunch-{platform}.zip"
                || asset.Name.ToLower() == $"mclaunch-{newerPlatform}-{arch}.zip");
    }

    public async Task InstallAsync()
    {
        GitHubReleaseAsset? platformAsset = FindMcLaunchForPlatform();

        using MemoryStream archiveStream =
            await DownloadManager.DownloadToMemoryAsync(platformAsset.DownloadUrl, (long) platformAsset.Size);

        OnExtractionStarted?.Invoke();

        if (!Directory.Exists(Parameters.TargetDirectory))
            Directory.CreateDirectory(Parameters.TargetDirectory);

        using ZipArchive archive = new(archiveStream, ZipArchiveMode.Read, false);

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string filename = $"{Parameters.TargetDirectory}/{entry.FullName}";

            if (File.Exists(filename))
                try
                {
                    File.Delete(filename);
                }
                catch (Exception e)
                {
                }
        }

        archive.ExtractToDirectory(Parameters.TargetDirectory, true);

        if (CopyInstaller)
        {
            try
            {
                Directory.CreateDirectory($"{Parameters.TargetDirectory}/installer");
                File.Copy(Environment.GetCommandLineArgs()[0],
                    $"{Parameters.TargetDirectory}/installer/installer" + (OperatingSystem.IsWindows() ? ".exe" : ""),
                    true);

                if (!OperatingSystem.IsWindows())
                    File.SetUnixFileMode($"{Parameters.TargetDirectory}/installer/installer", UnixFileMode.UserExecute);
            }
            catch (Exception e)
            {
                // ignored
            }
        }

        MainWindow.Instance.Next();

        if (OperatingSystem.IsWindows()) await RunPostInstallTasks();
    }
}