using Avalonia.Controls;
using Avalonia.Threading;
using ReactiveUI;

namespace mcLaunch.Views.Popups;

public partial class StatusPopup : UserControl
{
    private readonly Data dctx;

    public StatusPopup()
    {
        InitializeComponent();

        if (Design.IsDesignMode)
        {
            dctx = new Data
            {
                Title = "Sample Title",
                Text = "Sample Text",
                StatusText = "Please wait...",
                StatusPercent = 50
            };

            DataContext = dctx;
        }
    }

    public StatusPopup(string title, string text)
    {
        Instance = this;
        InitializeComponent();

        dctx = new Data
        {
            Title = title,
            Text = text,
            StatusText = "Please wait...",
            StatusPercent = 0
        };

        DataContext = dctx;
    }

    public static StatusPopup Instance { get; private set; }

    public string Status
    {
        get => dctx.StatusText;
        set => dctx.StatusText = value;
    }

    public float StatusPercent
    {
        get => dctx.StatusPercent / 100f;
        set
        {
            dctx.StatusPercent = (int)(value * 100);
            Dispatcher.UIThread.Post(() =>
            {
                Bar.IsIndeterminate = false;
            });
        }
    }
    
    public bool StatusIndeterminate
    {
        get => Bar.IsIndeterminate;
        set
        {
            Dispatcher.UIThread.Post(() =>
            {
                Bar.IsIndeterminate = value;
            });
        }
    }

    public bool ShowDownloadBanner
    {
        get => DownloadBannerContainer.IsVisible;
        set
        {
            DownloadBannerContainer.IsVisible = value;

            if (value) DownloadBanner.ForceToShow();
            else DownloadBanner.ResetForceToShown();
        }
    }

    public class Data : ReactiveObject
    {
        private int statusPercent;
        private string statusText;
        private string text;
        private string title;

        public string Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        public string Text
        {
            get => text;
            set => this.RaiseAndSetIfChanged(ref text, value);
        }

        public string StatusText
        {
            get => statusText;
            set => this.RaiseAndSetIfChanged(ref statusText, value);
        }

        public int StatusPercent
        {
            get => statusPercent;
            set => this.RaiseAndSetIfChanged(ref statusPercent, value);
        }
    }
}