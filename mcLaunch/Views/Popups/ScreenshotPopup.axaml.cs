using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using mcLaunch.Core.Boxes;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class ScreenshotPopup : UserControl
{
    private readonly Box box;
    private readonly string filename;

    public ScreenshotPopup()
    {
        InitializeComponent();
    }

    public ScreenshotPopup(string filename, Box box)
    {
        InitializeComponent();

        this.filename = filename;
        this.box = box;

        Picture.Source = new Bitmap(filename);
    }

    private void CloseButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private void EnterScreenshot(object? sender, PointerEventArgs e)
    {
        Overlay.IsVisible = true;
    }

    private void LeaveScreenshot(object? sender, PointerEventArgs e)
    {
        Overlay.IsVisible = false;
    }

    private void OpenButtonClicked(object? sender, RoutedEventArgs e)
    {
        PlatformSpecific.OpenFile(filename);
    }

    private void OpenFolderButtonClicked(object? sender, RoutedEventArgs e)
    {
        PlatformSpecific.OpenFolder($"{box.Folder.CompletePath}/screenshots");
    }

    private void SetAsBoxBackgroundButtonClicked(object? sender, RoutedEventArgs e)
    {
        box.SetAndSaveBackground((Bitmap) Picture.Source);
        Navigation.HidePopup();
    }
}