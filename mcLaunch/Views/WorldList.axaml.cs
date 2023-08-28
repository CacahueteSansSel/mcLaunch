using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods;
using mcLaunch.Core.Mods.Platforms;
using mcLaunch.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.MinecraftFormats;
using mcLaunch.Views.Popups;
using ReactiveUI;

namespace mcLaunch.Views;

public partial class WorldList : UserControl
{
    Box lastBox;
    string lastQuery;
    private BoxDetailsPage launchPage;

    public bool HideInstalledBadges { get; set; }

    public WorldList()
    {
        InitializeComponent();

        DataContext = new Data();
        
        /*
        SetWorlds(new []
        {
            new MinecraftWorld
            {
                Name = "Survie hardcore",
                GameMode = MinecraftGameMode.Creative,
                Icon = null,
                LastPlayed = DateTime.Now,
                IsCheats = true,
                Version = "1.19.84"
            }
        });
        */
    }

    public void SetBox(Box box)
    {
        lastBox = box;
    }

    public void SetLaunchPage(BoxDetailsPage page)
    {
        launchPage = page;
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
        if (e.AddedItems.Count > 0 && launchPage != null && launchPage.Box.SupportsQuickPlay)
        {
            MinecraftWorld world = (MinecraftWorld) e.AddedItems[0];

            Navigation.ShowPopup(new ConfirmMessageBoxPopup($"Launch world {world.Name} ?",
                $"Minecraft will start and automatically launch the world {world.Name}",
                () => { launchPage.Run(world: world); }));
        }
        
        WorldsList.UnselectAll();
    }
}