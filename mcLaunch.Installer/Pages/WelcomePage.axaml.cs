namespace mcLaunch.Installer.Pages;

public partial class WelcomePage : InstallerPage
{
    public WelcomePage()
    {
        InitializeComponent();

        VersionText.Text =
            $"The installer will install mcLaunch {MainWindow.Instance.Parameters.ReleaseToDownload.Name} (latest)";
    }
}