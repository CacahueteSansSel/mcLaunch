using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ddLaunch.Core.Boxes;
using ddLaunch.Core.Mods.Packs;
using ddLaunch.Utilities;
using ddLaunch.Views.Pages;

namespace ddLaunch.Views.Popups;

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

    private void ImportDdLaunchBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        
    }

    private async void ImportCurseForgeModpackButtonClicked(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Title = "Select a CurseForge modpack...";
        ofd.Filters = new List<FileDialogFilter>()
        {
            new()
            {
                Extensions = new List<string>()
                {
                    "zip"
                },
                Name = "Zip Archive"
            }
        };

        string[]? files = await ofd.ShowAsync(MainWindow.Instance);
        if (files == null) return;

        CurseForgeModificationPack modpack = new CurseForgeModificationPack(files[0]);
        Box box = await BoxManager.CreateFromModificationPack(modpack);
        
        Navigation.Push(new BoxDetailsPage(box));
        Navigation.HidePopup();
    }
}