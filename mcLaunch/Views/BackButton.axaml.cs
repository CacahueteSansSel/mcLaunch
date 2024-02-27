using Avalonia.Controls;
using Avalonia.Interactivity;
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