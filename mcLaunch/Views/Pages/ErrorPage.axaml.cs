using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ReactiveUI;

namespace mcLaunch.Views.Pages;

public partial class ErrorPage : UserControl, ITopLevelPageControl
{
    public ErrorPage()
    {
        InitializeComponent();
    }

    public ErrorPage(string message)
    {
        InitializeComponent();

        Title = message;

        DataContext = new ErrorPageData
        {
            Text = message
        };
    }

    public string Title { get; }

    private void ExitLauncherButtonClicked(object? sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }

    public class ErrorPageData : ReactiveObject
    {
        public string Text { get; set; }
    }
}