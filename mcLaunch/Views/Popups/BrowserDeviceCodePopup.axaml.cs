using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Cacahuete.MinecraftLib.Auth;
using mcLaunch.Utilities;

namespace mcLaunch.Views.Popups;

public partial class BrowserDeviceCodePopup : UserControl
{
    private BrowserLoginCallbackParameters parameters;

    public BrowserDeviceCodePopup()
    {
        InitializeComponent();
    }
    
    public BrowserDeviceCodePopup(BrowserLoginCallbackParameters parameters)
    {
        InitializeComponent();

        this.parameters = parameters;
        DataContext = parameters;
    }

    private async void CopyAndOpenButtonClicked(object? sender, RoutedEventArgs e)
    {
        await Application.Current.Clipboard.SetTextAsync(parameters.Code);

        CopyAndOpenButton.IsEnabled = false;
        CancelButton.IsEnabled = false;
        WaitingForMicrosoft.IsVisible = true;
        
        PlatformSpecific.OpenUrl(parameters.Url);
    }

    private void CancelButtonClicked(object? sender, RoutedEventArgs e)
    {
        Navigation.HidePopup();
    }
}