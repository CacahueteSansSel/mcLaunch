using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace ddLaunch.Views;

public partial class HeaderBar : UserControl
{
    public HeaderBar()
    {
        InitializeComponent();

        IsVisible = !OperatingSystem.IsLinux();
        Logo.SetValue(DockPanel.DockProperty, OperatingSystem.IsMacOS() ? Dock.Right : Dock.Left);
        Logo.Margin = new Thickness(15, 5, OperatingSystem.IsMacOS() ? -150 : 0, 0);
    }
}