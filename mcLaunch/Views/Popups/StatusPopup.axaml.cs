using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Managers;
using ReactiveUI;

namespace mcLaunch.Views.Popups;

public partial class StatusPopup : UserControl
{
    public static StatusPopup Instance { get; private set; }
    Data dctx;

    public string Status
    {
        get => dctx.StatusText;
        set => dctx.StatusText = value;
    }

    public float StatusPercent
    {
        get => dctx.StatusPercent / 100f;
        set => dctx.StatusPercent = (int)(value * 100);
    }

    public StatusPopup()
    {
        InitializeComponent();
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

    public class Data : ReactiveObject
    {
        string title;
        string text;
        string statusText;
        int statusPercent;

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