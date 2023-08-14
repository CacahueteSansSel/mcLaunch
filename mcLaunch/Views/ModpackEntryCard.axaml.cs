using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods;
using mcLaunch.Core.Utilities;

namespace mcLaunch.Views;

public partial class ModpackEntryCard : UserControl
{
    PlatformModpack modpack;
    
    public ModpackEntryCard()
    {
        InitializeComponent();
    }

    public ModpackEntryCard(PlatformModpack modpack)
    {
        InitializeComponent();
        this.modpack = modpack;
        DataContext = modpack;

        VersionBadge.Text = modpack.LatestMinecraftVersion ?? "Unknown";
        
        DownloadBackgroundAndApplyAsync();
    }

    async void DownloadBackgroundAndApplyAsync()
    {
        Bitmap icon = modpack.Icon;
        
        // Download additional infos for the modpack
        modpack = await ModPlatformManager.Platform.GetModpackAsync(modpack.Id);
        modpack.Icon = icon;
        
        await modpack.DownloadBackgroundAsync();

        DataContext = modpack;
        
        ModLoaderBadge.Text = modpack.LatestVersion.ModLoader.Capitalize();
        ModLoaderBadge.Icon =
            new Bitmap(AssetLoader.Open(
                new Uri($"avares://mcLaunch/resources/icons/{modpack.LatestVersion.ModLoader.ToLower()}.png")));

        if (modpack.Background == null)
        {
            Color accent = Color.FromUInt32(modpack.Color);
            Header.Background = new SolidColorBrush(new Color(255, accent.R, accent.G, accent.B));
        }
    }
}