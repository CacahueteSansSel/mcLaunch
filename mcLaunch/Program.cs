using Avalonia;
using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using Avalonia.Controls.ApplicationLifetimes;
using mcLaunch.Utilities;
using mcLaunch.Views.Windows;
using ReactiveUI;

namespace mcLaunch;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        AppBuilder app = BuildAvaloniaApp();

        if (Debugger.IsAttached)
        {
            app.StartWithClassicDesktopLifetime(args);
            
            return;
        }

        try
        {
            app.StartWithClassicDesktopLifetime(args);
        }
        catch (Exception e)
        {
            // Here, Avalonia is kind of broken and we can't show a window
            // So we need to use a weird trick in order to show a window
            // Basically, restarting the launcher and telling him that the last instance crashed
            
            // We generate a crash report that we will feed to the new launcher instance
            string crashReportFilename = LauncherCrashReport.Generate(e);
            PlatformSpecific.LaunchProcess("mcLaunch", $"--crash \"{crashReportFilename}\"");
            
            Environment.Exit(1);
        }
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .LogToTrace();
}