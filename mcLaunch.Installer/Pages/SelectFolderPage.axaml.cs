using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;

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
        IReadOnlyList<IStorageFolder> result = await MainWindow.Instance.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions()
        {
            Title = "Select the target installation directory"
        });
        
        if (result == null || result.Count == 0) return;

        string? path = result.First().TryGetLocalPath();
        if (path == null) return;
        
        if (!string.IsNullOrWhiteSpace(path))
        {
            TargetPathInput.Text = path;
            MainWindow.Instance.Parameters.TargetDirectory = path;
        }
    }
}