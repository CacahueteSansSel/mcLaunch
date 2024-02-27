using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.MinecraftFormats;
using mcLaunch.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;
using ReactiveUI;

namespace mcLaunch.Views;

public partial class BackupList : UserControl
{
    private Box lastBox;
    private string lastQuery;
    private BoxDetailsPage launchPage;

    public BackupList()
    {
        InitializeComponent();

        DataContext = new Data();
        if (Design.IsDesignMode) SetDefaultBackups();
    }

    private async void SetDefaultBackups()
    {
        await SetBackupsAsync([
            new BoxBackup("Test Backup", BoxBackupType.Complete, DateTime.Now, "sample.tar.gz")
        ]);
    }

    public void SetLaunchPage(BoxDetailsPage page)
    {
        launchPage = page;
    }

    public void SetBox(Box box)
    {
        lastBox = box;
    }

    public async Task SetBackupsAsync(BoxBackup[] backups)
    {
        Data ctx = (Data) DataContext;
        ctx.Backups = backups;

        NtsBanner.IsVisible = backups.Length == 0;
    }

    private async Task LoadServerIconsAsync(MinecraftServer[] servers)
    {
        Data ctx = (Data) DataContext;

        SetLoadingCircle(true);

        foreach (MinecraftServer server in servers)
            await server.LoadIconAsync();

        SetLoadingCircle(false);
    }

    public void SetLoadingCircle(bool isLoading)
    {
        LoadCircle.IsVisible = isLoading;
    }

    private void BackupSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0)
        {
            BoxBackup backup = (BoxBackup) e.AddedItems[0];

            Navigation.ShowPopup(new ConfirmMessageBoxPopup($"Restore backup {backup.Name} ?",
                $"Do you want to restore the backup {backup.Name} created at {backup.CreationTime} ? This will replace all changes you made since, and you can't undo this operation !",
                async () =>
                {
                    Navigation.ShowPopup(new StatusPopup($"Restoring {backup.Name}",
                        $"Restoring {backup.Name} onto {lastBox.Manifest.Name}"));

                    bool success = await lastBox.RestoreBackupAsync(backup.Name);
                    if (!success)
                    {
                        Navigation.ShowPopup(new MessageBoxPopup("Failed to restore backup",
                            $"Failed to restore backup {backup.Name} onto {lastBox.Manifest.Name}"));

                        return;
                    }

                    MainWindowDataContext.Instance.Reset();
                    MainPage.Instance.PopulateBoxList();
                    Navigation.Push(new BoxDetailsPage(lastBox));

                    Navigation.ShowPopup(new MessageBoxPopup("Backup restored",
                        $"Backup {backup.Name} was restored on {lastBox.Manifest.Name}"));
                }));
        }

        BackupsList.UnselectAll();
    }

    public class Data : ReactiveObject
    {
        private BoxBackup[] backups;
        private int page;

        public BoxBackup[] Backups
        {
            get => backups;
            set => this.RaiseAndSetIfChanged(ref backups, value);
        }

        public int Page
        {
            get => page;
            set => this.RaiseAndSetIfChanged(ref page, value);
        }
    }
}