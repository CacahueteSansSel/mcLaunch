﻿using System;
using Avalonia;
using Avalonia.Controls;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Contents.Platforms;
using mcLaunch.Core.Core;
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
                Type = MinecraftContentType.Modification,
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

    public MinecraftContent Mod
    {
        get => (MinecraftContent) DataContext;
        set => DataContext = value;
    }
}