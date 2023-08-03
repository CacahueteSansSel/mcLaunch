using System;
using System.IO;
using Cacahuete.MinecraftLib.Models.GitHub;

namespace mcLaunch.Installer.Core;

public class InstallerParameters
{
    public string TargetDirectory { get; set; }
    public bool PlaceShortcutOnDesktop { get; set; } = true;
    public bool RegisterInApplicationList { get; set; } = true;
    public GitHubRelease ReleaseToDownload { get; set; }

    public void SetDefaultTargetDirectory()
    {
        TargetDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}{Path.DirectorySeparatorChar}mcLaunch";
    }
}