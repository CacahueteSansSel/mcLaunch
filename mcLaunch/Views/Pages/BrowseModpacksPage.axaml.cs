using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Managers;

namespace mcLaunch.Views.Pages;

public partial class BrowseModpacksPage : UserControl, ITopLevelPageControl
{
    private readonly PageSelector[] pageSelectors;
    private string lastMinecraftVersion = string.Empty;
    private string lastQuery = string.Empty;

    public BrowseModpacksPage()
    {
        InitializeComponent();

        pageSelectors =
        [
            TopPageSelector,
            BottomPageSelector
        ];

        foreach (PageSelector component in pageSelectors)
            component.IsVisible = false;

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
        foreach (PageSelector component in pageSelectors)
            component.IsEnabled = false;

        PageIndex = page;
        lastQuery = query;
        lastMinecraftVersion = minecraftVersion;

        PaginatedResponse<PlatformModpack> packs =
            await ModPlatformManager.Platform.GetModpacksAsync(page, query, minecraftVersion);
        foreach (PlatformModpack modpack in packs.Items)
            BoxContainer.Children.Add(new ModpackEntryCard(modpack));

        LoadingCircleIcon.IsVisible = false;
        NtsBanner.IsVisible = packs.Length == 0;
        foreach (PageSelector component in pageSelectors)
        {
            component.IsEnabled = true;
            component.IsVisible = !NtsBanner.IsVisible;
            component.Setup(packs.TotalPageCount, PageSelectedCallback);
            component.SetPage(PageIndex, false);
        }
    }

    private void PageSelectedCallback(int index)
    {
        foreach (PageSelector component in pageSelectors)
            component.IsEnabled = false;

        Search(index, lastQuery, lastMinecraftVersion);

        foreach (PageSelector component in pageSelectors)
            component.IsEnabled = true;
    }

    private void LoadModpacksAsync()
    {
        Search(0, "", "");
    }

    private void SearchButtonClicked(object? sender, RoutedEventArgs e)
    {
        Search(0, SearchTextboxInput.Text ?? string.Empty, "");
    }

    private async void LoadMoreButtonClicked(object? sender, RoutedEventArgs e)
    {
        foreach (PageSelector component in pageSelectors)
            component.IsEnabled = false;
        PageIndex++;

        PaginatedResponse<PlatformModpack> additionalPacks = await ModPlatformManager.Platform.GetModpacksAsync(
            PageIndex,
            lastQuery, lastMinecraftVersion);

        foreach (PlatformModpack modpack in additionalPacks.Items)
            BoxContainer.Children.Add(new ModpackEntryCard(modpack));

        foreach (PageSelector component in pageSelectors)
            component.IsEnabled = true;
    }
}