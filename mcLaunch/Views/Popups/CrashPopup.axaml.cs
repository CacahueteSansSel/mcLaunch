using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Core.Boxes;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;

namespace mcLaunch.Views.Popups;

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

        string bodyText;
        string latestLogsPath = $"{box.Folder.CompletePath}/logs/latest.log";

        if (File.Exists(latestLogsPath))
        {
            bodyText = File.ReadAllText(latestLogsPath) + $"\n\nMinecraft exited with code {exitCode}";
        }
        else
        {
            bodyText = $"Minecraft has exited with code {exitCode}. This indicates that Minecraft has encountered an error " +
                       $"and shut down. Verify that every mod is up to date, not duplicate, and compatible with each other";
        }

        BodyText.Text = bodyText;
    }

    public CrashPopup()
    {
        InitializeComponent();
    }

    public CrashPopup WithCustomLog(string log)
    {
        BodyText.Text = log;

        return this;
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