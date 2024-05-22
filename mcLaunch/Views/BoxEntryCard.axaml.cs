using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using mcLaunch.Core.Boxes;
using mcLaunch.Managers;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;

namespace mcLaunch.Views;

public partial class BoxEntryCard : UserControl
{
    public static readonly AvaloniaProperty<Box> BoxProperty =
        AvaloniaProperty.RegisterDirect<BoxEntryCard, Box>(nameof(Box), card => card.box,
            (card, box) => card.SetBox(box));

    private Box box;

    public BoxEntryCard()
    {
        InitializeComponent();
    }

    public Box Box
    {
        get => box;
        set
        {
            box = value;
            SetBox(box);
        }
    }

    void UpdateDeletedStatus()
    {
        if (box == null) return;
        
        bool isBeingDeleted = !Directory.Exists(box.Path);
        DeletingText.IsVisible = isBeingDeleted;
        Badges.IsVisible = !isBeingDeleted;
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        
        UpdateDeletedStatus();

        if (Settings.Instance!.AnonymizeBoxIdentity)
        {
            (string, Bitmap)? tuple = null;

            await Task.Run(() => { tuple = AnonymitySession.Default.TakeNameAndIcon(); });

            BoxNameText.Text = tuple!.Value.Item1;
            BoxIcon.Source = tuple!.Value.Item2;
            AuthorText.Text = "Someone";
        }
    }

    public void SetBox(Box? box)
    {
        if (box == null) return;

        this.box = box;

        IsEnabled = true;
        DataContext = box.Manifest;
        
        UpdateDeletedStatus();

        if (Settings.Instance != null && Settings.Instance.AnonymizeBoxIdentity)
            BoxNameText.Text = AnonymitySession.Default.TakeName();

        VersionBadge.Text = box.Manifest.Version;
        ModLoaderBadge.Text = box.ModLoader?.Name ?? "Unknown";
        if (box.ModLoader != null)
            ModLoaderBadge.Icon =
                new Bitmap(AssetLoader.Open(new Uri($"avares://mcLaunch/resources/icons/{box.ModLoader.Id}.png")));

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
        if (!Directory.Exists(box.Path)) return;
        
        Navigation.Push(new BoxDetailsPage(box));
    }

    private void PlayButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(box.Path)) return;
        
        BoxDetailsPage page = new BoxDetailsPage(box);
        Navigation.Push(page);

        page.Run();
    }
}