using System;
using System.Diagnostics;
using System.IO;

namespace mcLaunch.Utilities;

public static class PlatformSpecific
{
    public static bool ProcessExists(string name)
    {
        string fullPath = Environment.GetCommandLineArgs()[0];
        string binaryFolder = fullPath.Replace(Path.GetFileName(fullPath), "").Trim();

        return File.Exists($"{binaryFolder}/{name}{(OperatingSystem.IsWindows() ? ".exe" : "")}");
    }

    public static void LaunchProcess(string name, string arguments, string verb = "", bool hidden = false)
    {
        string fullPath = Environment.GetCommandLineArgs()[0];
        string binaryFolder = fullPath.Replace(Path.GetFileName(fullPath), "").Trim();

        Process.Start(new ProcessStartInfo
        {
            FileName = $"{binaryFolder}/{name}{(OperatingSystem.IsWindows() ? ".exe" : "")}",
            WorkingDirectory = binaryFolder,
            Arguments = arguments,
            Verb = verb,
            CreateNoWindow = hidden,
            WindowStyle = hidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal,
            UseShellExecute = false
        });
    }

    public static void OpenFolder(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            Process.Start("explorer", path.Replace("/", "\\"));
            return;
        }

        if (OperatingSystem.IsMacOS())
        {
            Process.Start("open", $"\"{path}\"");
            return;
        }

        if (OperatingSystem.IsLinux()) Process.Start("xdg-open", $"\"{path}\"");
    }

    public static void OpenFile(string path)
    {
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            });
            return;
        }

        if (OperatingSystem.IsMacOS())
        {
            Process.Start("open", $"\"{path}\"");
            return;
        }

        if (OperatingSystem.IsLinux()) Process.Start("xdg-open", $"\"{path}\"");
    }

    public static void OpenUrl(string url)
    {
        if (OperatingSystem.IsWindows())
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url.Replace("&", "^&"),
                UseShellExecute = true
            });
            return;
        }

        if (OperatingSystem.IsMacOS())
        {
            Process.Start("open", url);
            return;
        }

        if (OperatingSystem.IsLinux()) Process.Start("xdg-open", url);
    }
}