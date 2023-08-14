using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Mods;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class ModpackVersionSelectionPopup : UserControl
{
    private Action<PlatformModpack.ModpackVersion> versionSelectedCallback;
    private PlatformModpack.ModpackVersion? selectedVersion;

    public ModpackVersionSelectionPopup()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            DataContext = new PlatformModpack()
            {
                Name = "hello world",
                Versions = new[]
                {
                    new PlatformModpack.ModpackVersion()
                    {
                        MinecraftVersion = "1.18.2",
                        ModLoader = "Fabric",
                        Name = "1.0.0"
                    },
                    new PlatformModpack.ModpackVersion()
                    {
                        MinecraftVersion = "1.16.5",
                        ModLoader = "Fabric",
                        Name = "0.0.0"
                    }
                },
            };
        }
    }

    public ModpackVersionSelectionPopup(PlatformModpack modpack, Action<PlatformModpack.ModpackVersion> callback)
    {
        InitializeComponent();

        DataContext = modpack;
        versionSelectedCallback = callback;
    }

    private void VersionSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            selectedVersion = (PlatformModpack.ModpackVersion) e.AddedItems[0];
            InstallButton.IsVisible = true;
        }
    }

    private void VersionListDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (selectedVersion != null)
            Close();
    }

    void Close()
    {
        Navigation.HidePopup();
        versionSelectedCallback?.Invoke(selectedVersion);
    }

    private void InstallButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (selectedVersion != null)
            Close();
    }

    private void CloseButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }
}