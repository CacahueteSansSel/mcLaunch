using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using mcLaunch.Core.Boxes;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;

namespace mcLaunch.Views.Popups;

public partial class EditBoxPopup : UserControl
{
    private readonly Box box;

    public EditBoxPopup()
    {
        InitializeComponent();
    }

    public EditBoxPopup(Box box, bool cancelEnabled = true)
    {
        InitializeComponent();

        this.box = box;
        BoxNameTb.Text = box.Manifest.Name;
        AuthorNameTb.Text = box.Manifest.Author;
        BoxIconImage.Source = box.Manifest.Icon?.IconLarge;

        CancelButton.IsVisible = cancelEnabled;
    }

    private async void SelectFileButtonClicked(object? sender, RoutedEventArgs e)
    {
        Bitmap[]? files = await FileSystemUtilities.PickBitmaps(false, "Select a new icon image");
        if (files.Length == 0) return;

        Bitmap? bmp = files.FirstOrDefault();
        if (bmp != null) BoxIconImage.Source = bmp;
    }

    private void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private void ApplyButtonClicked(object? sender, RoutedEventArgs e)
    {
        box.Manifest.Name = BoxNameTb.Text;
        box.Manifest.Author = AuthorNameTb.Text;
        if (BoxIconImage.Source != null)
            box.SetAndSaveIcon((Bitmap) BoxIconImage.Source);

        if (!CancelButton.IsVisible && box.Manifest.Type == BoxType.Temporary)
            box.Manifest.Type = BoxType.Default;

        box.SaveManifest();

        MainPage.Instance.PopulateBoxList();

        Navigation.Pop();
        Navigation.Push(new BoxDetailsPage(box));

        Navigation.HidePopup();
    }
}