using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Cacahuete.MinecraftLib.Core.ModLoaders;
using Cacahuete.MinecraftLib.Models;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Utilities;
using mcLaunch.Views.DataContexts;
using mcLaunch.Views.Pages;
using ReactiveUI;

namespace mcLaunch.Views.Popups;

public partial class NewBoxPopup : UserControl
{
    public NewBoxPopup()
    {
        InitializeComponent();
        DataContext = new MinecraftVersionSelectionDataContext();

        Random rng = new Random();

        Bitmap bmp =
            new Bitmap(AssetLoader.Open(new Uri($"avares://mcLaunch/resources/box_icons/{rng.Next(0, 4)}.png")));
        BoxIconImage.Source = bmp;

        if (AuthenticationManager.Account != null)
        {
            AuthorNameTb.Text = AuthenticationManager.Account.Username;
        }

        VersionSelector.OnVersionChanged += version => { FetchModLoadersLatestVersions(version.Id); };

        FetchModLoadersLatestVersions(VersionSelector.Version.Id);
    }

    async void FetchModLoadersLatestVersions(string versionId)
    {
        CreateButton.IsEnabled = false;
        MinecraftVersionSelectionDataContext ctx = (MinecraftVersionSelectionDataContext) DataContext!;
        List<ModLoaderSupport> all = [];

        foreach (ModLoaderSupport ml in ModLoaderManager.All)
        {
            ModLoaderVersion? version = await ml.FetchLatestVersion(versionId);
            if (version != null) all.Add(ml);
        }

        ctx.ModLoaders = all.Select(m => new DataContextModLoader(m)).ToArray();
        ctx.SelectedModLoader = ctx.ModLoaders[0];
        CreateButton.IsEnabled = true;
    }

    private void CloseButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private async void CreateBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(BoxNameTb.Text) || string.IsNullOrWhiteSpace(AuthorNameTb.Text))
            return;
        
        string boxName = BoxNameTb.Text;
        string boxAuthor = AuthorNameTb.Text;
        ManifestMinecraftVersion minecraftVersion = VersionSelector.Version;
        ModLoaderSupport modloader = ((MinecraftVersionSelectionDataContext) DataContext).SelectedModLoader.ModLoader;

        if (string.IsNullOrWhiteSpace(boxName)
            || string.IsNullOrWhiteSpace(boxAuthor)
            || minecraftVersion == null)
        {
            return;
        }

        Navigation.HidePopup();
        Navigation.ShowPopup(new StatusPopup($"Creating {boxName}", "We are creating the box... Please wait..."));

        IconCollection icon;
        if (BoxIconImage.Source is Bitmap bitmap)
        {
            icon = await IconCollection.FromBitmapAsync(bitmap);
        }
        else
        {
            Random rng = new Random(BoxNameTb.Text.GetHashCode());
            icon = IconCollection.FromStream(
                AssetLoader.Open(new Uri($"avares://mcLaunch/resources/box_icons/{rng.Next(0, 4)}.png")));
        }

        // We fetch automatically the latest version of the modloader for now
        // TODO: Allow the user to select a specific modloader version
        ModLoaderVersion[]? modloaderVersions = await modloader.GetVersionsAsync(minecraftVersion.Id);

        if (modloaderVersions == null || modloaderVersions.Length == 0)
        {
            Navigation.HidePopup();
            Navigation.ShowPopup(new MessageBoxPopup("Failed to initialize the mod loader",
                $"Failed to get any version of {modloader.Name} for Minecraft {minecraftVersion.Id}"));
            return;
        }

        BoxManifest newBoxManifest = new BoxManifest(boxName, null, boxAuthor, modloader.Id, modloaderVersions[0].Name,
            icon, minecraftVersion);

        StatusPopup.Instance.ShowDownloadBanner = true;

        await BoxManager.Create(newBoxManifest);
        
        StatusPopup.Instance.ShowDownloadBanner = false;
        Navigation.HidePopup();

        MainPage.Instance?.PopulateBoxList();
    }

    private async void SelectFileButtonClicked(object? sender, RoutedEventArgs e)
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
        if (files == null || files.Length == 0) return;

        BoxIconImage.Source = new Bitmap(files[0]);
    }

    private void NewMinecraftVersionSelectedCallback(object? sender, SelectionChangedEventArgs e)
    {
    }

    private void BoxNameTextChanged(object? sender, KeyEventArgs e)
    {
        Random rng = new Random((BoxNameTb.Text ?? string.Empty).GetHashCode());

        Bitmap bmp =
            new Bitmap(AssetLoader.Open(new Uri($"avares://mcLaunch/resources/box_icons/{rng.Next(0, 4)}.png")));

        BoxIconImage.Source = bmp;
    }
}