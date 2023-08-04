using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Cacahuete.MinecraftLib.Models.GitHub;
using mcLaunch.Installer.Win32;
using Microsoft.Win32;

namespace mcLaunch.Installer.Core;

public class SoftwareInstaller
{
    public InstallerParameters Parameters { get; private set; }
    public event Action? OnExtractionStarted;

    public SoftwareInstaller(InstallerParameters parameters)
    {
        Parameters = parameters;
    }

    async Task RunPostInstallTasks()
    {
        string appPath = $"{Parameters.TargetDirectory}/mcLaunch.exe";
        
        if (Parameters.PlaceShortcutOnDesktop)
        {
            string shortcutPath =
                $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}{Path.DirectorySeparatorChar}mcLaunch.lnk";
            
            Shortcut.Create(shortcutPath, appPath, "", Parameters.TargetDirectory, "mcLaunch Minecraft Launcher", "", appPath);
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

        await RunPostInstallTasks();
    }
}