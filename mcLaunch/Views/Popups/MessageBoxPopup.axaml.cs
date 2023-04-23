using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class MessageBoxPopup : UserControl
{
    public MessageBoxPopup()
    {
        InitializeComponent();
    }
    
    public MessageBoxPopup(string title, string text)
    {
        InitializeComponent();

        DataContext = new Data(title, text);
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

    private void OKButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }
}