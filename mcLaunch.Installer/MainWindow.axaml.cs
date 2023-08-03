using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
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
        
        Parameters.SetDefaultTargetDirectory();
        
        pages = new Control[]
        {
            new WelcomePage(),
            new SelectFolderPage()
        };
        
        SetupPageContainer.Content = pages[0];
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