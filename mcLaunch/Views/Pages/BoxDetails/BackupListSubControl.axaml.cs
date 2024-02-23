using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.MinecraftFormats;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class BackupListSubControl : SubControl
{
    public override string Title => "BACKUPS";
    
    public BackupListSubControl()
    {
        InitializeComponent();
    }
    
    public override async Task PopulateAsync()
    {
        BackupList.SetLoadingCircle(true);
        BackupList.SetLaunchPage(ParentPage);

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