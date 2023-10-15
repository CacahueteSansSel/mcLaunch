using System;
using System.IO;
using Avalonia.Controls;
using mcLaunch.Core.Managers;

namespace mcLaunch.Utilities;

public static class LauncherCrashReport
{
    public static string Generate(Exception exception)
    {
        StringWriter wr = new();

        wr.WriteLine($"[!] mcLaunch v{CurrentBuild.Version} commit {CurrentBuild.Commit}");
        wr.WriteLine($"If you are reporting this, please include this crash report");
        wr.WriteLine($"Platform: {Environment.OSVersion}");
        wr.WriteLine($"Platform ID: {Cacahuete.MinecraftLib.Core.Utilities.GetPlatformIdentifier()}");
        wr.WriteLine($"Java Platform ID: {Cacahuete.MinecraftLib.Core.Utilities.GetJavaPlatformIdentifier()}");
        wr.WriteLine($"Architecture: {Cacahuete.MinecraftLib.Core.Utilities.GetArchitecture()}");
        wr.WriteLine();
        wr.WriteLine("Crash Type: Exception");
        wr.WriteLine();
        wr.WriteLine(exception.ToString());

        if (!Directory.Exists("launcher_crashreports")) 
            Directory.CreateDirectory("launcher_crashreports");
        
        string filename = $"{AppdataFolderManager.GetValidPath("launcher_crashreports")}" +
                          $"/{DateTime.Now:yyyy MM dd - hh mm ss}.txt";
        File.WriteAllText(filename, wr.ToString());
        
        return filename;
    }
}