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
                return "windows";
            case PlatformID.Unix:
                return "linux";
            case PlatformID.MacOSX:
                return "osx";
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