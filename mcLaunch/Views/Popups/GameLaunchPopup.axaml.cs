using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace mcLaunch.Views.Popups;

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