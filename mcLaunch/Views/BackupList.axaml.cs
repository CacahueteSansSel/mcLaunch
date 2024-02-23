using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core;
using mcLaunch.Core.Managers;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Contents.Platforms;
using mcLaunch.Models;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.MinecraftFormats;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;
using ReactiveUI;

namespace mcLaunch.Views;

public partial class BackupList : UserControl
{
    private Box lastBox;
    private BoxDetailsPage launchPage;
    private string lastQuery;

    public BackupList()
    {
        InitializeComponent();

        DataContext = new Data();
        if (Design.IsDesignMode) SetDefaultBackups();
    }

    async void SetDefaultBackups()
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

    async Task LoadServerIconsAsync(MinecraftServer[] servers)
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

    public class Data : ReactiveObject
    {
        BoxBackup[] backups;
        int page;

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

    private void BackupSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && launchPage != null)
        {
            BoxBackup backup = (BoxBackup) e.AddedItems[0];

            Navigation.ShowPopup(new ConfirmMessageBoxPopup($"Connect to {backup.Name} ?",
                $"Minecraft will start and automatically connect to {backup.Name} at {backup.Filename}",
                () => { }));
        }

        BackupsList.UnselectAll();
    }
}