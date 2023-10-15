﻿using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Managers;
using mcLaunch.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;

namespace mcLaunch.Views.Popups;

public partial class DataMigrationPopup : UserControl
{
    public static bool IsActive { get; private set; }
    
    public DataMigrationPopup()
    {
        InitializeComponent();

        TargetPathBox.Text = AppdataFolderManager.Path
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        IsActive = true;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        IsActive = false;
    }

    private async void StartMigrationButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();

        StatusPopup statusPopup = new StatusPopup("Migrating data", "Please wait while we migrate your data...");
        Navigation.ShowPopup(statusPopup);

        await AppdataFolderManager.MigrateToAppdataAsync((status, percent) =>
        {
            statusPopup.Status = status;
            statusPopup.StatusPercent = percent;
        });
        
        Navigation.ShowPopup(new MessageBoxPopup("Migration successful", "All data have been migrated to the target location"));
    }

    private void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }
}