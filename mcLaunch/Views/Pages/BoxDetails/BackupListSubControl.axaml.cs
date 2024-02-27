using System.Threading.Tasks;
using Avalonia.Interactivity;
using mcLaunch.Core.Boxes;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class BackupListSubControl : SubControl
{
    public BackupListSubControl()
    {
        InitializeComponent();
    }

    public override string Title => "BACKUPS";

    public override async Task PopulateAsync()
    {
        BackupList.SetLoadingCircle(true);
        BackupList.SetLaunchPage(ParentPage);
        BackupList.SetBox(Box);

        BoxBackup[] backups = Box.Manifest.Backups.ToArray();
        await BackupList.SetBackupsAsync(backups);

        BackupList.SetLoadingCircle(false);
        DataContext = backups.Length;
    }

    private void NewBackupButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new NewBackupPopup(Box));
    }
}