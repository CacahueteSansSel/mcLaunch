using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Contents.Packs;
using mcLaunch.Launchsite.Core;
using mcLaunch.Launchsite.Core.ModLoaders;
using mcLaunch.Launchsite.Models;
using mcLaunch.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;

namespace mcLaunch.Views.Popups;

public partial class InstallContentOnPopup : UserControl, INewBoxPopupListener
{
    public MinecraftContent ShownContent { get; private set; }
    List<string>? supportedModLoaders;
    List<string>? supportedMinecraftVersions;

    public InstallContentOnPopup()
    {
        InitializeComponent();
    }

    public InstallContentOnPopup(MinecraftContent content) : this()
    {
        ShownContent = content;

        DescText.Text = DescText.Text!.Replace("$MOD", content.Name);
        NewBoxText.Text = NewBoxText.Text!.Replace("$MOD", content.Name);
        ExistingBoxText.Text = ExistingBoxText.Text!.Replace("$MOD", content.Name);
    }

    private void ClosePopupButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private async void NewBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new NewBoxPopup(this));
    }

    private async void ExistingBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new SelectBoxPopup(async box =>
        {
            ContentVersion[] versions = await ShownContent.Platform!
                .GetContentVersionsAsync(ShownContent, box.Manifest.ModLoaderId, box.Manifest.Version);

            if (versions.Length == 0)
            {
                Navigation.ShowPopup(new MessageBoxPopup("Error", 
                    "No version compatible with this box has been found", MessageStatus.Error));
                
                return;
            }

            await ShownContent.Platform.InstallContentAsync(box, ShownContent, versions[0].Id, false, true);
            
            Navigation.Push(new BoxDetailsPage(box));
        }));
    }

    public string? CustomShownText => $"{ShownContent.Name} will be installed when the box is created";

    public async Task InitializeAsync()
    {
        if (ShownContent.Type != MinecraftContentType.Modification) return;

        supportedModLoaders = [];
        supportedMinecraftVersions = [];

        ContentVersion[] versions = await ShownContent.Platform!
            .GetContentVersionsAsync(ShownContent, null, null);

        foreach (ContentVersion version in versions)
        {
            if (version.ModLoader != null && !supportedModLoaders.Contains(version.ModLoader!))
                supportedModLoaders.Add(version.ModLoader);

            if (!supportedMinecraftVersions.Contains(version.MinecraftVersion))
                supportedMinecraftVersions.Add(version.MinecraftVersion);
        }
    }

    public bool ShouldShowModLoader(ModLoaderSupport modLoader)
    {
        return supportedModLoaders == null
            ? modLoader is not VanillaModLoaderSupport && modLoader is not DirectJarMergingModLoaderSupport
            : supportedModLoaders.Contains(modLoader.Id);
    }

    public bool ShouldShowMinecraftVersion(ManifestMinecraftVersion version)
        => supportedMinecraftVersions?.Contains(version.Id) ?? true;

    public async Task<bool> WhenBoxCreatedAsync(Box box)
    {
        MinecraftContentPlatform? platform = ShownContent.Platform;
        if (platform == null)
        {
            Console.WriteLine("Warning: Content's platform is null");
            return true;
        }

        ContentVersion[] versions =
            await platform.GetContentVersionsAsync(ShownContent, box.ModLoader!.Id, box.Manifest.Version);

        if (versions == null || versions.Length == 0)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Error",
                $"Failed to add {ShownContent.Name}: could not find a mod version for Minecraft {box.Manifest.Version} on {box.ModLoader!.Id}",
                MessageStatus.Error));

            await Task.Run(() =>
            {
                while (MainWindowDataContext.Instance.IsPopup)
                    Task.Delay(10);
            });
            
            return true;
        }

        if (!await platform.InstallContentAsync(box, ShownContent, versions[0].Id, false, true))
        {
            Console.WriteLine("Warning: Failed to install content on box");
            return true;
        }

        return true;
    }

    public async Task WhenCancelledAsync()
    {
    }
}