using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using ddLaunch.Core.Boxes;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Mods;
using ddLaunch.Core.Mods.Packs;
using ddLaunch.Core.Utilities;
using ddLaunch.Utilities;
using ddLaunch.Views.Popups;

namespace ddLaunch.Views.Pages;

public partial class BoxDetailsPage : UserControl
{
    public Box Box { get; }

    public BoxDetailsPage()
    {
        InitializeComponent();
    }

    public BoxDetailsPage(Box box)
    {
        InitializeComponent();

        Box = box;
        DataContext = box;

        box.LoadBackground();

        PopulateStoredModsList();
    }

    async void PopulateStoredModsList()
    {
        ModsList.HideLoadMoreButton();
        ModsList.SetLoadingCircle(true);

        List<Modification> mods = new();

        foreach (BoxStoredModification storedMod in Box.Manifest.Modifications)
        {
            Modification mod = await ModPlatformManager.Platform.GetModAsync(storedMod.Id);
            mod.InstalledVersion = storedMod.VersionId;

            await mod.DownloadIconAsync();

            mods.Add(mod);
        }

        ModsList.SetBox(Box);
        ModsList.SetModifications(mods.ToArray());
        ModsList.SetLoadingCircle(false);

        List<Modification> updateMods = new();
        bool isChanges = false;

        foreach (Modification mod in mods)
        {
            string[] versions = await ModPlatformManager.Platform.GetVersionsForMinecraftVersionAsync(mod.Id,
                Box.Manifest.ModLoaderId, Box.Manifest.Version);

            mod.IsUpdateRequired = versions[0] != mod.InstalledVersion;
            if (mod.IsUpdateRequired) isChanges = true;

            updateMods.Add(mod);
        }
        
        if (isChanges) ModsList.SetModifications(updateMods.ToArray());
    }

    public async void Run()
    {
        if (Box.Manifest.ModLoader == null)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Can't run Minecraft",
                $"The modloader {Box.Manifest.ModLoaderId.Capitalize()} isn't supported"));

            return;
        }

        Navigation.ShowPopup(new GameLaunchPopup());

        await Box.PrepareAsync();
        Process java = Box.Run();

        // TODO: crash report parser
        // RegExp for mod dependencies error : /(Failure message): .+/g

        string backgroundProcessFilename
            = Path.GetFullPath("ddLaunch.MinecraftGuard" + (OperatingSystem.IsWindows() ? ".exe" : ""));

        if (File.Exists(backgroundProcessFilename))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = backgroundProcessFilename,
                Arguments = $"{java.Id.ToString()} {Box.Manifest.Id}",
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false
            });
        }

        Environment.Exit(0);
    }

    private async void RunButtonClicked(object? sender, RoutedEventArgs e)
    {
        Run();
    }

    private void AddModsButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.Push(new ModSearchPage(Box));
    }

    private async void EditBackgroundButtonClicked(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Title = "Select the background image...";
        ofd.Filters = new List<FileDialogFilter>()
        {
            new()
            {
                Extensions = new List<string>()
                {
                    "png"
                },
                Name = "PNG Image"
            }
        };

        string[]? files = await ofd.ShowAsync(MainWindow.Instance);
        if (files == null) return;

        Bitmap backgroundBitmap = new Bitmap(files[0]);
        Box.SetAndSaveBackground(backgroundBitmap);
    }

    private async void EditButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new EditBoxPopup(Box));
    }

    private void BoxIconCursorEntered(object? sender, PointerEventArgs e)
    {
        EditIconButton.IsVisible = true;
        EditIconButton.IsEnabled = true;
    }

    private void BoxIconCursorLeft(object? sender, PointerEventArgs e)
    {
        EditIconButton.IsVisible = false;
        EditIconButton.IsEnabled = false;
    }

    private async void EditIconButtonClicked(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Title = "Select the icon image...";
        ofd.Filters = new List<FileDialogFilter>()
        {
            new()
            {
                Extensions = new List<string>()
                {
                    "png"
                },
                Name = "PNG Image"
            }
        };

        string[]? files = await ofd.ShowAsync(MainWindow.Instance);
        if (files == null) return;

        Bitmap iconBitmap = new Bitmap(files[0]);
        Box.SetAndSaveIcon(iconBitmap);
    }

    private void OpenFolderButtonClicked(object? sender, RoutedEventArgs e)
    {
        PlatformSpecific.OpenFolder(Box.Path);
    }

    private async void ExportButtonClicked(object? sender, RoutedEventArgs e)
    {
        SaveFileDialog sfd = new SaveFileDialog();
        sfd.Title = $"Export {Box.Manifest.Name}";
        sfd.Filters = new List<FileDialogFilter>()
        {
            new()
            {
                Extensions = new List<string>()
                {
                    "box"
                },
                Name = "ddLaunch Box Binary File"
            }
        };

        string? filename = await sfd.ShowAsync(MainWindow.Instance);
        if (filename == null) return;

        Navigation.ShowPopup(new StatusPopup($"Exporting {Box.Manifest.Name}",
            "Please wait while we package your box in a file..."));

        BoxBinaryModificationPack bb = new();
        await bb.ExportAsync(Box, filename);

        Navigation.HidePopup();
        Navigation.ShowPopup(new MessageBoxPopup("Success !",
            $"The box {Box.Manifest.Name} have been exported successfully"));
    }

    private void DeleteBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new ConfirmMessageBoxPopup($"Delete {Box.Manifest.Name} ?",
            "Everything will be lost (mods, worlds, configs, etc.) and there is no coming back",
            () =>
            {
                try
                {
                    Directory.Delete(Box.Path, true);
                    MainPage.Instance.PopulateBoxList();
                    
                    Navigation.Pop();
                }
                catch (Exception exception)
                {
                    Navigation.ShowPopup(new MessageBoxPopup("Failed to delete box", $"Failed to delete the box {Box.Manifest.Name} : {exception.Message}"));
                }
            }));
    }
}