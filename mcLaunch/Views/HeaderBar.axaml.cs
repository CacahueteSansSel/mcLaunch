using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Cacahuete.MinecraftLib.Http;

namespace mcLaunch.Views;

public partial class HeaderBar : UserControl
{
    public HeaderBar()
    {
        InitializeComponent();

        IsVisible = !OperatingSystem.IsLinux();
        Logo.SetValue(DockPanel.DockProperty, OperatingSystem.IsMacOS() ? Dock.Right : Dock.Left);
        Logo.Margin = new Thickness(15, 5, OperatingSystem.IsMacOS() ? -150 : 0, 0);
        
        Api.OnNetworkError += OnApiNetworkError;
        Api.OnNetworkSuccess += OnApiNetworkSuccess;
    }

    private void OnApiNetworkSuccess(string url)
    {
        OfflineBadge.IsVisible = false;
    }

    private void OnApiNetworkError(string url)
    {
        OfflineBadge.IsVisible = true;
    }
}