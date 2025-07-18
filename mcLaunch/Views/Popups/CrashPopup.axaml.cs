using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Core.Boxes;
using mcLaunch.Utilities;
using mcLaunch.Views.Pages;
using mcLaunch.Views.Windows;

namespace mcLaunch.Views.Popups;

public partial class CrashPopup : UserControl
{
    private Box? box;
    private string? customLog;
    private string? fileToOpen;
    private Process? javaProcess;

    public CrashPopup(int exitCode, string boxId, Process? javaProcess)
    {
        InitializeComponent();

        this.javaProcess = javaProcess;
        DebugButton.IsEnabled = javaProcess != null;
        
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
        ButtonsRow.IsEnabled = false;

        box = (await BoxManager.LoadLocalBoxesAsync(true, false)).FirstOrDefault(b => b.Manifest.Id == boxId);
        if (box == null) return;

        OpenBoxDetailsButton.IsEnabled = box.Manifest.Type != BoxType.Temporary;

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
            bodyText = customLog ??
                       $"Minecraft has exited with code {exitCode}.\nThis indicates that Minecraft has encountered an error " +
                       $"and shut down.\nVerify that every mod is up to date, not duplicate, and compatible with each other";

            BodyText.Text = bodyText;
            BodyText.IsVisible = true;
            OpenCrashReportButton.IsVisible = false;
        }

        ButtonsRow.IsEnabled = true;
        LoadingIcon.IsVisible = false;
        BoxCard.SetBox(box);
    }

    public CrashPopup WithCustomLog(string log)
    {
        customLog = log;
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

        BoxDetailsPage page = new(box!);
        Navigation.Push(page);

        page.Run();
    }

    private void OpenCrashReportButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (fileToOpen != null) PlatformSpecific.OpenFile(fileToOpen);
    }

    private void DebugButtonClicked(object? sender, RoutedEventArgs e)
    {
        if (javaProcess == null) 
            return;
        
        new DebugBoxCrashPopup(box!, javaProcess).ShowDialog(MainWindow.Instance);
    }
}