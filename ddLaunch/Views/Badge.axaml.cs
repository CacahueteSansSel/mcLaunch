using System;
using System.Drawing;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Platform;

namespace ddLaunch.Views;

public partial class Badge : UserControl
{
    public static readonly AttachedProperty<string> TextProperty =
        AvaloniaProperty.RegisterAttached<Badge, UserControl, string>(
            nameof(Text),
            "Editor",
            inherits: true);
    
    
    public static readonly StyledProperty<IImage> IconProperty =
        AvaloniaProperty.Register<Badge, IImage>(nameof(Icon));

    string text;
    IImage icon;

    public string Text
    {
        get => text;
        set
        {
            text = value;
            Label.Text = value;
        }
    }

    public IImage Icon
    {
        get => icon;
        set
        {
            icon = value;
            IconImage.Source = icon;
        }
    }

    public Badge()
    {
        InitializeComponent();
    }

    public Badge(string text, string iconName)
    {
        InitializeComponent();

        DataContext = new Data(text, iconName);
    }

    public class Data
    {
        public string Text { get; set; }
        public Bitmap Icon { get; set; }

        public Data(string text, string iconName)
        {
            Text = text;

            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            Icon = new Bitmap(assets.Open(new Uri($"avares:resources/icons/{iconName}.png")));
        }
    }
}