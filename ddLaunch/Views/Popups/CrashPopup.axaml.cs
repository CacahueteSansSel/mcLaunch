using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ddLaunch.Core.Boxes;
using ddLaunch.Utilities;
using ddLaunch.Views.Pages;

namespace ddLaunch.Views.Popups;

public partial class CrashPopup : UserControl
{
    private Box? box;
    public CrashPopup(int exitCode, string boxId)
    {
        InitializeComponent();

        box = BoxManager.LoadLocalBoxes()
            .FirstOrDefault(b => b.Manifest.Id == boxId);

        if (box == null) return;
        
        BoxCard.SetBox(box);

        BodyText.Text =
            $"Minecraft has exited with code {exitCode}. This indicates that Minecraft has encountered an error " +
            $"and shut down. Verify that every mod is up to date, not duplicate, and compatible with each other";
    }

    public CrashPopup()
    {
        InitializeComponent();
    }

    private void ClosePopupButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    private void OpenBoxDetailsButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.Push(new BoxDetailsPage(box!));
        
        Navigation.HidePopup();
    }

    private void RestartBoxButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
        
        BoxDetailsPage page = new BoxDetailsPage(box!);
        Navigation.Push(page);
        
        page.Run();
    }
}