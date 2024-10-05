using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class GameLaunchPopup : UserControl
{
    public GameLaunchPopup()
    {
        InitializeComponent();

        if (Settings.Instance != null && !Settings.Instance.CloseLauncherAtLaunch)
            FooterText.Text = "This popup will close when Minecraft will start";
    }
}