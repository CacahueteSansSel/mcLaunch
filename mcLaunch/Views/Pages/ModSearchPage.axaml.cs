using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Mods;

namespace mcLaunch.Views.Pages;

public partial class ModSearchPage : UserControl, ITopLevelPageControl
{
    public Box Box { get; }
    public string Title => $"Browse mods for {Box.Manifest.Name} on {Box.Manifest.ModLoaderId} {Box.Manifest.Version}";

    public ModSearchPage()
    {
        InitializeComponent();
    }
    
    public ModSearchPage(Box box)
    {
        InitializeComponent();

        Box = box;
        DataContext = Box;

        DefaultBackground.IsVisible = box.Manifest.Background == null;
        
        ModList.Search(box, "");
    }

    private void SearchButtonClicked(object? sender, RoutedEventArgs e)
    {
        ModList.SetModifications(Array.Empty<Modification>());
        
        ModList.Search(Box, SearchBoxInput.Text);
    }
}