using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class SelectSkinPopup : UserControl
{
    public SelectSkinPopup()
    {
        InitializeComponent();
    }

    private void OKButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private async void SelectFileButtonClicked(object? sender, RoutedEventArgs e)
    {
        IStorageProvider storage = MainWindow.Instance.StorageProvider;

        IReadOnlyList<IStorageFile> list = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select a skin file",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("PNG files") { Patterns = ["*.png"] }
            ]
        });
        
        if (list.Count == 0) return;
        
        IStorageFile file = list[0];
        SkinPreview.Texture = new Bitmap(file.Path.AbsolutePath);
        SkinPreview.InvalidateVisual();
    }
}