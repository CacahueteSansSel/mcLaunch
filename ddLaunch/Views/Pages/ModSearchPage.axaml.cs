using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ddLaunch.Core.Boxes;

namespace ddLaunch.Views.Pages;

public partial class ModSearchPage : UserControl
{
    public Box Box { get; }

    public ModSearchPage()
    {
        InitializeComponent();
    }
    
    public ModSearchPage(Box box)
    {
        InitializeComponent();

        Box = box;
        DataContext = Box;
        
        ModList.Search(box, "");
    }

    private void SearchButtonClicked(object? sender, RoutedEventArgs e)
    {
        ModList.Search(Box, SearchBoxInput.Text);
    }
}