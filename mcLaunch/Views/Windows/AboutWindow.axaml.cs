using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Windows;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();

        string platform;
        if (OperatingSystem.IsWindows()) platform = "windows";
        else if (OperatingSystem.IsMacOS()) platform = "macOS";
        else if (OperatingSystem.IsLinux()) platform = "linux";
        else platform = "unknown";

        VersionText.Text = $"mcLaunch v{CurrentBuild.Version}";
        BuildInfoText.Text = $"commit {CurrentBuild.Commit} • " +
                             $"{platform} {Cacahuete.MinecraftLib.Core.Utilities.GetArchitecture()} • " +
                             $".NET {Environment.Version.ToString(2)}";
    }

    private void GitHubButtonClicked(object? sender, RoutedEventArgs e)
    {
        PlatformSpecific.OpenUrl("https://github.com/CacahueteSansSel/mcLaunch");
    }
}