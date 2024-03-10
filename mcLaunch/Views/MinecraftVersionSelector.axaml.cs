using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Managers;
using mcLaunch.Launchsite.Models;
using mcLaunch.Views.Windows;

namespace mcLaunch.Views;

public partial class MinecraftVersionSelector : UserControl
{
    public MinecraftVersionSelector()
    {
        InitializeComponent();

        SetVersion(MinecraftManager.ManifestVersions[0]);
    }

    public ManifestMinecraftVersion Version { get; private set; }

    public event Action<ManifestMinecraftVersion>? OnVersionChanged;

    private async void ChangeMinecraftVersionButtonClicked(object? sender, RoutedEventArgs e)
    {
        VersionSelectWindow selectWindow = new();

        ManifestMinecraftVersion? newVersion = await selectWindow
            .ShowDialog<ManifestMinecraftVersion?>(MainWindow.Instance);

        if (newVersion == null) return;

        Version = newVersion;
        DataContext = Version;
        OnVersionChanged?.Invoke(Version);
    }

    public void SetVersion(ManifestMinecraftVersion version)
    {
        Version = version;
        DataContext = version;
        OnVersionChanged?.Invoke(Version);
    }
}