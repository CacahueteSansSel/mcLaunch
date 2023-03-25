using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ddLaunch.Views;

public partial class HeaderBar : UserControl
{
    public HeaderBar()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}