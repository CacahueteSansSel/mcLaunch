using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using mcLaunch.Core.Boxes;
using mcLaunch.Core.Core;
using mcLaunch.Launchsite.Models;
using mcLaunch.Managers;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Popups;

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

        if (Design.IsDesignMode)
        {
            DataContext = new BoxManifest("TestBox", "1.0.0", "TestModLoader", "TestModLoaderId", "0.0.0",
                IconCollection.FromResources("box_icons/0.png"), new ManifestMinecraftVersion() { }, BoxType.Default);
        }
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

    private void UpdateDeletedStatus()
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

        Regex snapshotVersionRegex = new("\\d.w\\d.a");

        SnapshotStripe.IsVisible = snapshotVersionRegex.IsMatch(box.Manifest.Version);
    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);

        if (!BackgroundManager.IsMinecraftRunning)
        {
            PlayButton.IsVisible = true;
            return;
        }

        if (BackgroundManager.IsBoxRunning(Box))
        {
            StopButton.IsVisible = true;
            PlayButton.IsVisible = false;
        }
        else
        {
            StopButton.IsVisible = false;
            PlayButton.IsVisible = true;
        }
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        PlayButton.IsVisible = false;
        StopButton.IsVisible = false;
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        if (e.GetCurrentPoint(null).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed)
            return;

        if (!Directory.Exists(box.Path)) return;

        Navigation.Push(new BoxDetailsPage(box));
    }

    private void PlayButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(box.Path)) return;

        BoxDetailsPage page = new(box);
        Navigation.Push(page);

        page.Run();
    }

    private void OpenMenuOptionClicked(object? sender, RoutedEventArgs e)
    {
        if (!Directory.Exists(box.Path)) return;

        Navigation.Push(new BoxDetailsPage(box));
    }

    private void CopyMenuOptionClicked(object? sender, RoutedEventArgs e)
    {
        MainWindow.Instance.Clipboard?.SetTextAsync(box.Manifest.Id);
    }

    private void DeleteMenuOptionClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new ConfirmMessageBoxPopup($"Delete {box.Manifest.Name} ?", "This action is irreversible",
            () =>
            {
                Box.Delete();
                MainPage.Instance.PopulateBoxListAsync();
            }));
    }

    private void OpenFolderMenuOptionClicked(object? sender, RoutedEventArgs e)
    {
        PlatformSpecific.OpenFolder(Box.Path);
    }

    private void DuplicateOptionClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new DuplicateBoxPopup(Box));
    }

    private async void CompleteReportOptionClicked(object? sender, RoutedEventArgs e)
    {
        if (MainWindow.Instance.Clipboard == null)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Error", "Unable to access clipboard", MessageStatus.Error));
            return;
        }

        string report = await BoxUtilities.GenerateReportAsync(Box);
        MainWindow.Instance.Clipboard?.SetTextAsync(report);

        Navigation.ShowPopup(new MessageBoxPopup("Success", "Report copied to clipboard", MessageStatus.Success));
    }

    private async void RelativeReportOptionClicked(object? sender, RoutedEventArgs e)
    {
        if (MainWindow.Instance.Clipboard == null)
        {
            Navigation.ShowPopup(new MessageBoxPopup("Error", "Unable to access clipboard", MessageStatus.Error));
            return;
        }

        string report = await BoxUtilities.GenerateReportAsync(Box, false);
        MainWindow.Instance.Clipboard?.SetTextAsync(report);

        Navigation.ShowPopup(new MessageBoxPopup("Success", "Report copied to clipboard", MessageStatus.Success));
    }

    private void StopButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (BackgroundManager.IsMinecraftRunning)
            BackgroundManager.KillMinecraftProcess();
    }

    private async void InputElement_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        Animation textAnimation = new Animation
        {
            Duration = TimeSpan.FromMilliseconds(200),
            Easing = new CubicEaseInOut(),
            FillMode = FillMode.Forward,
            PlaybackDirection = PlaybackDirection.Alternate,
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(Canvas.LeftProperty, 0)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(Canvas.LeftProperty, -20)
                    }
                }
            }
        };
        
        await textAnimation.RunAsync(BoxNameText);
    }

    private void InputElement_OnPointerExited(object? sender, PointerEventArgs e)
    {
    }

    private void ExportOptionClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.ShowPopup(new ExportBoxPopup(Box));
    }
}