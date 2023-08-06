using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.GitHub.Models;
using mcLaunch.Managers;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views;

public partial class UpdateNotificationBar : UserControl
{
    public UpdateNotificationBar()
    {
        InitializeComponent();
    }

    private async void UpdateButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (!await UpdateManager.UpdateAsync())
        {
            Navigation.ShowPopup(new MessageBoxPopup("Error", "Update failed. You can download the update on the GitHub repository manually."));
            return;
        }
    }

    private void IgnoreButtonClicked(object? sender, RoutedEventArgs e)
    {
        IsVisible = false;
    }

    public void SetUpdateDetails(GitHubRelease release)
    {
        VersionNameText.Text = $"mcLaunch {release.Name.TrimStart('v')}";
    }
}