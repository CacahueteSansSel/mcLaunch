using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace mcLaunch.Views.Pages;

public partial class ErrorPage : UserControl, ITopLevelPageControl
{
    public string Title { get; private set; }
    
    public ErrorPage()
    {
        InitializeComponent();
    }

    public ErrorPage(string message)
    {
        InitializeComponent();

        Title = message;

        DataContext = new Data()
        {
            Text = message
        };
    }

    private void ExitLauncherButtonClicked(object? sender, RoutedEventArgs e)
    {
        Environment.Exit(0);
    }

    public class Data : ReactiveObject
    {
        public string Text { get; set; }
    }
}