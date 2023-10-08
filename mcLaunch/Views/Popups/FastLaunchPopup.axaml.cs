using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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

public partial class FastLaunchPopup : UserControl
{
    public FastLaunchPopup()
    {
        InitializeComponent();
        DataContext = new Data();
        Data ctx = (Data) DataContext;

        VersionSelector.OnVersionChanged += version =>
        {
            FetchModLoadersLatestVersions(version.Id);
        };
        
        FetchModLoadersLatestVersions(ctx.Versions[0].Id);
    }

    async void FetchModLoadersLatestVersions(string versionId)
    {
        Data ctx = (Data) DataContext;
        List<ModLoaderSupport> all = new();

        foreach (ModLoaderSupport ml in ModLoaderManager.All)
        {
            ModLoaderVersion? version = await ml.FetchLatestVersion(versionId);
            if (version != null) all.Add(ml);
        }

        ctx.ModLoaders = all.ToArray();
    }

    private void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private async void LaunchButtonClicked(object? sender, RoutedEventArgs e)
    {
        ManifestMinecraftVersion minecraftVersion = VersionSelector.Version;
        ModLoaderSupport modloader = ((Data) DataContext).SelectedModLoader;
        string name = $"fastlaunch_{Guid.NewGuid()}";

        Navigation.HidePopup();
        Navigation.ShowPopup(new StatusPopup("Preparing launch", 
            $"Preparing launching Minecraft {minecraftVersion.Id}..."));

        Random rng = new Random(minecraftVersion.Id.GetHashCode());
        IconCollection icon = IconCollection.FromStream(
            AssetLoader.Open(new Uri($"avares://mcLaunch/resources/box_icons/{rng.Next(0, 4)}.png")));

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

        BoxManifest newBoxManifest = new BoxManifest(name, null, "FastLaunch", modloader.Id, modloaderVersions[0].Name,
            icon, minecraftVersion, BoxType.Temporary);

        string path = await BoxManager.Create(newBoxManifest);

        BoxDetailsPage detailsPage = new BoxDetailsPage(new Box(path));
        await detailsPage.RunAsync();
    }
    
    public class Data : ReactiveObject
    {
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

        public Data()
        {
            Versions = Settings.Instance.EnableSnapshots
                ? MinecraftManager.Manifest!.Versions
                : MinecraftManager.ManifestVersions;
            ModLoaders = ModLoaderManager.All.ToArray();

            selectedModLoader = ModLoaders[0];
        }
    }
}