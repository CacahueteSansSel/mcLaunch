using System.Diagnostics;

namespace mcLaunch.Core.Utilities;

public static class Unix
{
    public static async Task<bool> ChmodAsync(string target, string perms)
    {
        if (Environment.OSVersion.Platform != PlatformID.Unix) return false;
        
        try
        {
            using Process proc = Process.Start("/bin/bash", $"-c \"chmod {perms} {target}\"");
            
            await proc.WaitForExitAsync();
            
            return proc.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}