using System.Runtime.InteropServices;

namespace Cacahuete.MinecraftLib.Core;

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
                Console.WriteLine("platform is windows");
                
                return "windows";
            case PlatformID.Unix:
                if (OperatingSystem.IsMacOS()) 
                    return "osx";
                Console.WriteLine("platform is linux");
                
                return "linux";
            case PlatformID.MacOSX:
                Console.WriteLine("platform is macos");
                return "osx";
            default:
                Console.WriteLine("platform is unknown");
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
}