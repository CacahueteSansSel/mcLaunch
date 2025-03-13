namespace mcLaunch.Installer.Pages;

public partial class FailedPage : InstallerPage
{
    public FailedPage()
    {
        InitializeComponent();
    }

    public FailedPage(string message) : this()
    {
        ErrorDetailsText.Text = message;
    }

    public override void OnShow()
    {
        base.OnShow();

        MainWindow.Instance.HidePreviousButtons();
    }
}