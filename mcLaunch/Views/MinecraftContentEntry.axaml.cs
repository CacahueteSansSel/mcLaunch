using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Contents.Platforms;
using mcLaunch.Core.Managers;

namespace mcLaunch.Views;

public partial class MinecraftContentEntry : UserControl
{
    public static readonly AttachedProperty<MinecraftContent> ModProperty =
        AvaloniaProperty.RegisterAttached<MinecraftContent, UserControl, MinecraftContent>(
            nameof(Mod));

    public MinecraftContentEntry()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            CacheManager.Init();
            Mod = new MinecraftContent
            {
                Name = "Sample Mod",
                IconUrl = "/resources/icons/download.png",
                Type = MinecraftContentType.Modification,
                Author = "sample dev",
                ShortDescription = "sample desc",
                IsUpdateRequired = false,
                LastUpdated = DateTime.Today,
                DownloadCount = 2800,
                Platform = new ModrinthMinecraftContentPlatform()
            };
        }
    }

    public MinecraftContent Mod
    {
        get => (MinecraftContent)DataContext;
        set
        {
            DataContext = value;

            UpdateBadges();
        }
    }

    public void UpdateBadges()
    {
        InstalledBadge.IsVisible = Mod.IsInstalledOnCurrentBoxUi;
        UpdateBadge.IsVisible = Mod.IsUpdateRequired;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        UpdateBadges();
    }
}