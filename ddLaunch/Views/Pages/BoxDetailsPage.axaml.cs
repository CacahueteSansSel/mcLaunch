using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Logging;
using Avalonia.Markup.Xaml;
using ddLaunch.Core.Boxes;
using ddLaunch.Core.Managers;
using ddLaunch.Core.Mods;
using ddLaunch.Utilities;
using ddLaunch.Views.Popups;

namespace ddLaunch.Views.Pages;

public partial class BoxDetailsPage : UserControl
{
    public Box Box { get; }

    public BoxDetailsPage()
    {
        InitializeComponent();
    }
    
    public BoxDetailsPage(Box box)
    {
        InitializeComponent();
        
        Box = box;
        DataContext = box;

        PopulateStoredModsList();
    }

    async void PopulateStoredModsList()
    {
        ModsList.HideLoadMoreButton();
        ModsList.SetLoadingCircle(true);
        
        List<Modification> mods = new();

        foreach (BoxStoredModification storedMod in Box.Manifest.Modifications)
        {
            Modification mod = await ModPlatformManager.Platform.GetModAsync(storedMod.Id);
            await mod.DownloadIconAsync();
            
            mods.Add(mod);
        }
        
        ModsList.SetModifications(mods.ToArray());
        ModsList.SetLoadingCircle(false);
        ModsList.SetBox(Box);
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