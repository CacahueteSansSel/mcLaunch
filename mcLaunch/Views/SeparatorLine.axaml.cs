using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace mcLaunch.Views;

public partial class SeparatorLine : UserControl
{
    public SeparatorLine()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}