using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents;
using mcLaunch.Core.Contents.Packs;
using mcLaunch.Launchsite.Core;
using mcLaunch.Launchsite.Core.ModLoaders;
using mcLaunch.Launchsite.Models;
using mcLaunch.Models;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;

namespace mcLaunch.Views.Popups;

public partial class SelectBoxPopup : UserControl
{
    Box[] loadedBoxes;
    Action<Box> selectedBoxCallback;
    
    public SelectBoxPopup(Action<Box> selectedBoxCallback)
    {
        this.selectedBoxCallback = selectedBoxCallback;
        InitializeComponent();

        FillWithLocalBoxesAsync();
    }

    async void FillWithLocalBoxesAsync()
    {
        loadedBoxes = await BoxManager.LoadLocalBoxesAsync(false, false);
        DataContext = loadedBoxes;
    }

    private void ClosePopupButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    void ContentSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        SelectButton.IsEnabled = ContentList.SelectedItems != null && ContentList.SelectedItems?.Count > 0;
    }

    void SelectButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (ContentList.SelectedItem == null) return;
        
        selectedBoxCallback?.Invoke((Box)ContentList.SelectedItem!);
        Navigation.HidePopup();
    }

    void SearchTextBoxTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SearchTextBox.Text))
        {
            DataContext = loadedBoxes;
            return;
        }
        
        DataContext = loadedBoxes.Where(box => box.Manifest.Name.Contains(SearchTextBox.Text, StringComparison.InvariantCultureIgnoreCase)).ToArray();
    }
}