using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods;

namespace mcLaunch.Views.Pages;

public partial class BrowseModpacksPage : UserControl
{
    public int PageIndex { get; private set; }
    string lastQuery = string.Empty;
    string lastMinecraftVersion = string.Empty;
    
    public BrowseModpacksPage()
    {
        InitializeComponent();
        
        if (!Design.IsDesignMode) LoadModpacksAsync();
    }

    async void Search(int page, string query, string minecraftVersion)
    {
        BoxContainer.Children.Clear();
        LoadingCircleIcon.IsVisible = true;

        PageIndex = 0;
        lastQuery = query;
        lastMinecraftVersion = minecraftVersion;

        PaginatedResponse<PlatformModpack> packs = await ModPlatformManager.Platform.GetModpacksAsync(page, query, minecraftVersion);
        foreach (PlatformModpack modpack in packs.Items)
            BoxContainer.Children.Add(new ModpackEntryCard(modpack));
        
        LoadingCircleIcon.IsVisible = false;
    }

    async void LoadModpacksAsync() => Search(0, "", "");

    private void SearchButtonClicked(object? sender, RoutedEventArgs e)
    {
        Search(0, SearchTextboxInput.Text ?? string.Empty, "");
    }

    private async void LoadMoreButtonClicked(object? sender, RoutedEventArgs e)
    {
        PageIndex++;
        
        PaginatedResponse<PlatformModpack> additionalPacks = await ModPlatformManager.Platform.GetModpacksAsync(PageIndex, 
            lastQuery, lastMinecraftVersion);

        foreach (PlatformModpack modpack in additionalPacks.Items)
            BoxContainer.Children.Add(new ModpackEntryCard(modpack));
    }
}