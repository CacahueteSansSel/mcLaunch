namespace mcLaunch.Build.Core;

public class Utilities
{
    public static string GetPlatformName()
    {
        if (OperatingSystem.IsWindows()) return "Windows";
        if (OperatingSystem.IsMacOS()) return "macOS";
        if (OperatingSystem.IsLinux()) return "Linux";

        return Environment.OSVersion.Platform.ToString();
    }
}