using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using mcLaunch.Core.Managers;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;
using ReactiveUI;

namespace mcLaunch.Views;

public partial class DownloadBanner : UserControl
{
    private Data UIDataContext => (Data) DataContext;
    public bool IsForcedToBeShown { get; set; }
    
    public DownloadBanner()
    {
        InitializeComponent();

        DataContext = new Data();
    }

    public void ForceToShow()
    {
        IsForcedToBeShown = true;
        IsVisible = true;
    }

    public void ResetForceToShown()
    {
        IsForcedToBeShown = false;
        IsVisible = false;
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        FileNameText.IsVisible = true;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        
        FileNameText.IsVisible = false;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        DownloadManager.OnDownloadPrepareStarting += OnDownloadPrepareStarting;
        DownloadManager.OnDownloadPrepareEnding += OnDownloadFinished;
        DownloadManager.OnDownloadProgressUpdate += OnDownloadProgressUpdate;
        DownloadManager.OnDownloadFinished += OnDownloadFinished;
        DownloadManager.OnDownloadSectionStarting += OnDownloadSectionStarting;
        DownloadManager.OnDownloadError += OnDownloadError;

        if (!Design.IsDesignMode && !IsForcedToBeShown) IsVisible = false;
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        
        DownloadManager.OnDownloadPrepareStarting -= OnDownloadPrepareStarting;
        DownloadManager.OnDownloadPrepareEnding -= OnDownloadFinished;
        DownloadManager.OnDownloadProgressUpdate -= OnDownloadProgressUpdate;
        DownloadManager.OnDownloadFinished -= OnDownloadFinished;
        DownloadManager.OnDownloadSectionStarting -= OnDownloadSectionStarting;
        DownloadManager.OnDownloadError -= OnDownloadError;
    }

    private void OnDownloadError(string sectionName, string file)
    {
        Navigation.ShowPopup(new MessageBoxPopup($"Download failed for {sectionName}",
            $"{sectionName} failed to download (file: {file}). Try restarting the download."));
    }

    private void OnDownloadSectionStarting(string sectionName, int index)
    {
        UIDataContext.Progress = 0;
        UIDataContext.ResourceName = sectionName;
        UIDataContext.ResourceCount = $"{index}/{DownloadManager.PendingSectionCount}";

        IsVisible = true;
    }

    private void OnDownloadPrepareStarting(string name)
    {
        UIDataContext.Progress = 0;
        UIDataContext.ResourceName = name;
        UIDataContext.ResourceDetailsText = "Preparing";
        
        IsVisible = true;
    }

    private void OnDownloadFinished()
    {
        UIDataContext.Progress = 0;
        UIDataContext.ResourceName = string.Empty;
        UIDataContext.ResourceCount = string.Empty;

        if (!IsForcedToBeShown) IsVisible = false;
    }

    private void OnDownloadProgressUpdate(string file, float percent, int currentSectionIndex)
    {
        Dispatcher.UIThread.Post(() =>
        {
            UIDataContext.Progress = (int) MathF.Round(percent * 100);
            UIDataContext.ResourceName = DownloadManager.DescriptionLine;
            UIDataContext.ResourceDetailsText = $"{(int)MathF.Round(percent * 100)}%";
            UIDataContext.ResourceCount = $"{currentSectionIndex}/{DownloadManager.PendingSectionCount}";
            FileNameText.Text = Path.GetFileName(file);
        });
    }

    private class Data : ReactiveObject
    {
        int progress;
        string resourceName = "No pending download";
        string resourceDetailsText = "-";
        string resourceFileText = "-";
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

        public string ResourceDetailsText
        {
            get => resourceDetailsText;
            set => this.RaiseAndSetIfChanged(ref resourceDetailsText, value);
        }

        public string ResourceFileText
        {
            get => resourceFileText;
            set => this.RaiseAndSetIfChanged(ref resourceFileText, value);
        }

        public string ResourceCount
        {
            get => resourceCount;
            set => this.RaiseAndSetIfChanged(ref resourceCount, value);
        }
    }
}