using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Managers;

namespace mcLaunch.Views.Pages;

public partial class BrowseModsPage : UserControl, ITopLevelPageControl
{
    public string Title => $"Browse mods";

    public BrowseModsPage()
    {
        InitializeComponent();
        
        ModList.Search(null, string.Empty);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        DiscordManager.SetPresenceModsList();
    }

    private void SearchButtonClicked(object? sender, RoutedEventArgs e)
    {
        ModList.Search(null, SearchTextboxInput.Text);
    }
}