﻿using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Utilities;

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
        string[] files = await FilePickerUtilities.PickFiles(false, "Import a box file", ["box"]);
        if (files.Length == 0) return;

        await BoxUtilities.ImportBoxAsync(files[0]);
    }

    private async void ImportCurseForgeModpackButtonClicked(object? sender, RoutedEventArgs e)
    {
        string[] files = await FilePickerUtilities.PickFiles(false, "Import a CurseForge modpack", ["zip"]);
        if (files.Length == 0) return;

        await BoxUtilities.ImportCurseforgeAsync(files[0]);
    }

    private async void ImportModrinthModpackButtonClicked(object? sender, RoutedEventArgs e)
    {
        string[] files = await FilePickerUtilities.PickFiles(false, "Import a Modrinth modpack", ["mrpack"]);
        if (files.Length == 0) return;

        await BoxUtilities.ImportModrinthAsync(files[0]);
    }
}