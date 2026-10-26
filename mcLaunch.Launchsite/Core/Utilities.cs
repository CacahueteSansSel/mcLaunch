using System.Runtime.InteropServices;

namespace mcLaunch.Launchsite.Core;

public static class Utilities
{
    public static string GetPlatformIdentifier()
    {
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.Win32NT:
            case PlatformID.WinCE:
                return "windows";
            case PlatformID.Unix:
                if (OperatingSystem.IsMacOS())
                    return "osx";

                return "linux";
            case PlatformID.MacOSX:
                return "osx";
            default:
                return "unknown";
        }
    }

    public static string GetMcLaunchPlatformIdentifier()
    {
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.Win32NT:
            case PlatformID.WinCE:
                return "win";
            case PlatformID.Unix:
                if (OperatingSystem.IsMacOS())
                    return "osx";

                return "linux";
            case PlatformID.MacOSX:
                return "osx";
            default:
                return "unknown";
        }
    }
    
    public static string GetZuluPlatformIdentifier()
    {
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.Win32NT:
            case PlatformID.WinCE:
                return "win";
            case PlatformID.Unix:
                if (OperatingSystem.IsMacOS())
                    return "macos";

                return "linux";
            case PlatformID.MacOSX:
                return "macos";
            default:
                return "unknown";
        }
    }

    public static string GetJavaPlatformIdentifier()
    {
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.Win32NT:
            case PlatformID.WinCE:
                return $"windows-{GetArchitecture()}";
            case PlatformID.Unix:
                if (OperatingSystem.IsMacOS())
                    return RuntimeInformation.ProcessArchitecture == Architecture.Arm64 ? "mac-os-arm64" : "mac-os";

                return RuntimeInformation.ProcessArchitecture == Architecture.X86 ? "linux-i386" : "linux";
            default:
                return "unknown";
        }
    }

    public static string GetArchitecture()
    {
        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.X86:
                return "x86";
            case Architecture.X64:
                return "x64";
            case Architecture.Arm:
                return "arm";
            case Architecture.Arm64:
                return "arm64";
            case Architecture.Wasm:
                return "wasm";
            default:
                return RuntimeInformation.ProcessArchitecture.ToString().ToLower();
        }
    }

    public static void RemoveTopDirectory(string directoryPath)
    {
        FileSystemInfo[] entries = new DirectoryInfo(directoryPath).GetFileSystemInfos();
        if (entries.Length != 1 || entries[0] is not DirectoryInfo topDirectory
                                || (topDirectory.Attributes & FileAttributes.ReparsePoint) != 0)
            return;

        // Rename the wrapper first so a child with the same name can also be moved up.
        topDirectory.MoveTo(Path.Combine(directoryPath, $".mclaunch-unpack-{Guid.NewGuid():N}"));

        foreach (FileSystemInfo entry in topDirectory.GetFileSystemInfos())
        {
            string destination = Path.Combine(directoryPath, entry.Name);
            if (entry is DirectoryInfo directory)
                directory.MoveTo(destination);
            else
                File.Move(entry.FullName, destination);
        }

        topDirectory.Delete();
    }
}
