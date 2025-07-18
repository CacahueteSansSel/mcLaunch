using System;
using System.IO;
using System.Threading.Tasks;
using mcLaunch.Core.Managers;
using mcLaunch.GitHub;
using mcLaunch.GitHub.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;

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

    public static async Task<string?> FindInstallerUrlAsync()
    {
        GitHubRelease[]? releases = await GitHubRepository.GetReleasesAsync();
        if (releases == null) return null;

        string platform = Launchsite.Core.Utilities.GetMcLaunchPlatformIdentifier();
        string arch = Launchsite.Core.Utilities.GetArchitecture();
        string extension = OperatingSystem.IsWindows() ? ".exe" : "";
        string expectedInstallerName = $"mcLaunch-Installer-{platform}-{arch}{extension}";

        foreach (GitHubRelease release in releases)
        {
            foreach (GitHubReleaseAsset asset in release.Assets)
            {
                if (asset.Name == expectedInstallerName)
                    return asset.DownloadUrl;
            }
        }

        return null;
    }

    public static bool LaunchInstaller()
    {
        if (Directory.Exists("installer") && PlatformSpecific.ProcessExists("installer/installer"))
        {
            PlatformSpecific.LaunchProcess("installer/installer",
                $"\"{Environment.CurrentDirectory}\"",
                "runas");

            Environment.Exit(0);
            return true;
        }

        return false;
    }

    public static async Task<bool> UpdateInstallerAsync(bool popup = true)
    {
        string? installerUrl = await FindInstallerUrlAsync();
        if (installerUrl == null) return false;

        void ProgressUpdate(string str, float percent, int _)
        {
            StatusPopup.Instance.StatusPercent = percent;
        }

        if (popup)
        {
            Navigation.ShowPopup(new StatusPopup("Downloading installer", "Downloading latest mcLaunch installer"));
            StatusPopup.Instance.ShowDownloadBanner = false;

            DownloadManager.OnDownloadProgressUpdate += ProgressUpdate;
        }

        if (File.Exists("installer/installer")) File.Delete("installer/installer");
        if (File.Exists("installer/installer.exe")) File.Delete("installer/installer.exe");

        Directory.CreateDirectory("installer");

        DownloadManager.Begin("mcLaunch Installer");
        DownloadManager.Add(installerUrl, Path.GetFullPath("installer/installer.exe"), null, EntryAction.Download);
        DownloadManager.End();

        await DownloadManager.ProcessAll();

        if (popup)
        {
            DownloadManager.OnDownloadProgressUpdate -= ProgressUpdate;
            Navigation.HidePopup();
        }

        return Directory.Exists("installer") && PlatformSpecific.ProcessExists("installer/installer");
    }

    public static async Task<bool> UpdateAsync()
    {
        if (!OperatingSystem.IsWindows()) return false;

        if (LaunchInstaller()) return true;

        if (!await UpdateInstallerAsync()) return false;
        if (LaunchInstaller()) return true;

        return false;
    }
}