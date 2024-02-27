using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Boxes;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class NewBackupPopup : UserControl
{
    private readonly Box box;

    public NewBackupPopup()
    {
        InitializeComponent();
    }

    public NewBackupPopup(Box box)
    {
        InitializeComponent();

        this.box = box;
        BackupOfBoxCard.SetBox(box);
    }

    private void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private async void CreateBackupButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(BackupNameTb.Text)) return;

        CreateBackupButtonText.IsVisible = false;
        LoadingCircle.IsVisible = true;

        IsEnabled = false;

        bool isComplete = CompleteBackupRadioButton.IsChecked ?? false;

        BoxBackup? backup = null;

        if (isComplete) backup = await box.CreateBackupAsync(BackupNameTb.Text);

        Navigation.HidePopup();

        if (backup == null)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Backup failed", "Failed to create backup"));
            return;
        }

        Navigation.ShowPopup(new MessageBoxPopup("Backup created",
            $"Your backup {backup.Name} for {box.Manifest.Name} has been created"));
    }
}