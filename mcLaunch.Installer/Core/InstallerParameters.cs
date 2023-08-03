using System;
using System.IO;

namespace mcLaunch.Installer.Core;

public class InstallerParameters
{
    public string TargetDirectory { get; set; }
    public bool PlaceShortcutOnDesktop { get; set; }
    public bool RegisterInApplicationList { get; set; }

    public void SetDefaultTargetDirectory()
    {
        TargetDirectory = $"{Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)}{Path.DirectorySeparatorChar}mcLaunch";
    }
}