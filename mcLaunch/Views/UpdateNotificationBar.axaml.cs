using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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
            Navigation.ShowPopup(new MessageBoxPopup("Error", "Update failed"));
            return;
        }
    }

    private void IgnoreButtonClicked(object? sender, RoutedEventArgs e)
    {
        IsVisible = false;
    }
}