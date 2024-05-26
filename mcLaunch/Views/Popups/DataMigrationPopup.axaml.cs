using System;
using System.IO;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Managers;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class DataMigrationPopup : UserControl
{
    public DataMigrationPopup()
    {
        InitializeComponent();

        TargetPathBox.Text = AppdataFolderManager.Path
            .Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);
    }

    public static bool IsActive { get; private set; }

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

        Navigation.ShowPopup(new MessageBoxPopup("Migration successful",
            "All data have been migrated to the target location", MessageStatus.Success));
    }

    private void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }
}