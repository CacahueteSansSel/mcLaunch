using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net.Http.Headers;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Cacahuete.MinecraftLib.Http;
using mcLaunch.GitHub;
using mcLaunch.GitHub.Models;
using mcLaunch.Installer.Core;
using mcLaunch.Installer.Pages;
using mcLaunch.Installer.Win32;

namespace mcLaunch.Installer;

public partial class MainWindow : Window
{
    public static MainWindow Instance { get; private set; }

    InstallerPage[] pages;
    int pageCounter = 0;

    public InstallerParameters Parameters { get; set; } = new();

    public MainWindow()
    {
        Instance = this;
        InitializeComponent();
        
        Api.SetUserAgent(new ProductInfoHeaderValue("mcLaunch.Installer", "1.0.0"));
        
        Parameters.SetDefaultTargetDirectory();

        FetchLatestVersion();
    }

    async void FetchLatestVersion()
    {
        GitHubRelease? release = await GitHubRepository.GetLatestReleaseAsync();
        if (release == null)
        {
            Environment.Exit(1);
            return;
        }
        
        Parameters.ReleaseToDownload = release;

        if (Environment.GetCommandLineArgs().Length == 2)
        {
            string updateDirectory = Environment.GetCommandLineArgs()[1];

            Parameters.RegisterInApplicationList = false;
            Parameters.PlaceShortcutOnDesktop = false;
            Parameters.TargetDirectory = updateDirectory;

            pages = new InstallerPage[]
            {
                new InstallationPage().SetUpdate(),
                new InstalledPage()
            };
            
            SetupPageContainer.Content = pages[0];
            pages[0].OnShow();
            
            return;
        }

        if (OperatingSystem.IsWindows())
        {
            pages = [
                new WelcomePage(),
                new SelectFolderPage(),
                new CheckboxesSettingsPage(),
                new InstallationPage(),
                new InstalledPage()
            ];
        }
        else
        {
            pages = [
                new WelcomePage(),
                new SelectFolderPage(),
                new InstallationPage(),
                new InstalledPage()
            ];
        }
        
        SetupPageContainer.Content = pages[0];
        pages[0].OnShow();

        Title = $"mcLaunch Installer (for mcLaunch {release.Name})";
    }

    public void HideBottomButtons()
    {
        PreviousButton.IsVisible = false;
        NextButton.IsVisible = false;
    }

    public void HidePreviousButtons()
    {
        PreviousButton.IsVisible = false;
        NextButton.IsVisible = true;
    }

    public void Previous()
    {
        if (pageCounter <= 0) return;
        
        pages[pageCounter].OnHide();

        pageCounter--;
        InstallerPage page = pages[pageCounter];
        SetupPageContainer.Content = page;
        page.IsVisible = true;
        
        PreviousButton.IsVisible = pageCounter > 0;
        
        page.OnShow();
    }

    public void Next()
    {
        if (pageCounter + 1 >= pages.Length)
        {
            pages[pageCounter].OnHide();
            Environment.Exit(0);
            return;
        }
        
        pages[pageCounter].OnHide();

        pageCounter++;
        InstallerPage page = pages[pageCounter];
        SetupPageContainer.Content = page;
        
        PreviousButton.IsVisible = true;
        NextButton.Content = pageCounter + 1 >= pages.Length ? "Finish" : "Next";
        
        page.OnShow();
    }

    private void NextButtonClicked(object? sender, RoutedEventArgs e)
    {
        Next();
    }

    private void PreviousButtonClicked(object? sender, RoutedEventArgs e)
    {
        Previous();
    }
}