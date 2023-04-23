using System;
using System.Diagnostics;
using mcLaunch.Core.Boxes;

namespace mcLaunch.Utilities;

public static class PlatformSpecific
{
    public static void OpenFolder(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            Process.Start("explorer", path);
            return;
        }

        if (OperatingSystem.IsMacOS())
        {
            Process.Start("open", path);
            return;
        }

        if (OperatingSystem.IsLinux())
        {
            Process.Start("xdg-open", path);
        }
    }
}