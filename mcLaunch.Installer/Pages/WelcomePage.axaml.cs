using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Cacahuete.MinecraftLib.Models.GitHub;

namespace mcLaunch.Installer.Pages;

public partial class WelcomePage : UserControl
{
    public WelcomePage()
    {
        InitializeComponent();

        VersionText.Text =
            $"The installer will install mcLaunch {MainWindow.Instance.Parameters.ReleaseToDownload.Name} (latest)";
    }
}