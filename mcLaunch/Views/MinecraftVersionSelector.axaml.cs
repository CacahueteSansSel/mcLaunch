using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Cacahuete.MinecraftLib.Models;
using mcLaunch.Core.Managers;
using mcLaunch.Views.Windows;

namespace mcLaunch.Views;

public partial class MinecraftVersionSelector : UserControl
{
    public ManifestMinecraftVersion Version { get; private set; }

    public event Action<ManifestMinecraftVersion>? OnVersionChanged; 

    public MinecraftVersionSelector()
    {
        InitializeComponent();
        
        SetVersion(MinecraftManager.ManifestVersions[0]);
    }

    private async void ChangeMinecraftVersionButtonClicked(object? sender, RoutedEventArgs e)
    {
        VersionSelectWindow selectWindow = new();

        Version = await selectWindow.ShowDialog<ManifestMinecraftVersion>(MainWindow.Instance) 
                  ?? MinecraftManager.ManifestVersions[0];
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