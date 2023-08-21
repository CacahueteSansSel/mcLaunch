using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace mcLaunch.Views.Pages;

public partial class DefaultsPage : UserControl
{
    public DefaultsPage()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}