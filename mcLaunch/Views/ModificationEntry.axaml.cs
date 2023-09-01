using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods;
using mcLaunch.Core.Mods.Platforms;

namespace mcLaunch.Views;

public partial class ModificationEntry : UserControl
{
    public static readonly AttachedProperty<Modification> ModProperty =
        AvaloniaProperty.RegisterAttached<Modification, UserControl, Modification>(
            nameof(Mod),
            null,
            inherits: true);

    public Modification Mod
    {
        get => (Modification) DataContext;
        set => DataContext = value;
    }

    public ModificationEntry()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            CacheManager.Init();
            Mod = new Modification
            {
                Name = "Sample Mod",
                Icon = new(""),
                Author = "sample dev",
                ShortDescription = "sample desc",
                IsUpdateRequired = false,
                LastUpdated = DateTime.Today,
                DownloadCount = 2800,
                Platform = new ModrinthModPlatform()
            };
        }
    }
}