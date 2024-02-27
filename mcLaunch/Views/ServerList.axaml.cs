using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.MinecraftFormats;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;
using ReactiveUI;

namespace mcLaunch.Views;

public partial class ServerList : UserControl
{
    private Box lastBox;
    private string lastQuery;
    private BoxDetailsPage launchPage;

    public ServerList()
    {
        InitializeComponent();

        DataContext = new Data();
        if (Design.IsDesignMode) SetDefaultServers();
    }

    public bool HideInstalledBadges { get; set; }

    private async void SetDefaultServers()
    {
        await SetServersAsync([
            new MinecraftServer
            {
                IsHidden = false,
                Address = "example.com",
                IconData = Array.Empty<byte>(),
                Name = "Test Server",
                Port = "25565"
            }
        ]);
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

        NtsBanner.IsVisible = servers.Length == 0;
    }

    private async Task LoadServerIconsAsync(MinecraftServer[] servers)
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

    public class Data : ReactiveObject
    {
        private int page;
        private MinecraftServer[] servers;

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
}