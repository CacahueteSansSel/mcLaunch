using System;
using System.Collections.Generic;
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
using mcLaunch.Views.Pages;
using ReactiveUI;

namespace mcLaunch.Views.Popups;

public partial class NewBoxPopup : UserControl
{
    public NewBoxPopup()
    {
        InitializeComponent();
        DataContext = new Data(this);
        Data ctx = (Data) DataContext;

        Random rng = new Random();

        Bitmap bmp = new Bitmap(AssetLoader.Open(new Uri($"avares://mcLaunch/resources/box_icons/{rng.Next(0, 4)}.png")));
        BoxIconImage.Source = bmp;

        if (AuthenticationManager.Account != null)
        {
            AuthorNameTb.Text = AuthenticationManager.Account.Username;
        }

        VersionSelector.OnVersionChanged += version =>
        {
            FetchModLoadersLatestVersions(version.Id);
        };

        FetchModLoadersLatestVersions(ctx.Versions[0].Id);
    }

    async void FetchModLoadersLatestVersions(string versionId)
    {
        CreateButton.IsEnabled = false;
        Data ctx = (Data) DataContext;
        List<ModLoaderSupport> all = new();

        foreach (ModLoaderSupport ml in ModLoaderManager.All)
        {
            ModLoaderVersion? version = await ml.FetchLatestVersion(versionId);
            if (version != null) all.Add(ml);
        }

        ctx.ModLoaders = all.ToArray();
        ctx.SelectedModLoader = ctx.ModLoaders[0];
        CreateButton.IsEnabled = true;
    }

    private void CloseButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private async void CreateBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        string boxName = BoxNameTb.Text;
        string boxAuthor = AuthorNameTb.Text;
        ManifestMinecraftVersion minecraftVersion = VersionSelector.Version;
        ModLoaderSupport modloader = ((Data) DataContext).SelectedModLoader;

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

        await BoxManager.Create(newBoxManifest);

        Navigation.HidePopup();

        MainPage.Instance?.PopulateBoxList();
    }

    public class Data : ReactiveObject
    {
        private NewBoxPopup popup;
        ModLoaderVersion latestVersion;
        ModLoaderSupport selectedModLoader;
        private ModLoaderSupport[] modLoaders;

        public ManifestMinecraftVersion[] Versions { get; }

        public ModLoaderSupport[] ModLoaders
        {
            get => modLoaders;
            set => this.RaiseAndSetIfChanged(ref modLoaders, value);
        }

        public ModLoaderSupport SelectedModLoader
        {
            get => selectedModLoader;
            set => this.RaiseAndSetIfChanged(ref selectedModLoader, value);
        }

        public ModLoaderVersion LatestVersion
        {
            get => latestVersion;
            set => this.RaiseAndSetIfChanged(ref latestVersion, value);
        }

        public Data(NewBoxPopup popup)
        {
            this.popup = popup;
            Versions = Settings.Instance.EnableSnapshots
                ? MinecraftManager.Manifest!.Versions
                : MinecraftManager.ManifestVersions;
            ModLoaders = ModLoaderManager.All.ToArray();

            selectedModLoader = ModLoaders[0];
        }
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

        Bitmap bmp = new Bitmap(AssetLoader.Open(new Uri($"avares://mcLaunch/resources/box_icons/{rng.Next(0, 4)}.png")));

        BoxIconImage.Source = bmp;
    }
}