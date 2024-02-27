using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class ConfirmMessageBoxPopup : UserControl
{
    private readonly Action? cancelCallback;
    private readonly Action confirmCallback;

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
        public Data(string title, string text)
        {
            Title = title;
            Text = text;
        }

        public string Title { get; set; }
        public string Text { get; set; }
    }
}