using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using mcLaunch.Core.Boxes;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;

namespace mcLaunch.Views.Popups;

public partial class DuplicateBoxPopup : UserControl
{
    public DuplicateBoxPopup()
    {
        InitializeComponent();
    }

    public DuplicateBoxPopup(Box sourceBox)
    {
        InitializeComponent();

        SourceBox = sourceBox;

        BoxNameTb.Text = sourceBox.Manifest.Name;
        BoxAuthorTb.Text = sourceBox.Manifest.Author;
        BoxIconImage.Source = sourceBox.Manifest.Icon?.IconLarge;
    }

    public Box SourceBox { get; }

    private async void SelectFileButtonClicked(object? sender, RoutedEventArgs e)
    {
        Bitmap[]? files = await FilePickerUtilities.PickBitmaps(false, "Select a new icon image");
        if (files.Length == 0) return;

        Bitmap? bmp = files.FirstOrDefault();
        if (bmp != null) BoxIconImage.Source = bmp;
    }

    private void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private async void ApplyButtonClicked(object? sender, RoutedEventArgs e)
    {
        Box box = await BoxUtilities.DuplicateAsync(SourceBox, BoxNameTb.Text, BoxAuthorTb.Text);

        if (BoxIconImage.Source != null)
            box.SetAndSaveIcon((Bitmap)BoxIconImage.Source);

        if (!CancelButton.IsVisible && box.Manifest.Type == BoxType.Temporary)
            box.Manifest.Type = BoxType.Default;

        await box.SaveManifestAsync();

        await MainPage.Instance?.PopulateBoxListAsync();

        Navigation.Pop();
        Navigation.Push(new BoxDetailsPage(box));

        Navigation.HidePopup();
    }
}