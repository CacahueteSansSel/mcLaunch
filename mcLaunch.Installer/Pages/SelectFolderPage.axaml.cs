using Avalonia.Controls;
using Avalonia.Interactivity;

namespace mcLaunch.Installer.Pages;

public partial class SelectFolderPage : InstallerPage
{
    public SelectFolderPage()
    {
        InitializeComponent();

        TargetPathInput.Text = MainWindow.Instance.Parameters.TargetDirectory;
    }

    private async void SelectFolderButtonClicked(object? sender, RoutedEventArgs e)
    {
        OpenFolderDialog ofd = new();
        ofd.Title = "Select the target installation directory";

        string? path = await ofd.ShowAsync(MainWindow.Instance);
        if (!string.IsNullOrWhiteSpace(path))
        {
            TargetPathInput.Text = path;
            MainWindow.Instance.Parameters.TargetDirectory = path;
        }
    }
}