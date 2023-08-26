using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace mcLaunch.Views.Pages;

public partial class BrowseModsPage : UserControl
{
    public BrowseModsPage()
    {
        InitializeComponent();
        
        ModList.Search(null, string.Empty);
    }

    private void SearchButtonClicked(object? sender, RoutedEventArgs e)
    {
        ModList.Search(null, SearchTextboxInput.Text);
        ModList.ShowLoadMoreButton();
    }
}