using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Cacahuete.MinecraftLib.Core;
using mcLaunch.Core.Managers;
using mcLaunch.GitHub;
using mcLaunch.GitHub.Models;
using mcLaunch.Utilities;

namespace mcLaunch.Managers;

public static class UpdateManager
{
    public static async Task<bool> IsUpdateAvailableAsync()
    {
        GitHubRelease? latestRelease = await GitHubRepository.GetLatestReleaseAsync();
        if (latestRelease == null) return false;

        string releaseName = string.IsNullOrEmpty(latestRelease.Name) ? latestRelease.TagName : latestRelease.Name;

        return releaseName.TrimStart('v') != CurrentBuild.Version.ToString();
    }
    
    public static async Task<bool> UpdateAsync()
    {
        if (!OperatingSystem.IsWindows()) return false;
        
        GitHubRelease? latestRelease = await GitHubRepository.GetLatestReleaseAsync();
        if (latestRelease == null) return false;

        if (Directory.Exists("installer") && File.Exists("installer/installer.exe"))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = Path.GetFullPath("installer/installer.exe"),
                WorkingDirectory = Path.GetFullPath("installer"),
                Arguments = $"\"{Environment.CurrentDirectory}\"",
                UseShellExecute = true,
                Verb = "runas"
            });
            
            Environment.Exit(0);
            return true;
        }
        
        // TODO: download the installer
        
        /*
        DownloadManager.Begin($"mcLaunch {latestRelease.Name}");
        
        DownloadManager.End();

        await DownloadManager.DownloadAll();
        */
        
        return false;
    }
}