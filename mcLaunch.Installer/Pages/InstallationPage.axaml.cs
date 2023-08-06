using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Cacahuete.MinecraftLib.Download;
using mcLaunch.Installer.Core;

namespace mcLaunch.Installer.Pages;

public partial class InstallationPage : InstallerPage
{
    SoftwareInstaller installer;
    
    public InstallationPage()
    {
        InitializeComponent();

        installer = new SoftwareInstaller(MainWindow.Instance.Parameters);
        Downloader.OnDownloadProgressUpdate += DownloadProgressUpdate;
        installer.OnExtractionStarted += OnExtractionStarted;
    }

    public InstallationPage SetUpdate()
    {
        TitleText.Text = "Updating mcLaunch";
        installer.CopyInstaller = false;
        
        return this;
    }

    private void OnExtractionStarted()
    {
        StatusText.Text = "Extracting...";
        StatusBar.Value = 1f;
    }

    private void DownloadProgressUpdate(string file, float percent)
    {
        StatusBar.Value = (int) MathF.Round(percent * 100);
        StatusText.Text = $"Downloading {Path.GetFileName(file)}...";
    }

    public override async void OnShow()
    {
        base.OnShow();

        MainWindow.Instance.HideBottomButtons();

        try
        {
            await installer.InstallAsync();
        }
        catch (Exception e)
        {
            File.WriteAllText("error.log", e.ToString());
            Environment.Exit(1);
        }
    }
}