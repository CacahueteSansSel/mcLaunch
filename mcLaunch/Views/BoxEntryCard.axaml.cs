using System;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Models;
using mcLaunch.Core.Boxes;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;

namespace mcLaunch.Views;

public partial class BoxEntryCard : UserControl
{
    public Box Box { get; private set; }

    public BoxEntryCard()
    {
        InitializeComponent();
    }
    
    public BoxEntryCard(Box box)
    {
        InitializeComponent();

        SetBox(box);
    }

    public void SetBox(Box box)
    {
        Box = box;
        DataContext = box.Manifest;

        VersionBadge.Text = box.Manifest.Version;
        ModLoaderBadge.Text = box.ModLoader?.Name ?? "Unknown";
        if (box.ModLoader != null)
        {
            ModLoaderBadge.Icon =
                new Bitmap(AssetLoader.Open(new Uri($"avares://mcLaunch/resources/icons/{box.ModLoader.Id}.png")));
        }

        Regex snapshotVersionRegex = new Regex("\\d.w\\d.a");

        SnapshotStripe.IsVisible = snapshotVersionRegex.IsMatch(box.Manifest.Version);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        Navigation.Push(new BoxDetailsPage(Box));
    }
}