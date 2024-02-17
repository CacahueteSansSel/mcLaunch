using System;
using System.Text.RegularExpressions;
using System.Web;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Mods;
using mcLaunch.GitHub.Models;
using mcLaunch.Managers;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class UpdateChangelogPopup : UserControl
{
    private GitHubRelease? _release;
    
    public UpdateChangelogPopup()
    {
        InitializeComponent();
    }

    public UpdateChangelogPopup(GitHubRelease release)
    {
        InitializeComponent();

        _release = release;
        DataContext = release;
    }

    private async void UpdateButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
        
        if (!await UpdateManager.UpdateAsync())
        {
            Navigation.ShowPopup(new MessageBoxPopup("Error", 
                "Update failed. You can download the update on the GitHub repository manually."));
        }
    }

    private void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }
}