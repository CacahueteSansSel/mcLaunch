using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Boxes;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class SelectBoxPopup : UserControl
{
    private readonly Action<Box> selectedBoxCallback;
    private Box[] loadedBoxes;

    public SelectBoxPopup(Action<Box> selectedBoxCallback)
    {
        this.selectedBoxCallback = selectedBoxCallback;
        InitializeComponent();

        FillWithLocalBoxesAsync();
    }

    private async void FillWithLocalBoxesAsync()
    {
        loadedBoxes = await BoxManager.LoadLocalBoxesAsync(false, false);
        DataContext = loadedBoxes;
    }

    private void ClosePopupButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private void ContentSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SelectButton.IsEnabled = ContentList.SelectedItems != null && ContentList.SelectedItems?.Count > 0;
    }

    private void SelectButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (ContentList.SelectedItem == null) return;

        selectedBoxCallback?.Invoke((Box)ContentList.SelectedItem!);
        Navigation.HidePopup();
    }

    private void SearchTextBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
        {
            DataContext = loadedBoxes;
            return;
        }

        DataContext = loadedBoxes.Where(box =>
            box.Manifest.Name.Contains(SearchTextBox.Text, StringComparison.InvariantCultureIgnoreCase)).ToArray();
    }
}