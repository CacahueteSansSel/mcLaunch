using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Managers;
using mcLaunch.Core.MinecraftFormats;

namespace mcLaunch.Views.Pages;

public partial class DefaultsPage : UserControl
{
    public DefaultsPage()
    {
        InitializeComponent();

        MinecraftOptions? options = DefaultsManager.LoadDefaultMinecraftOptions();
        DefaultOptionsStatusText.Text = options == null ? "No default options set" : "Default options are set";
        DefaultOptionsDeleteButton.IsEnabled = options != null;
    }

    private void DeleteButtonClicked(object? sender, RoutedEventArgs e)
    {
        DefaultsManager.ClearDefaultOptions();
        
        DefaultOptionsStatusText.Text = "No default options set";
        DefaultOptionsDeleteButton.IsEnabled = false;
    }
}