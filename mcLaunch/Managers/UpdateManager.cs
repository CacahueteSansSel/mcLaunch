using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Cacahuete.MinecraftLib.Core;
using mcLaunch.Core.Managers;
using mcLaunch.GitHub;
using mcLaunch.GitHub.Models;

namespace mcLaunch.Managers;

public static class UpdateManager
{
    public static async Task<bool> UpdateAsync()
    {
        GitHubRelease? latestRelease = await GitHubRepository.GetLatestReleaseAsync();
        if (latestRelease == null) return false;

        if (File.Exists("installer.exe"))
        {
            Process.Start(Path.GetFullPath("installer.exe"));
            Environment.Exit(0);
            
            return true;
        }
        
        /*
        DownloadManager.Begin($"mcLaunch {latestRelease.Name}");
        
        DownloadManager.End();

        await DownloadManager.DownloadAll();
        */
        
        return false;
    }
}