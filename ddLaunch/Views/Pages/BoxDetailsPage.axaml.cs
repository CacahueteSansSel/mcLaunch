using System;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using ddLaunch.Core.Boxes;
using ddLaunch.Utilities;
using ddLaunch.Views.Popups;

namespace ddLaunch.Views.Pages;

public partial class BoxDetailsPage : UserControl
{
    public Box Box { get; }

    public BoxDetailsPage()
    {
        
    }
    
    public BoxDetailsPage(Box box)
    {
        InitializeComponent();
        
        Box = box;
        DataContext = box;
    }

    private async void RunButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new GameLaunchPopup());
        
        await Box.PrepareAsync();
        Box.Run();
        
        // TODO: watcher process that checks if minecraft is closed, then reopens the launcher
        Environment.Exit(0);
    }

    private void AddModsButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.Push(new ModSearchPage(Box));
    }
}