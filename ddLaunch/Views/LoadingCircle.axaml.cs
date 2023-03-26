using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ddLaunch.Views;

public partial class LoadingCircle : UserControl
{
    public LoadingCircle()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}