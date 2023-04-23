using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;

namespace mcLaunch.Views;

public partial class PictureFrame : UserControl
{
    public PictureFrame()
    {
        InitializeComponent();
    }

    public PictureFrame(string filename)
    {
        InitializeComponent();
        
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
        Bitmap bmp = await Task.Run(() => new Bitmap(filename));

        Picture.Source = bmp;
        SetLoadingCircle(false);
    }

    public void SetLoadingCircle(bool show)
    {
        LoadCircle.IsVisible = show;
        Overlay.IsVisible = show;
    }
}