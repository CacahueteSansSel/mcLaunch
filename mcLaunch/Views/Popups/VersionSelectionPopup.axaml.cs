using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Core;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class VersionSelectionPopup : UserControl
{
    private readonly Action<IVersion> versionSelectedCallback;
    private IVersion? selectedVersion;

    public VersionSelectionPopup()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
            DataContext = new PlatformModpack
            {
                Name = "hello world",
                Versions = new[]
                {
                    new PlatformModpack.ModpackVersion
                    {
                        MinecraftVersion = "1.18.2",
                        ModLoader = "Fabric",
                        Name = "1.0.0"
                    },
                    new PlatformModpack.ModpackVersion
                    {
                        MinecraftVersion = "1.16.5",
                        ModLoader = "Fabric",
                        Name = "0.0.0"
                    }
                }
            };
    }

    public VersionSelectionPopup(IVersionContent content, Action<IVersion> callback)
    {
        InitializeComponent();

        DataContext = content;
        versionSelectedCallback = callback;
    }

    private void VersionSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            selectedVersion = (IVersion) e.AddedItems[0];
            InstallButton.IsVisible = true;
        }
    }

    private void VersionListDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (selectedVersion != null)
            Close();
    }

    private void Close()
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