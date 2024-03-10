using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Managers;

namespace mcLaunch.Views.Pages;

public partial class BrowseModpacksPage : UserControl, ITopLevelPageControl
{
    private string lastMinecraftVersion = string.Empty;

    private string lastQuery = string.Empty;

    public BrowseModpacksPage()
    {
        InitializeComponent();

        if (!Design.IsDesignMode) LoadModpacksAsync();
    }

    public int PageIndex { get; private set; }

    public string Title => "Browse modpacks";

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        DiscordManager.SetPresenceModpacksList();
    }

    private async void Search(int page, string query, string minecraftVersion)
    {
        BoxContainer.Children.Clear();
        LoadingCircleIcon.IsVisible = true;
        NtsBanner.IsVisible = false;
        LoadMoreButton.IsVisible = false;

        PageIndex = 0;
        lastQuery = query;
        lastMinecraftVersion = minecraftVersion;

        PaginatedResponse<PlatformModpack> packs =
            await ModPlatformManager.Platform.GetModpacksAsync(page, query, minecraftVersion);
        foreach (PlatformModpack modpack in packs.Items)
            BoxContainer.Children.Add(new ModpackEntryCard(modpack));

        LoadingCircleIcon.IsVisible = false;
        NtsBanner.IsVisible = packs.Length == 0;
        LoadMoreButton.IsVisible = !NtsBanner.IsVisible;
    }

    private async void LoadModpacksAsync()
    {
        Search(0, "", "");
    }

    private void SearchButtonClicked(object? sender, RoutedEventArgs e)
    {
        Search(0, SearchTextboxInput.Text ?? string.Empty, "");
    }

    private async void LoadMoreButtonClicked(object? sender, RoutedEventArgs e)
    {
        LoadingButtonFrame.IsVisible = true;
        PageIndex++;

        PaginatedResponse<PlatformModpack> additionalPacks = await ModPlatformManager.Platform.GetModpacksAsync(
            PageIndex,
            lastQuery, lastMinecraftVersion);

        foreach (PlatformModpack modpack in additionalPacks.Items)
            BoxContainer.Children.Add(new ModpackEntryCard(modpack));
        
        LoadingButtonFrame.IsVisible = false;
    }
}