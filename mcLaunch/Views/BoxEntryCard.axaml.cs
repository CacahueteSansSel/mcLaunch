using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Models;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Utilities;
using mcLaunch.Managers;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;

namespace mcLaunch.Views;

public partial class BoxEntryCard : UserControl
{
    public Box Box { get; private set; }
    private AnonymitySession anonSession;

    public BoxEntryCard()
    {
        InitializeComponent();
    }
    
    public BoxEntryCard(Box box, AnonymitySession anonSession)
    {
        InitializeComponent();
        this.anonSession = anonSession;

        SetBox(box);
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        if (Settings.Instance.AnonymizeBoxIdentity)
        {
            object v = null;
            
            await Task.Run(() =>
            {
                v = anonSession.TakeNameAndIcon();
            });

            var tuple = ((string, Bitmap)) v;
            
            BoxNameText.Text = tuple.Item1;
            BoxIcon.Source = tuple.Item2;
            AuthorText.Text = "Someone";
        }
    }

    public void SetBox(Box box)
    {
        Box = box;
        DataContext = box.Manifest;

        if (Settings.Instance.AnonymizeBoxIdentity)
        {
            BoxNameText.Text = anonSession.TakeName();
        }

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

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        PlayButton.IsVisible = true;
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        PlayButton.IsVisible = false;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        
        Navigation.Push(new BoxDetailsPage(Box));
    }

    private void PlayButtonClicked(object? sender, RoutedEventArgs e)
    {
        BoxDetailsPage page = new BoxDetailsPage(Box);
        Navigation.Push(page);
        
        page.Run();
    }
}