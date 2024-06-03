using System;
using System.Globalization;
using System.IO;
using mcLaunch.Core.Managers;

namespace mcLaunch.Utilities;

public static class LauncherCrashReport
{
    public static string Generate(Exception exception)
    {
        StringWriter wr = new();

        wr.WriteLine($"[!] mcLaunch v{CurrentBuild.Version} commit {CurrentBuild.Commit} on branch {CurrentBuild.Branch}");
        wr.WriteLine("If you are reporting this, please include this crash report");
        wr.WriteLine($"Platform: {Environment.OSVersion}");
        wr.WriteLine($"Platform ID: {Launchsite.Core.Utilities.GetPlatformIdentifier()}");
        wr.WriteLine($"Java Platform ID: {Launchsite.Core.Utilities.GetJavaPlatformIdentifier()}");
        wr.WriteLine($"Architecture: {Launchsite.Core.Utilities.GetArchitecture()}");
        wr.WriteLine($"Dotnet version: {Environment.Version:0.0.0}");
        wr.WriteLine($"System language: {CultureInfo.CurrentCulture.EnglishName} [{CultureInfo.CurrentCulture.Name}]");
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