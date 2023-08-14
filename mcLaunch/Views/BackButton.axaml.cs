using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Models;
using mcLaunch.Views.Pages;
using mcLaunch.Utilities;

namespace mcLaunch.Views;

public partial class BackButton : UserControl
{
    public BackButton()
    {
        InitializeComponent();
    }

    private void BackButton_OnClick(object? sender, RoutedEventArgs e)
    {
        Navigation.Pop();
    }
}