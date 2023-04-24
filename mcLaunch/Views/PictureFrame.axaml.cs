using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Visuals.Media.Imaging;
using mcLaunch.Core.Boxes;
using mcLaunch.Utilities;
using mcLaunch.Views.Popups;

namespace mcLaunch.Views;

public partial class PictureFrame : UserControl
{
    string? filename;
    Box? box;
    
    public PictureFrame()
    {
        InitializeComponent();
    }

    public PictureFrame(string filename, Box? box = null)
    {
        InitializeComponent();

        this.box = box;
        
        SetLoadingCircle(true);
        LoadAsync(filename);
    }

    public PictureFrame(Bitmap bitmap)
    {
        InitializeComponent();

        Picture.Source = bitmap;
        SetLoadingCircle(false);
    }

    async void LoadAsync(string filename)
    {
        this.filename = filename;
        
        Bitmap bmp = await Task.Run(() =>
            new Bitmap(filename).CreateScaledBitmap(new PixelSize(1280, 720), BitmapInterpolationMode.MediumQuality));

        Picture.Source = bmp;
        SetLoadingCircle(false);
    }

    public void SetLoadingCircle(bool show)
    {
        LoadCircle.IsVisible = show;
        Overlay.IsVisible = show;
    }

    private void PictureClicked(object? sender, RoutedEventArgs e)
    {
        if (filename == null) return;
        
        Navigation.ShowPopup(new ScreenshotPopup(filename, box));
    }
}