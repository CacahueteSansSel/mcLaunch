using Avalonia.Controls;
using Avalonia.Interactivity;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class MessageBoxPopup : UserControl
{
    public MessageBoxPopup()
    {
        InitializeComponent();

        DataContext = new MessageBoxPopupData("Hello, World !", "Lorem Ipsum");
    }

    public MessageBoxPopup(string title, string text, MessageStatus status)
    {
        InitializeComponent();

        DataContext = new MessageBoxPopupData(title, text);

        StatusError.IsVisible = status == MessageStatus.Error;
        StatusWarning.IsVisible = status == MessageStatus.Warning;
        StatusSuccess.IsVisible = status == MessageStatus.Success;
    }

    private void OKButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }

    public class MessageBoxPopupData
    {
        public MessageBoxPopupData(string title, string text)
        {
            Title = title;
            Text = text;
        }

        public string Title { get; set; }
        public string Text { get; set; }
    }
}

public enum MessageStatus
{
    None,
    Warning,
    Error,
    Success
}