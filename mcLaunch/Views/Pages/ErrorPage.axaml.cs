using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace mcLaunch.Views.Pages;

public partial class ErrorPage : UserControl
{
    public ErrorPage()
    {
        InitializeComponent();
    }

    public ErrorPage(string message)
    {
        InitializeComponent();

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