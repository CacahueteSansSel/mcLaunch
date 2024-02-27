using System.Diagnostics;
using System.IO;

namespace mcLaunch.Installer.Pages;

public partial class InstalledPage : InstallerPage
{
    public InstalledPage()
    {
        InitializeComponent();
    }

    public override void OnShow()
    {
        base.OnShow();

        MainWindow.Instance.HidePreviousButtons();
    }

    public override void OnHide()
    {
        base.OnHide();

        if (StartLauncherBox.IsChecked.HasValue && StartLauncherBox.IsChecked.Value)
        {
            string launcherExecutablePath =
                $"{MainWindow.Instance.Parameters.TargetDirectory}{Path.DirectorySeparatorChar}mcLaunch.exe";

            Process.Start(new ProcessStartInfo
            {
                FileName = launcherExecutablePath,
                WorkingDirectory = MainWindow.Instance.Parameters.TargetDirectory
            });
        }
    }
}