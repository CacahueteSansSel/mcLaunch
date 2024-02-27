using Avalonia.Controls;
using Avalonia.Interactivity;
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

    private void OKButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
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