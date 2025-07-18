using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using mcLaunch.Core.Core;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class SelectSkinPopup : UserControl
{
    Action<string, string, SkinType> _skinSelectedCallback;
    private string _skinFilename;
    private SkinType _type = SkinType.Classic;

    public SelectSkinPopup()
    {
        InitializeComponent();
    }
    
    public SelectSkinPopup(Action<string, string, SkinType> skinSelectedCallback)
    {
        InitializeComponent();
        
        _skinSelectedCallback = skinSelectedCallback;
    }

    private void OKButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SkinNameInputText.Text) || string.IsNullOrWhiteSpace(_skinFilename))
            return;
        
        Navigation.HidePopup();
        _skinSelectedCallback?.Invoke(_skinFilename, SkinNameInputText.Text, _type);
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

        if (list.Count == 0)
        {
            ErrorText.Text = "";
            AddButton.IsEnabled = false;
            
            return;
        }
        
        IStorageFile file = list[0];
        _skinFilename = file.Path.AbsolutePath;
        
        Bitmap skin = new(file.Path.AbsolutePath);
        
        if (skin.PixelSize.Width != 64 || skin.PixelSize.Height != 64)
        {
            ErrorText.Text = "Skin must be 64x64 pixels";
            AddButton.IsEnabled = false;
            return;
        }
        
        ErrorText.Text = "";
        AddButton.IsEnabled = !string.IsNullOrWhiteSpace(SkinNameInputText.Text);
        
        SkinPreview.Texture = skin.CreateScaledBitmap(new PixelSize(512, 512), BitmapInterpolationMode.None);
        skin.Dispose();
        
        SkinPreview.InvalidateVisual();
    }

    private void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private void SkinTypeChanged(object? sender, RoutedEventArgs e)
    {
        _type = SlimSkinTypeRadioButton.IsChecked!.Value ? SkinType.Slim : SkinType.Classic;

        SkinPreview.Type = _type;
        SkinPreview.InvalidateVisual();
    }

    private void SkinNameTextChanged(object? sender, TextChangedEventArgs e)
    {
        AddButton.IsEnabled = !string.IsNullOrWhiteSpace(SkinNameInputText.Text);
    }
}