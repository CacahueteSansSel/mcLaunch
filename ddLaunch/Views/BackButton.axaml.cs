using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ddLaunch.Models;
using ddLaunch.Utilities;
using ddLaunch.Views.Pages;

namespace ddLaunch.Views;

public partial class BackButton : UserControl
{
    public BackButton()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void BackButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Navigation.Pop();
    }
}