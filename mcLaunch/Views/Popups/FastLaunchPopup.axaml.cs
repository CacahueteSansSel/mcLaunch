using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
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
        LaunchButton.IsEnabled = false;
        Data ctx = (Data) DataContext;
        List<ModLoaderSupport> all = new();

        foreach (ModLoaderSupport ml in ModLoaderManager.All)
        {
            ModLoaderVersion? version = await ml.FetchLatestVersion(versionId);
            if (version != null) all.Add(ml);
        }

        ctx.ModLoaders = all.ToArray();
        ctx.SelectedModLoader = ctx.ModLoaders[0];
        LaunchButton.IsEnabled = true;
    }

    private void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private async void LaunchButtonClicked(object? sender, RoutedEventArgs e)
    {
        ManifestMinecraftVersion minecraftVersion = VersionSelector.Version;
        ModLoaderSupport modloader = ((Data) DataContext).SelectedModLoader;
        string name = $"Minecraft {minecraftVersion.Id}";

        Navigation.HidePopup();
        Navigation.ShowPopup(new StatusPopup("Preparing launch", 
            $"Preparing launching {name}..."));

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
            null, minecraftVersion, BoxType.Temporary);

        string path = await BoxManager.Create(newBoxManifest);
        Box box = new Box(path);
        box.SetAndSaveIcon(new Bitmap(AssetLoader.Open(
            new Uri("avares://mcLaunch/resources/fastlaunch_box_logo.png"))));

        BoxDetailsPage detailsPage = new BoxDetailsPage(box);
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
            
            // TODO: Allow all modloaders for FastLaunch
            
            ModLoaders = new[]
            {
                new VanillaModLoaderSupport()
            };

            selectedModLoader = ModLoaders[0];
        }
    }
}