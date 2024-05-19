using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Boxes;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;

namespace mcLaunch.Views.Popups;

public partial class CrashPopup : UserControl
{
    private Box? box;
    private string? fileToOpen;

    public CrashPopup(int exitCode, string boxId)
    {
        InitializeComponent();

        if (box == null) return;

        BoxCard.SetBox(box);

        PopulateAsync(boxId, exitCode);
    }

    public CrashPopup()
    {
        InitializeComponent();
    }

    private async void PopulateAsync(string boxId, int exitCode)
    {
        LoadingIcon.IsVisible = true;
        BodyText.IsVisible = false;

        box = (await BoxManager.LoadLocalBoxesAsync()).FirstOrDefault(b => b.Manifest.Id == boxId);

        string bodyText;
        string latestLogsPath = $"{box.Folder.CompletePath}/logs/latest.log";

        if (File.Exists(latestLogsPath))
        {
            fileToOpen = latestLogsPath;

            BodyText.IsVisible = false;
            OpenCrashReportButton.IsVisible = true;
        }
        else
        {
            bodyText =
                $"Minecraft has exited with code {exitCode}.\nThis indicates that Minecraft has encountered an error " +
                $"and shut down.\nVerify that every mod is up to date, not duplicate, and compatible with each other";

            BodyText.Text = bodyText;
            BodyText.IsVisible = true;
            OpenCrashReportButton.IsVisible = false;
        }

        LoadingIcon.IsVisible = false;
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

    private void OpenCrashReportButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (fileToOpen != null) PlatformSpecific.OpenFile(fileToOpen);
    }
}