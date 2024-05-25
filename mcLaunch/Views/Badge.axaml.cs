using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using ReactiveUI;

namespace mcLaunch.Views;

public partial class Badge : UserControl
{
    private Data data;
    
    public static readonly AttachedProperty<string> TextProperty =
        AvaloniaProperty.RegisterAttached<Badge, UserControl, string>(
            nameof(Text),
            "Editor",
            true);

    public Badge()
    {
        InitializeComponent();
    }

    public Badge(string text)
    {
        InitializeComponent();

        data = new Data(text);
        DataContext = data;
    }

    public string Text
    {
        get => data.Text;
        set
        {
            if (data == null) data = new Data(value);
            
            data.Text = value;
            DataContext = data;
        }
    }

    public class Data : ReactiveObject
    {
        private string text;
        
        public Data(string text)
        {
            this.text = text;
        }

        public string Text
        {
            get => text;
            set => this.RaiseAndSetIfChanged(ref text, value);
        }
    }
}