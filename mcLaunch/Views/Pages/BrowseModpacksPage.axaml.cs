using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Mods;

namespace mcLaunch.Views.Pages;

public partial class BrowseModpacksPage : UserControl
{
    public BrowseModpacksPage()
    {
        InitializeComponent();
        
        if (!Design.IsDesignMode) LoadModpacksAsync();
    }

    async void Search(int page, string query, string minecraftVersion)
    {
        BoxContainer.Children.Clear();
        LoadingCircleIcon.IsVisible = true;

        PlatformModpack[] packs = await ModPlatformManager.Platform.GetModpacksAsync(page, query, minecraftVersion);

        foreach (PlatformModpack modpack in packs)
        {
            BoxContainer.Children.Add(new ModpackEntryCard(modpack));
        }
        
        LoadingCircleIcon.IsVisible = false;
    }

    async void LoadModpacksAsync() => Search(0, "", "");

    private void SearchButtonClicked(object? sender, RoutedEventArgs e)
    {
        Search(0, SearchTextboxInput.Text ?? string.Empty, "");
    }
}