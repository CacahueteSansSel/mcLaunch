using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents.Packs;
using mcLaunch.Launchsite.Core;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;

namespace mcLaunch.Views.Popups;

public partial class ImportBoxPopup : UserControl
{
    public ImportBoxPopup()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void ClosePopupButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private async void ImportMcLaunchBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        string[] files = await FileSystemUtilities.PickFiles(false, "Import a box file", ["box"]);
        if (files.Length == 0) return;

        await BoxImportUtilities.ImportBoxAsync(files[0]);
    }

    private async void ImportCurseForgeModpackButtonClicked(object? sender, RoutedEventArgs e)
    {
        string[] files = await FileSystemUtilities.PickFiles(false, "Import a CurseForge modpack", ["zip"]);
        if (files.Length == 0) return;

        await BoxImportUtilities.ImportCurseforgeAsync(files[0]);
    }

    private async void ImportModrinthModpackButtonClicked(object? sender, RoutedEventArgs e)
    {
        string[] files = await FileSystemUtilities.PickFiles(false, "Import a Modrinth modpack", ["mrpack"]);
        if (files.Length == 0) return;

        await BoxImportUtilities.ImportModrinthAsync(files[0]);
    }
}