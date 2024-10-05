using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Managers;

namespace mcLaunch.Views.Pages;

public partial class BrowseModsPage : UserControl, ITopLevelPageControl
{
    public BrowseModsPage()
    {
        InitializeComponent();

        ModList.Search(null, string.Empty);
    }

    public string Title => "Browse mods";

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        DiscordManager.SetPresenceModsList();
    }

    private void SearchButtonClicked(object? sender, RoutedEventArgs e)
    {
        ModList.Search(null, SearchTextboxInput.Text);
    }

    void UpButtonClicked(object? sender, RoutedEventArgs e)
    {
        ScrollArea.Offset = Vector.Zero;
    }
}