using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Mods;

namespace mcLaunch.Views.Pages;

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
        ModList.SetModifications(Array.Empty<Modification>());
        
        ModList.Search(Box, SearchBoxInput.Text);
    }
}