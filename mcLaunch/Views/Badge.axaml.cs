using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace mcLaunch.Views;

public partial class Badge : UserControl
{
    public static readonly AttachedProperty<string> TextProperty =
        AvaloniaProperty.RegisterAttached<Badge, UserControl, string>(
            nameof(Text),
            "Editor",
            true);


    public static readonly StyledProperty<IImage> IconProperty =
        AvaloniaProperty.Register<Badge, IImage>(nameof(Icon));

    private IImage icon;

    private string text;

    public Badge()
    {
        InitializeComponent();
    }

    public Badge(string text, string iconName)
    {
        InitializeComponent();

        DataContext = new Data(text, iconName);
    }

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

    public class Data
    {
        public Data(string text, string iconName)
        {
            Text = text;

            Icon = new Bitmap(AssetLoader.Open(new Uri($"avares:resources/icons/{iconName}.png")));
        }

        public string Text { get; set; }
        public Bitmap Icon { get; set; }
    }
}