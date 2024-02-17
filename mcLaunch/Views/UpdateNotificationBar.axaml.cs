using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.GitHub;
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
        Navigation.ShowPopup(new UpdateChangelogPopup(await GitHubRepository.GetLatestReleaseAsync()));
    }

    private void IgnoreButtonClicked(object? sender, RoutedEventArgs e)
    {
        IsVisible = false;
    }

    public void SetUpdateDetails(GitHubRelease release)
    {
        VersionNameText.Text = release.Name.TrimStart('v');
    }
}