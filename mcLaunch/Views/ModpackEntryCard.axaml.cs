using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Utilities;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;

namespace mcLaunch.Views;

public partial class ModpackEntryCard : UserControl
{
    private PlatformModpack modpack;

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
        PlatformBadge.Text = modpack.Platform.Name;
        //PlatformBadge.SetIcon("mod");

        ApplyModpackValues();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        Navigation.Push(new ModpackDetailsPage(modpack));
    }

    private async void ApplyModpackValues()
    {
        // Download additional infos for the modpack
        modpack = await ModPlatformManager.Platform.GetModpackAsync(modpack.Id);
        
        Color accent = Color.FromUInt32(modpack.Color);
        Header.Background = new SolidColorBrush(new Color(255, accent.R, accent.G, accent.B));
        
        if (modpack == null)
        {
            IsEnabled = false;
            IsVisible = false;

            return;
        }

        DataContext = modpack;

        if (modpack.LatestVersion.ModLoader != null)
        {
            ModLoaderBadge.IsVisible = true;

            ModLoaderBadge.Text = modpack.LatestVersion.ModLoader.Capitalize();
            //ModLoaderBadge.SetIcon(modpack.LatestVersion.ModLoader.ToLower());
        }
        else
        {
            ModLoaderBadge.IsVisible = false;
        }
    }
}