using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Mods.Packs;
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

    private async void ImportDdLaunchBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Title = "Select a mcLaunch Box Binary file...";
        ofd.Filters = new List<FileDialogFilter>()
        {
            new()
            {
                Extensions = new List<string>()
                {
                    "box"
                },
                Name = "mcLaunch Box Binary file"
            }
        };

        string[]? files = await ofd.ShowAsync(MainWindow.Instance);
        if (files == null) return;

        BoxBinaryModificationPack bb = new(files[0]);
        
        Navigation.HidePopup();
        Navigation.ShowPopup(new StatusPopup($"Importing {bb.Name}", "Please wait for the modpack to be imported"));
        
        Box box = await BoxManager.CreateFromModificationPack(bb, (msg, percent) =>
        {
            StatusPopup.Instance.Status = msg;
            StatusPopup.Instance.StatusPercent = percent;
        });

        try
        {
            box.SetAndSaveIcon(new Bitmap(new MemoryStream(bb.IconData)));
            box.SetAndSaveBackground(new Bitmap(new MemoryStream(bb.BackgroundData)));
        }
        catch (Exception exception)
        {
            
        }
        
        Navigation.HidePopup();
        
        Navigation.Push(new BoxDetailsPage(box));
        MainPage.Instance.PopulateBoxList();
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
        
        Navigation.HidePopup();
        Navigation.ShowPopup(new StatusPopup($"Importing {modpack.Name}", "Please wait for the modpack to be imported"));
        
        Box box = await BoxManager.CreateFromModificationPack(modpack, (msg, percent) =>
        {
            StatusPopup.Instance.Status = msg;
            StatusPopup.Instance.StatusPercent = percent;
        });
        
        Navigation.HidePopup();
        
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        Bitmap bmp = new Bitmap(assets.Open(new Uri($"avares://mcLaunch/resources/default_cf_modpack_logo.png")));
        box.SetAndSaveIcon(bmp);
        
        Navigation.Push(new BoxDetailsPage(box));
        MainPage.Instance.PopulateBoxList();
    }

    private async void ImportModrinthModpackButtonClicked(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Title = "Select a Modrinth modpack...";
        ofd.Filters = new List<FileDialogFilter>()
        {
            new()
            {
                Extensions = new List<string>()
                {
                    "mrpack"
                },
                Name = "Modrinth Modpack"
            }
        };

        string[]? files = await ofd.ShowAsync(MainWindow.Instance);
        if (files == null) return;

        ModrinthModificationPack modpack = new ModrinthModificationPack(files[0]);
        
        Navigation.HidePopup();
        Navigation.ShowPopup(new StatusPopup($"Importing {modpack.Name}", "Please wait for the modpack to be imported"));
        
        Box box = await BoxManager.CreateFromModificationPack(modpack, (msg, percent) =>
        {
            StatusPopup.Instance.Status = msg;
            StatusPopup.Instance.StatusPercent = percent;
        });
        
        Navigation.HidePopup();

        if (File.Exists($"{box.Path}/minecraft/icon.png"))
        {
            Bitmap modpackIcon = new Bitmap($"{box.Path}/minecraft/icon.png");
            box.SetAndSaveIcon(modpackIcon);
        }
        else
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            Bitmap bmp = new Bitmap(assets.Open(new Uri($"avares://mcLaunch/resources/default_box_logo.png")));
            box.SetAndSaveIcon(bmp);
        }

        Navigation.Push(new BoxDetailsPage(box));
        MainPage.Instance.PopulateBoxList();
    }
}