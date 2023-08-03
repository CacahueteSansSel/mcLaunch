using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.Net.Http.Headers;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Cacahuete.MinecraftLib.Http;
using Cacahuete.MinecraftLib.Models.GitHub;
using mcLaunch.Installer.Core;
using mcLaunch.Installer.Pages;

namespace mcLaunch.Installer;

public partial class MainWindow : Window
{
    public static MainWindow Instance { get; private set; }

    Control[] pages;
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
        
        pages = new Control[]
        {
            new WelcomePage(),
            new SelectFolderPage(),
            new CheckboxesSettingsPage()
        };
        SetupPageContainer.Content = pages[0];

        Title = $"mcLaunch Installer (for mcLaunch {release.Name})";
    }

    public void Previous()
    {
        if (pageCounter <= 0) return;

        pageCounter--;
        Control page = pages[pageCounter];
        SetupPageContainer.Content = page;
        page.IsVisible = true;

        PreviousButton.IsVisible = pageCounter > 0;
    }

    public void Next()
    {
        if (pageCounter + 1 >= pages.Length) return;

        pageCounter++;
        Control page = pages[pageCounter];
        SetupPageContainer.Content = page;

        PreviousButton.IsVisible = true;
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