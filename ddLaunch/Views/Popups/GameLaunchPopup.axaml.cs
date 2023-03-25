using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ddLaunch.Views.Popups;

public partial class GameLaunchPopup : UserControl
{
    public GameLaunchPopup()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}