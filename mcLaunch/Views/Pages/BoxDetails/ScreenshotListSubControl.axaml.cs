﻿using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;

namespace mcLaunch.Views.Pages.BoxDetails;

public partial class ScreenshotListSubControl : UserControl, ISubControl
{
    public ScreenshotListSubControl()
    {
        InitializeComponent();
    }

    public BoxDetailsPage ParentPage { get; set; }
    public Box Box { get; set; }
    public string Title { get; } = "SCREENSHOTS";
    
    public async Task PopulateAsync()
    {
        Container.Children.Clear();
        
        foreach (string path in Box.GetScreenshotPaths())
            Container.Children.Add(new PictureFrame(path, Box));
    }
}