using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ddLaunch.Core;
using ddLaunch.Core.Boxes;
using ddLaunch.Core.Managers;
using ddLaunch.Core.MinecraftFormats;
using ddLaunch.Core.Mods;
using ddLaunch.Core.Mods.Platforms;
using ddLaunch.Models;
using ddLaunch.Utilities;
using ddLaunch.Views.Pages;
using ReactiveUI;

namespace ddLaunch.Views;

public partial class WorldList : UserControl
{
    Box lastBox;
    string lastQuery;

    public bool HideInstalledBadges { get; set; }

    public WorldList()
    {
        InitializeComponent();

        DataContext = new Data();
        
        SetWorlds(new []
        {
            new MinecraftWorld
            {
                Name = "Survie hardcore",
                GameMode = MinecraftGameMode.Creative,
                Icon = null,
                LastPlayed = DateTime.Now
            }
        });
    }

    public void SetBox(Box box)
    {
        lastBox = box;
    }

    public void SetWorlds(MinecraftWorld[] worlds)
    {
        Data ctx = (Data) DataContext;

        ctx.Worlds = worlds;
    }

    public void SetLoadingCircle(bool isLoading)
    {
        LoadCircle.IsVisible = isLoading;
    }

    public class Data : ReactiveObject
    {
        MinecraftWorld[] worlds;
        int page;

        public MinecraftWorld[] Worlds
        {
            get => worlds;
            set => this.RaiseAndSetIfChanged(ref worlds, value);
        }

        public int Page
        {
            get => page;
            set => this.RaiseAndSetIfChanged(ref page, value);
        }
    }

    private void WorldSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        
    }
}