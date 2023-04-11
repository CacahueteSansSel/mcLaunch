using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ddLaunch.Utilities;

namespace ddLaunch.Views.Popups;

public partial class ConfirmMessageBoxPopup : UserControl
{
    Action confirmCallback;
    Action? cancelCallback;
    
    public ConfirmMessageBoxPopup()
    {
        InitializeComponent();
    }

    public ConfirmMessageBoxPopup(string title, string text, Action confirmCallback, Action? cancelCallback = null)
    {
        InitializeComponent();

        this.confirmCallback = confirmCallback;
        this.cancelCallback = cancelCallback;
        DataContext = new Data(title, text);
    }

    private void YesButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
        confirmCallback.Invoke();
    }

    private void NoButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
        cancelCallback?.Invoke();
    }

    public class Data
    {
        public string Title { get; set; }
        public string Text { get; set; }

        public Data(string title, string text)
        {
            Title = title;
            Text = text;
        }
    }
}