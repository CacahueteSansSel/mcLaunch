using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Launchsite.Core;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents.Packs;
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

        BoxBinaryModificationPack bb = new(files[0]);

        Navigation.HidePopup();
        Navigation.ShowPopup(new StatusPopup($"Importing {bb.Name}", "Please wait for the modpack to be imported"));
        StatusPopup.Instance.ShowDownloadBanner = true;

        Result<Box> boxResult = await BoxManager.CreateFromModificationPack(bb, (msg, percent) =>
        {
            StatusPopup.Instance.Status = msg;
            StatusPopup.Instance.StatusPercent = percent;
        });
        if (boxResult.IsError)
        {
            boxResult.ShowErrorPopup();
            return;
        }

        Box box = boxResult.Data!;

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
        string[] files = await FileSystemUtilities.PickFiles(false, "Import a CurseForge modpack", ["zip"]);
        if (files.Length == 0) return;

        CurseForgeModificationPack modpack = new CurseForgeModificationPack(files[0]);

        Navigation.HidePopup();
        Navigation.ShowPopup(new StatusPopup($"Importing {modpack.Name}",
            "Please wait for the modpack to be imported"));
        StatusPopup.Instance.ShowDownloadBanner = true;

        Result<Box> boxResult = await BoxManager.CreateFromModificationPack(modpack, (msg, percent) =>
        {
            StatusPopup.Instance.Status = msg;
            StatusPopup.Instance.StatusPercent = percent;
        });
        if (boxResult.IsError)
        {
            boxResult.ShowErrorPopup();
            return;
        }

        Box box = boxResult.Data!;

        Navigation.HidePopup();

        Bitmap bmp = new Bitmap(AssetLoader.Open(new Uri("avares://mcLaunch/resources/default_cf_modpack_logo.png")));
        box.SetAndSaveIcon(bmp);

        Navigation.Push(new BoxDetailsPage(box));
        MainPage.Instance.PopulateBoxList();
    }

    private async void ImportModrinthModpackButtonClicked(object? sender, RoutedEventArgs e)
    {
        string[] files = await FileSystemUtilities.PickFiles(false, "Import a Modrinth modpack", ["mrpack"]);
        if (files.Length == 0) return;

        ModrinthModificationPack modpack = new ModrinthModificationPack(files[0]);

        Navigation.HidePopup();
        Navigation.ShowPopup(new StatusPopup($"Importing {modpack.Name}",
            "Please wait for the modpack to be imported"));

        StatusPopup.Instance.Status = "Resolving modifications...";

        await modpack.SetupAsync();

        StatusPopup.Instance.ShowDownloadBanner = true;

        Result<Box> boxResult = await BoxManager.CreateFromModificationPack(modpack, (msg, percent) =>
        {
            StatusPopup.Instance.Status = $"{msg}";
            StatusPopup.Instance.StatusPercent = percent;
        });
        if (boxResult.IsError)
        {
            boxResult.ShowErrorPopup();
            return;
        }

        Box box = boxResult.Data!;

        Navigation.HidePopup();

        if (File.Exists($"{box.Path}/minecraft/icon.png"))
        {
            Bitmap modpackIcon = new Bitmap($"{box.Path}/minecraft/icon.png");
            box.SetAndSaveIcon(modpackIcon);
        }
        else
        {
            Bitmap bmp = new Bitmap(AssetLoader.Open(new Uri("avares://mcLaunch/resources/default_box_logo.png")));
            box.SetAndSaveIcon(bmp);
        }

        Navigation.Push(new BoxDetailsPage(box));
        MainPage.Instance.PopulateBoxList();
    }
}