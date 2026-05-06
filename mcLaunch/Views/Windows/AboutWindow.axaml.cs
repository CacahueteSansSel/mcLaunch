using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Windows;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        
        string? avaloniaVersion = typeof(Avalonia.Application)
            .Assembly
            .GetName()
            .Version?.ToString(2);

        string platform;
        if (OperatingSystem.IsWindows()) platform = "Windows";
        else if (OperatingSystem.IsMacOS()) platform = "macOS";
        else if (OperatingSystem.IsLinux()) platform = "Linux";
        else platform = "Unknown";

        VersionText.Text = $"mcLaunch v{CurrentBuild.Version}";
        BuildInfoText.Text = $"branch {CurrentBuild.Branch} • " +
                             $"{platform} {Launchsite.Core.Utilities.GetArchitecture()} • " +
                             $".NET {Environment.Version.ToString(2)}";

        if (avaloniaVersion != null)
            BuildInfoText.Text += $" • Avalonia {avaloniaVersion}";
    }

    private void GitHubButtonClicked(object? sender, RoutedEventArgs e)
    {
        PlatformSpecific.OpenUrl("https://github.com/CacahueteSansSel/mcLaunch");
    }
}