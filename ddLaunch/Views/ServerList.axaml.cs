using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using ddLaunch.Views.Popups;
using ReactiveUI;

namespace ddLaunch.Views;

public partial class ServerList : UserControl
{
    private Box lastBox;
    private BoxDetailsPage launchPage;
    private string lastQuery;

    public bool HideInstalledBadges { get; set; }

    public ServerList()
    {
        InitializeComponent();

        DataContext = new Data();
        //SetDefaultServers();
    }

    async void SetDefaultServers()
    {
        await SetServersAsync(new MinecraftServer[]
        {
            new MinecraftServer
            {
                IsHidden = false,
                Address = "machintruc.fr",
                IconData = Array.Empty<byte>(),
                Name = "Le club de test",
                Port = "25565"
            }
        });
    }

    public void SetLaunchPage(BoxDetailsPage page)
    {
        launchPage = page;
    }

    public void SetBox(Box box)
    {
        lastBox = box;
    }

    public async Task SetServersAsync(MinecraftServer[] servers)
    {
        await LoadServerIconsAsync(servers);

        Data ctx = (Data) DataContext;
        ctx.Servers = servers;
    }

    async Task LoadServerIconsAsync(MinecraftServer[] servers)
    {
        Data ctx = (Data) DataContext;

        SetLoadingCircle(true);

        foreach (MinecraftServer server in servers)
            await server.LoadIconAsync();

        SetLoadingCircle(false);
    }

    public void SetLoadingCircle(bool isLoading)
    {
        LoadCircle.IsVisible = isLoading;
    }

    public class Data : ReactiveObject
    {
        MinecraftServer[] servers;
        int page;

        public MinecraftServer[] Servers
        {
            get => servers;
            set => this.RaiseAndSetIfChanged(ref servers, value);
        }

        public int Page
        {
            get => page;
            set => this.RaiseAndSetIfChanged(ref page, value);
        }
    }

    private void WorldSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && launchPage != null)
        {
            MinecraftServer server = (MinecraftServer) e.AddedItems[0];

            Navigation.ShowPopup(new ConfirmMessageBoxPopup($"Connect to {server.Name} ?",
                $"Minecraft will start and automatically connect to {server.Name} at {server.Address}",
                () => { launchPage.Run(server.Address, server.Port); }));
        }

        ServersList.UnselectAll();
    }
}