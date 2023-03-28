using System;
using System.Drawing;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;

namespace ddLaunch.Views;

public partial class Badge : UserControl
{
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