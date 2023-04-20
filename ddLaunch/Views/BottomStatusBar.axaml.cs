using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ddLaunch.Core.Managers;
using ddLaunch.Utilities;
using ddLaunch.Views.Popups;
using ReactiveUI;

namespace ddLaunch.Views;

public partial class BottomStatusBar : UserControl
{
    public static BottomStatusBar Instance { get; private set; }

    public Data UIDataContext => (Data)DataContext;
    
    public BottomStatusBar()
    {
        InitializeComponent();
        Instance = this;

        DataContext = new Data();
        
        DownloadManager.OnDownloadPrepareStarting += OnDownloadPrepareStarting;
        DownloadManager.OnDownloadPrepareEnding += OnDownloadFinished;
        DownloadManager.OnDownloadProgressUpdate += OnDownloadProgressUpdate;
        DownloadManager.OnDownloadFinished += OnDownloadFinished;
        DownloadManager.OnDownloadSectionStarting += OnDownloadSectionStarting;
        DownloadManager.OnDownloadError += OnDownloadError;
    }

    private void OnDownloadError(string sectionName, string file)
    {
        Navigation.ShowPopup(new MessageBoxPopup($"Download failed for {sectionName}", $"{sectionName} failed to download (file: {file}). Try restarting the download."));
    }

    private void OnDownloadSectionStarting(string sectionName, int index)
    {
        UIDataContext.Progress = 0;
        UIDataContext.ResourceName = sectionName;
        UIDataContext.ResourceCount = $"{index}/{DownloadManager.PendingSectionCount}";
    }

    private void OnDownloadPrepareStarting(string name)
    {
        UIDataContext.Progress = 0;
        UIDataContext.ResourceName = $"Preparing downloading {name}";
    }

    private void OnDownloadFinished()
    {
        UIDataContext.Progress = 0;
        UIDataContext.ResourceName = "No pending download";
        UIDataContext.ResourceCount = string.Empty;
    }

    private void OnDownloadProgressUpdate(string file, float percent, int currentSectionIndex)
    {
        UIDataContext.Progress = (int)MathF.Round(percent * 100);
        UIDataContext.ResourceName = DownloadManager.DescriptionLine;
        UIDataContext.ResourceCount = $"{currentSectionIndex}/{DownloadManager.PendingSectionCount}";
    }

    public class Data : ReactiveObject
    {
        int progress;
        string resourceName = "No pending download";
        string resourceCount;
        
        public int Progress
        {
            get => progress;
            set => this.RaiseAndSetIfChanged(ref progress, value);
        }
        
        public string ResourceName
        {
            get => resourceName;
            set => this.RaiseAndSetIfChanged(ref resourceName, value);
        }
        
        public string ResourceCount
        {
            get => resourceCount;
            set => this.RaiseAndSetIfChanged(ref resourceCount, value);
        }
    }
}