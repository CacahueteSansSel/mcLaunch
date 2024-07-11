using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Contents;
using mcLaunch.Managers;

namespace mcLaunch.Views.Pages;

public partial class ContentSearchPage : UserControl, ITopLevelPageControl
{
    public ContentSearchPage()
    {
        InitializeComponent();
    }

    public ContentSearchPage(Box box, MinecraftContentType contentType)
    {
        InitializeComponent();

        Box = box;
        ContentType = contentType;
        DataContext = Box;

        DefaultBackground.IsVisible = box.Manifest.Background == null;

        ModList.ContentType = ContentType;
        ModList.Search(box, "");

        TitleText.Text = $"Browse more {ContentNamePlural.ToLower()} on";
    }

    public Box Box { get; }
    public MinecraftContentType ContentType { get; }
    public string ContentName => ContentType.ToString();
    public string ContentNamePlural => $"{ContentName}s";

    public string Title => $"Browse {ContentNamePlural.ToLower()} for {Box.Manifest.Name} on " +
                           $"{Box.Manifest.ModLoaderId} {Box.Manifest.Version}";

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        DiscordManager.SetPresenceEditingBox(Box);
    }

    private void SearchButtonClicked(object? sender, RoutedEventArgs e)
    {
        ModList.SetContents(Array.Empty<MinecraftContent>());

        ModList.Search(Box, SearchBoxInput.Text);
    }

    void UpButtonClicked(object? sender, RoutedEventArgs e)
    {
        ScrollArea.Offset = Vector.Zero;
    }
}