using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Managers;
using mcLaunch.Core.MinecraftFormats;

namespace mcLaunch.Views.Pages;

public partial class DefaultsPage : UserControl, ITopLevelPageControl
{
    public DefaultsPage()
    {
        InitializeComponent();

        MinecraftOptions? options = DefaultsManager.LoadDefaultMinecraftOptions();
        DefaultOptionsStatusText.Text = options == null ? "No default options set" : "Default options are set";
        DefaultOptionsDeleteButton.IsEnabled = options != null;
    }

    public string Title => "Manage defaults";

    private void DeleteButtonClicked(object? sender, RoutedEventArgs e)
    {
        DefaultsManager.ClearDefaultOptions();

        DefaultOptionsStatusText.Text = "No default options set";
        DefaultOptionsDeleteButton.IsEnabled = false;
    }
}