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
        Logo.IsVisible = !OperatingSystem.IsMacOS();
    }
}