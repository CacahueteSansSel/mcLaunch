﻿using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using mcLaunch.Core.Boxes;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;

namespace mcLaunch.Views.Popups;

public partial class EditBoxPopup : UserControl
{
    Box box;
    
    public EditBoxPopup()
    {
        InitializeComponent();
    }

    public EditBoxPopup(Box box)
    {
        InitializeComponent();

        this.box = box;
        BoxNameTb.Text = box.Manifest.Name;
        AuthorNameTb.Text = box.Manifest.Author;
        BoxIconImage.Source = box.Manifest.Icon;
    }

    private async void SelectFileButtonClicked(object? sender, RoutedEventArgs e)
    {
        OpenFileDialog ofd = new OpenFileDialog();
        ofd.Title = "Select the icon image...";
        ofd.Filters = new List<FileDialogFilter>()
        {
            new()
            {
                Extensions = new List<string>()
                {
                    "png"
                },
                Name = "PNG Image"
            }
        };

        string[]? files = await ofd.ShowAsync(MainWindow.Instance);
        if (files == null) return;

        BoxIconImage.Source = new Bitmap(files[0]);
    }

    private void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private void ApplyButtonClicked(object? sender, RoutedEventArgs e)
    {
        box.Manifest.Name = BoxNameTb.Text;
        box.Manifest.Author = AuthorNameTb.Text;
        box.SetAndSaveIcon((Bitmap)BoxIconImage.Source);
        
        box.SaveManifest();
        
        MainPage.Instance.PopulateBoxList();
        
        Navigation.Pop();
        Navigation.Push(new BoxDetailsPage(box));
        
        Navigation.HidePopup();
    }
}