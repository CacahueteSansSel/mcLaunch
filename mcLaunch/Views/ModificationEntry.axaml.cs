using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Contents.Platforms;

namespace mcLaunch.Views;

public partial class ModificationEntry : UserControl
{
    public static readonly AttachedProperty<MinecraftContent> ModProperty =
        AvaloniaProperty.RegisterAttached<MinecraftContent, UserControl, MinecraftContent>(
            nameof(Mod),
            null,
            inherits: true);

    public MinecraftContent Mod
    {
        get => (MinecraftContent) DataContext;
        set => DataContext = value;
    }

    public ModificationEntry()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            CacheManager.Init();
            Mod = new MinecraftContent
            {
                Name = "Sample Mod",
                Icon = IconCollection.Default,
                Author = "sample dev",
                ShortDescription = "sample desc",
                IsUpdateRequired = false,
                LastUpdated = DateTime.Today,
                DownloadCount = 2800,
                Platform = new ModrinthMinecraftContentPlatform()
            };
        }
    }
}