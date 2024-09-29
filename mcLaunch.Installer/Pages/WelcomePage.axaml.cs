namespace mcLaunch.Installer.Pages;

public partial class WelcomePage : InstallerPage
{
    public WelcomePage()
    {
        InitializeComponent();
        if (MainWindow.Instance == null) return;

        VersionText.Text =
            $"The installer will install mcLaunch {MainWindow.Instance.Parameters.ReleaseToDownload.Name} (latest)";
    }
}